using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using CsvHelper;
using DotNetEnv;

public class FraudControlService
{
    private readonly GeminiAI _geminiAI;
    private readonly TensorFlowModel _tensorFlowModel;
    private readonly IPQS _ipqs;

    public FraudControlService()
    {
        _geminiAI = new GeminiAI();
        _tensorFlowModel = new TensorFlowModel();
        _ipqs = new IPQS();
    }

    public async Task<IEnumerable<OutputRecord>> AnalyzeMessagesAsync(IEnumerable<InputRecord> inputRecords, CancellationToken cancellationToken)
    {
        var uniqueRecords = FilterDuplicateMessages(inputRecords.ToList());
        var results = new ConcurrentBag<OutputRecord>();
        var tasks = uniqueRecords.Select(async record =>
        {
            string smsContent = record.Message;
            var geminiTask = _geminiAI.ExecutePythonScriptAsync(new string[] { smsContent }, cancellationToken);
            var tensorFlowTask = _tensorFlowModel.PredictAsync(new string[] { smsContent }, cancellationToken);

            var urlMatch = Regex.Match(smsContent, @"http[s]?://\S+");
            Task<float>? ipqsTask = null;
            if (urlMatch.Success)
            {
                string url = urlMatch.Value;
                ipqsTask = _ipqs.CheckURL(new string[] { url }, cancellationToken);
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
        return results.OrderBy(r => r.Message);
    }

    private static int CalculateFinalScore(float geminiScore, float tensorFlowScore, float ipqsScore)
    {
        if (ipqsScore == -1)
        {
            return (int)((geminiScore * 0.5) + (tensorFlowScore * 0.5));
        }
        return (int)((geminiScore * 0.4) + (tensorFlowScore * 0.4) + (ipqsScore * 0.2));
    }

    private static string GenerateExplanation(float geminiScore, float tensorFlowScore, float ipqsScore, int finalScore)
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

    private static List<InputRecord> FilterDuplicateMessages(List<InputRecord> records)
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
}
