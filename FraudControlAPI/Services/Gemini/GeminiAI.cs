using System.Diagnostics;
using System.Text.RegularExpressions;
using DotNetEnv;

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

            if (string.IsNullOrEmpty(pythonInterpreter) || string.IsNullOrEmpty(pythonScript))
            {
                throw new Exception("Environment variables for Python interpreter or Gemini script are not set.");
            }

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
                if (process == null)
                {
                    throw new Exception("Failed to start the process.");
                }

                var stopwatch = new Stopwatch();
                stopwatch.Start();
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
                        stopwatch.Stop();
                         Console.WriteLine("Total Execution Time of Gemini API: {0} ms", stopwatch.ElapsedMilliseconds);
                        return fraudProbability;
                    }
                    else
                    {
                        stopwatch.Stop();
                        Console.WriteLine("Total Execution Time of Gemini API: {0} ms", stopwatch.ElapsedMilliseconds);
                        return 0.0f;
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
