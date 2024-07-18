using System.Diagnostics;
using DotNetEnv;

public class TensorFlowModel
{
    public async Task<float> PredictAsync(string[] input, CancellationToken cancellationToken)
    {
        try
        {
            // Load environment variables from .env file
            Env.Load();

            var pythonInterpreter = Env.GetString("PYTHON_INTERPRETER");
            var pythonScript = Env.GetString("TENSORFLOW_SCRIPT");

            if (string.IsNullOrEmpty(pythonInterpreter) || string.IsNullOrEmpty(pythonScript))
            {
                throw new Exception("Environment variables for Python interpreter or TensorFlow script are not set.");
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
                        throw new Exception($"TensorFlow script error: {error}");
                    }

                    var output = await outputTask;
                    var lines = output.Trim().Split(Environment.NewLine); // Split by new line characters

                    // Find the line containing "Spam Probability"
                    string? spamLine = lines.FirstOrDefault(line => line.StartsWith("Spam Probability: "));

                    if (spamLine == null)
                    {
                        throw new Exception("Could not find 'Spam Probability' in model output.");
                    }

                    if (float.TryParse(spamLine.Split(':')[1].Trim(), out float probability))
                    {
                        stopwatch.Stop();
                        Console.WriteLine("Total Execution Time of TensorFlow API: {0} ms", stopwatch.ElapsedMilliseconds);
                        return probability;
                    }
                    else
                    {
                        throw new Exception("Failed to parse spam probability value.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error executing TensorFlow script: {ex.Message}");
        }
    }
}
