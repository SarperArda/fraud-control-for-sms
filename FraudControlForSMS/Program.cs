using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using DotNetEnv;

public class GeminiAI
{
    public async Task<string> ExecutePythonScriptAsync(string[] input, CancellationToken cancellationToken)
    {
        try
        {
            // Load environment variables from .env file
            Env.Load();

            var pythonInterpreter = Env.GetString("PYTHON_INTERPRETER");
            var pythonScript = Env.GetString("PYTHON_SCRIPT");

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

class Program
{
    static async Task Main(string[] args)
    {
        var geminiAI = new GeminiAI();
        var tensorFlowModel = new TensorFlowModel();

        string[] input = new string[] { "my love today i will come to you." };

        var cts = new CancellationTokenSource();

        var geminiTask = geminiAI.ExecutePythonScriptAsync(input, cts.Token);
        var tensorFlowTask = tensorFlowModel.PredictAsync(input, cts.Token);

        try
        {
            await Task.WhenAll(geminiTask, tensorFlowTask);

            Console.WriteLine("GeminiAI Result:");
            Console.WriteLine(await geminiTask);

            Console.WriteLine("TensorFlow Model Result:");
            Console.WriteLine(await tensorFlowTask);
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
