using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using DotNetEnv;
using Sprache;


public class GeminiAI
{
    public async Task<float> ExecutePythonScriptAsync(string[] input, CancellationToken cancellationToken)
    {
        try
        {
            // Load environment variables from .env file
            Env.Load();

            var pythonInterpreter = Env.GetString("PYTHON_INTERPRETER");
            var pythonScript = Env.GetString("GEMINI_SCRIPT");

            var psi = new ProcessStartInfo()
            {
                FileName = pythonInterpreter,
                Arguments = $"{pythonScript} \"{string.Join(";", input)}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(psi))
            {
                using (cancellationToken.Register(() => process.Kill()))
                {
                    // Read the output
                    var outputTask = process.StandardOutput.ReadToEndAsync();
                    var errorTask = process.StandardError.ReadToEndAsync();

                    // Wait for the process to exit
                    await Task.WhenAny(Task.Run(() => process.WaitForExit(), cancellationToken), Task.Delay(Timeout.Infinite, cancellationToken));

                    if (process.ExitCode != 0)
                    {
                        // Handle errors
                        var error = await errorTask;
                        throw new Exception($"Python script error: {error}");
                    }

                    var output = await outputTask;
                    output = output.Trim();

                    // Use Regex to search for digits (0-9) and optional decimal point
                    Match match = Regex.Match(output, @"\d+\.?\d*");

                    if (match.Success)
                    {
                        // Extract the matched group and convert to float
                        float fraudProbability = float.Parse(match.Groups[0].Value);
                        return fraudProbability;
                    }
                    else
                    {
                        // No match found, return -1
                        return -1.0f;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error executing Python script: {ex.Message}");
        }
    }
}

public class TensorFlowModel
{
    public async Task<string> PredictAsync(string[] input, CancellationToken cancellationToken)
    {
        try
        {
            // Load environment variables from .env file
            Env.Load();

            var pythonInterpreter = Env.GetString("PYTHON_INTERPRETER");
            var pythonScript = Env.GetString("TENSORFLOW_SCRIPT");

            var psi = new ProcessStartInfo()
            {
                FileName = pythonInterpreter,
                Arguments = $"{pythonScript} \"{string.Join(";", input)}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(psi))
            {
                using (cancellationToken.Register(() => process.Kill()))
                {
                    // Read the output
                    var outputTask = process.StandardOutput.ReadToEndAsync();
                    var errorTask = process.StandardError.ReadToEndAsync();

                    // Wait for the process to exit
                    await Task.WhenAny(Task.Run(() => process.WaitForExit(), cancellationToken), Task.Delay(Timeout.Infinite, cancellationToken));

                    if (process.ExitCode != 0)
                    {
                        // Handle errors
                        var error = await errorTask;
                        throw new Exception($"TensorFlow script error: {error}");
                    }

                    var output = await outputTask;
                    return output.Trim();
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error executing TensorFlow script: {ex.Message}");
        }
    }
}
class IPQS{
    public async Task<string> CheckURL(string[] input, CancellationToken cancellationToken)
    {
        try
        {
            // Load environment variables from .env file
            Env.Load();

            var pythonInterpreter = Env.GetString("PYTHON_INTERPRETER");
            var pythonScript = Env.GetString("IPQS_SCRIPT");

            var psi = new ProcessStartInfo()
            {
                FileName = pythonInterpreter,
                Arguments = $"{pythonScript} \"{string.Join(";", input)}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(psi))
            {
                using (cancellationToken.Register(() => process.Kill()))
                {
                    // Read the output
                    var outputTask = process.StandardOutput.ReadToEndAsync();
                    var errorTask = process.StandardError.ReadToEndAsync();

                    // Wait for the process to exit
                    await Task.WhenAny(Task.Run(() => process.WaitForExit(), cancellationToken), Task.Delay(Timeout.Infinite, cancellationToken));

                    if (process.ExitCode != 0)
                    {
                        // Handle errors
                        var error = await errorTask;
                        throw new Exception($"Python script error: {error}");
                    }

                    var output = await outputTask;
                    return output.Trim();
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error executing Python script: {ex.Message}");
        }
    }
}
class Program
{
    static async Task Main(string[] args)
    {
        var geminiAI = new GeminiAI();
        var tensorFlowModel = new TensorFlowModel();
        var ipqs = new IPQS();

        string[] input = new string[] {"ALERT: Your bank account has been suspended. Call us to verify: 123-456-7890 and click https://pegasus.com"};

        var cts = new CancellationTokenSource();

        var geminiTask = geminiAI.ExecutePythonScriptAsync(input, cts.Token);
        var tensorFlowTask = tensorFlowModel.PredictAsync(input, cts.Token);

        string smsContent = input[0];
        var urlMatch = Regex.Match(smsContent, @"http[s]?://\S+");

        Task<string> ipqsTask = null;
        if (urlMatch.Success)
        {
            string url = urlMatch.Value;
            ipqsTask = ipqs.CheckURL(new string[] { url }, cts.Token);
        }
        try
        {
            if (ipqsTask != null)
            {
                await Task.WhenAll(geminiTask, tensorFlowTask, ipqsTask);
            }
            else
            {
                await Task.WhenAll(geminiTask, tensorFlowTask);
            }

            await Task.WhenAll(geminiTask, tensorFlowTask);
            Console.WriteLine("GeminiAI Result:");
            float Result = await geminiTask;
            Console.WriteLine(Result);

            Console.WriteLine("TensorFlow Model Result:");
            Console.WriteLine(await tensorFlowTask);

            if (ipqsTask != null)
            {
                Console.WriteLine("IPQS Check Result:");
                Console.WriteLine(await ipqsTask);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            cts.Cancel(); // Cancel any remaining tasks
        }
    }
}
