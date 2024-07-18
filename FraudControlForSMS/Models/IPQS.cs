using System.Diagnostics;
using System.Text.RegularExpressions;
using DotNetEnv;

public class IPQS
{
    public async Task<float> CheckURL(string[] input, CancellationToken cancellationToken)
    {
        try
        {
            // Load environment variables from .env file
            Env.Load();

            var pythonInterpreter = Env.GetString("PYTHON_INTERPRETER");
            var pythonScript = Env.GetString("IPQS_SCRIPT");

            if (string.IsNullOrEmpty(pythonInterpreter) || string.IsNullOrEmpty(pythonScript))
            {
                throw new Exception("Environment variables for Python interpreter or IPQS script are not set.");
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
                        Console.WriteLine("Total Execution Time of IPQS API: {0} ms", stopwatch.ElapsedMilliseconds);
                        return fraudProbability;
                    }
                    else
                    {
                        // No match found, return -1
                        stopwatch.Stop();
                        Console.WriteLine("Total Execution Time of IPQS API: {0} ms", stopwatch.ElapsedMilliseconds);
                        return -1f;
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
