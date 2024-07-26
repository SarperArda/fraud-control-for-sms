using System.Diagnostics;
using System.Text.RegularExpressions;
using DotNetEnv;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

public class OpenAI
{
    // Define the desired categories
    private readonly HashSet<string> _desiredCategories = new HashSet<string> 
    { 
        "eticaret","kampanya", "hukuki", "finans", "otp", "diğer" 
    };

    public async Task<(float, string)> ExecutePythonScriptAsync(string[] input, CancellationToken cancellationToken)
    {
        try
        {
            // Load environment variables from .env file
            Env.Load();

            var pythonInterpreter = Env.GetString("PYTHON_INTERPRETER");
            var pythonScript = Env.GetString("OPENAI_SCRIPT");

            if (string.IsNullOrEmpty(pythonInterpreter) || string.IsNullOrEmpty(pythonScript))
            {
                throw new Exception("Environment variables for Python interpreter or OpenAI script are not set.");
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

                    // Improved regex to capture percentage and category
                    var match = Regex.Match(output, @"%(\d+\.?\d*)\s+(\w+[\w\s]*)");

                    if (match.Success)
                    {
                        // Extract the matched groups and convert to float and string
                        float fraudProbability = float.Parse(match.Groups[1].Value);
                        string category = match.Groups[2].Value.Trim().ToLower(); // Ensure case insensitivity

                        // Check if the category is in the desired categories
                        if (_desiredCategories.Contains(category))
                        {
                            stopwatch.Stop();
                            Console.WriteLine("Total Execution Time of OpenAI API: {0} ms", stopwatch.ElapsedMilliseconds);
                            return (fraudProbability, category);
                        }
                        else
                        {
                            stopwatch.Stop();
                            Console.WriteLine("Total Execution Time of OpenAI API: {0} ms", stopwatch.ElapsedMilliseconds);
                            return (fraudProbability, output); // For non-matching categories
                        }
                    }
                    else
                    {
                        stopwatch.Stop();
                        Console.WriteLine("Total Execution Time of OpenAI API: {0} ms", stopwatch.ElapsedMilliseconds);
                        return (0.0f, output); // When output does not match the expected format
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
