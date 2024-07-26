using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
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
        var openAI = new OpenAI();

        string inputFilePath = Env.GetString("INPUT_FILE") ?? throw new ArgumentNullException("INPUT_FILE environment variable not found");
        string outputFilePath = Env.GetString("OUTPUT_FILE") ?? throw new ArgumentNullException("OUTPUT_FILE environment variable not found");

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
                var openAITask = openAI.ExecutePythonScriptAsync(new string[] { smsContent }, cts.Token);
                var tensorFlowTask = tensorFlowModel.PredictAsync(new string[] { smsContent }, cts.Token);

                var urlMatch = Regex.Match(smsContent, @"http[s]?://\S+");
                Task<float>? ipqsTask = null;
                if (urlMatch.Success)
                {
                    string url = urlMatch.Value;
                    ipqsTask = ipqs.CheckURL(new string[] { url }, cts.Token);
                }

                if (ipqsTask != null)
                {
                    await Task.WhenAll(geminiTask, tensorFlowTask, ipqsTask,openAITask);
                }
                else
                {
                    await Task.WhenAll(geminiTask, tensorFlowTask,openAITask);
                }

                float geminiScore = await geminiTask;
                var (openAIScore,category) = await openAITask;
                float tensorFlowScore = (float)Math.Round(await tensorFlowTask * 100, 2);
                float ipqsScore = ipqsTask != null ? await ipqsTask : -1;

                if (geminiScore < 0 || tensorFlowScore < 0 || openAIScore < 0)
                {
                    geminiScore = Math.Max(geminiScore, 0);
                    tensorFlowScore = Math.Max(tensorFlowScore, 0);
                    openAIScore = Math.Max(openAIScore, 0);
                }
                if (geminiScore > 100 || tensorFlowScore > 100 || ipqsScore > 100 || openAIScore > 100)
                {
                    geminiScore = Math.Min(geminiScore, 100);
                    tensorFlowScore = Math.Min(tensorFlowScore, 100);
                    ipqsScore = Math.Min(ipqsScore, 100);
                    openAIScore = Math.Min(openAIScore, 100);
                }

                int finalScore = CalculateFinalScore(geminiScore, tensorFlowScore, ipqsScore, openAIScore);
                string explanation = GenerateExplanation(finalScore);

                results.Add(new OutputRecord
                {
                    Message = smsContent,
                    GeminiScore = geminiScore,
                    TensorFlowScore = tensorFlowScore,
                    IPQSScore = ipqsScore,
                    OpenAIScore = openAIScore,
                    FinalScore = finalScore,
                    Category = category,
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

    static int CalculateFinalScore(float geminiScore, float tensorFlowScore, float ipqsScore, float openAIScore)
    {
        if (ipqsScore == -1)
        {
            return (int)((geminiScore * 0.3) + (tensorFlowScore * 0.3) + (openAIScore * 0.5));
        }
        return (int)((geminiScore * 0.3) + (tensorFlowScore * 0.2) + (ipqsScore * 0.1) + (openAIScore * 0.4));
    }

    static string GenerateExplanation(int finalScore)
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
