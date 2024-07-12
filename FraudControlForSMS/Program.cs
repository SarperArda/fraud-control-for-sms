using System.Diagnostics;
using System.Text.RegularExpressions;

class Program
{
    static async Task Main(string[] args)
    {
        
        var geminiAI = new GeminiAI();
        var tensorFlowModel = new TensorFlowModel();
        var ipqs = new IPQS();

        string[] input = new string[] {"This is your fly ticket https://flypgs.com"};

        var cts = new CancellationTokenSource();
        var stopwatch = new Stopwatch();

        var geminiTask = geminiAI.ExecutePythonScriptAsync(input, cts.Token);
        var tensorFlowTask = tensorFlowModel.PredictAsync(input, cts.Token);

        string smsContent = input[0];
        var urlMatch = Regex.Match(smsContent, @"http[s]?://\S+");

        Task<float> ipqsTask = null;
        if (urlMatch.Success)
        {
            string url = urlMatch.Value;
            ipqsTask = ipqs.CheckURL(new string[] { url }, cts.Token);
        }
        try
        {
            stopwatch.Start();
            if (ipqsTask != null)
            {
                await Task.WhenAll(geminiTask, tensorFlowTask, ipqsTask);
            }
            else
            {
                await Task.WhenAll(geminiTask, tensorFlowTask);
            }
            stopwatch.Stop();
            Console.WriteLine("Total Execution Time: {0} ms", stopwatch.ElapsedMilliseconds);

            float geminiScore = await geminiTask;
            float tensorFlowScore = await tensorFlowTask * 100;
            float ipqsScore = ipqsTask != null ? await ipqsTask : 0;

            int finalScore = CalculateFinalScore(geminiScore, tensorFlowScore, ipqsScore);
            Console.WriteLine($"Explanation: {GenerateExplanation(geminiScore, tensorFlowScore, ipqsScore, finalScore)}");
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
        // Example weighted average
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

        return $"The final fraud/spam risk score is {finalScore}, which indicates a {riskLevel} risk level.\n" +
            $"This score is derived from the combined results of three different detection mechanisms:\n\n" +
            $"- GeminiAI Score: {geminiScore}\n" +
            $"  GeminiAI is a generative AI model that analyzes the content of the message to detect any suspicious or spam-like patterns.\n\n" +
            $"- TensorFlow Model Score: {tensorFlowScore}\n" +
            $"  This score is produced by a machine learning model trained on a dataset of SMS messages. It evaluates the likelihood of the message being spam based on its content.\n\n" +
            $"- IPQS Check Score: {ipqsScore}\n" +
            $"  IPQS (IP Quality Score) assesses the URL included in the message (if any) for potential phishing, malware, or other fraudulent activities.\n\n" +
            $"Each of these components contributes to the overall assessment, providing a comprehensive analysis of the message's risk level. A higher final score suggests a greater likelihood of the SMS being spam or fraudulent.";
    }

}
