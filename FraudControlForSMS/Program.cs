﻿using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using CsvHelper;
using DotNetEnv;

class Program
{
    static async Task Main(string[] args)
    {
        // Load environment variables from .env file
        Env.Load();

        var geminiAI = new GeminiAI();
        var tensorFlowModel = new TensorFlowModel();
        var ipqs = new IPQS();

        string inputFilePath = Env.GetString("INPUT_FILE");
        string outputFilePath = Env.GetString("OUTPUT_FILE");

        var cts = new CancellationTokenSource();
        var stopwatch = new Stopwatch();

        try
        {
            var records = ReadCsv(inputFilePath);
            var uniqueRecords = FilterDuplicateMessages(records);
            var results = new ConcurrentBag<OutputRecord>();

            stopwatch.Start();
            var tasks = uniqueRecords.Select(async record =>
            {
                string smsContent = record.Message;
                var geminiTask = geminiAI.ExecutePythonScriptAsync(new string[] { smsContent }, cts.Token);
                var tensorFlowTask = tensorFlowModel.PredictAsync(new string[] { smsContent }, cts.Token);

                var urlMatch = Regex.Match(smsContent, @"http[s]?://\S+");
                Task<float> ipqsTask = null;
                if (urlMatch.Success)
                {
                    string url = urlMatch.Value;
                    ipqsTask = ipqs.CheckURL(new string[] { url }, cts.Token);
                }

                if (ipqsTask != null)
                {
                    await Task.WhenAll(geminiTask, tensorFlowTask, ipqsTask);
                }
                else
                {
                    await Task.WhenAll(geminiTask, tensorFlowTask);
                }

                float geminiScore = await geminiTask;
                float tensorFlowScore = (float)Math.Round(await tensorFlowTask * 100, 2);
                float ipqsScore = ipqsTask != null ? await ipqsTask : -1;

                if (geminiScore < 0 || tensorFlowScore < 0)
                {
                    geminiScore = Math.Max(geminiScore, 0);
                    tensorFlowScore = Math.Max(tensorFlowScore, 0);
                }
                if (geminiScore > 100 || tensorFlowScore > 100 || ipqsScore > 100)
                {
                    geminiScore = Math.Min(geminiScore, 100);
                    tensorFlowScore = Math.Min(tensorFlowScore, 100);
                    ipqsScore = Math.Min(ipqsScore, 100);
                }

                int finalScore = CalculateFinalScore(geminiScore, tensorFlowScore, ipqsScore);
                string explanation = GenerateExplanation(geminiScore, tensorFlowScore, ipqsScore, finalScore);

                results.Add(new OutputRecord
                {
                    Message = smsContent,
                    GeminiScore = geminiScore,
                    TensorFlowScore = tensorFlowScore,
                    IPQSScore = ipqsScore,
                    FinalScore = finalScore,
                    Explanation = explanation
                });
            }).ToArray();

            await Task.WhenAll(tasks);
            stopwatch.Stop();

            WriteCsv(outputFilePath, results.OrderBy(r => r.Message));
            Console.WriteLine("Total Execution Time: {0} ms", stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            cts.Cancel();
        }
    }

    static int CalculateFinalScore(float geminiScore, float tensorFlowScore, float ipqsScore)
    {
        if(ipqsScore == -1){
            return (int)((geminiScore * 0.5) + (tensorFlowScore * 0.5));
        }
        return (int)((geminiScore * 0.4) + (tensorFlowScore * 0.4) + (ipqsScore * 0.2));
    }

    static string GenerateExplanation(float geminiScore, float tensorFlowScore, float ipqsScore, int finalScore)
    {
        string riskLevel;

        if (finalScore >= 80)
        {
            riskLevel = "high";
        }
        else if (finalScore >= 50)
        {
            riskLevel = "moderate";
        }
        else
        {
            riskLevel = "low";
        }

        return $"The final fraud/spam risk score is {finalScore}, which indicates a {riskLevel} risk level.";
    }

    static List<InputRecord> ReadCsv(string filePath)
    {
        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        return csv.GetRecords<InputRecord>().ToList();
    }

    static List<InputRecord> FilterDuplicateMessages(List<InputRecord> records)
    {
        var uniqueMessages = new HashSet<string>();
        var uniqueRecords = new List<InputRecord>();

        foreach (var record in records)
        {
            if (uniqueMessages.Add(record.Message))
            {
                uniqueRecords.Add(record);
            }
        }

        return uniqueRecords;
    }

    static void WriteCsv(string filePath, IEnumerable<OutputRecord> records)
    {
        using var writer = new StreamWriter(filePath);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        csv.WriteRecords(records);
    }
}