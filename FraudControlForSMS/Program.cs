using System;
using System.Diagnostics;

public class GeminiAI{
    public string ExecutePythonScript(string prompt){
        try{

            var pythonInterpreter = "/Library/Frameworks/Python.framework/Versions/3.11/bin/python3";
            var pythonScript = "/Users/sarperardabakir/Desktop/fraud-control-for-sms/FraudControlForSMS/gemini_api.py";

            var psi = new ProcessStartInfo(){
                FileName = pythonInterpreter,
                Arguments = $"{pythonScript} \"{prompt}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using(var process = Process.Start(psi)){
                // Read the output
                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();

                // Wait for the process to exit
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    // Handle errors
                    throw new Exception($"Python script error: {error}");
                }

                return output.Trim();
            }

        }
        catch(Exception ex){
            throw new Exception($"Error executing Python script: {ex.Message}");
        }
    }

}
class Program
{
    static void Main()
    {
        var geminiAI = new GeminiAI();
        var prompt = "What is the weather like today?";
        var response = geminiAI.ExecutePythonScript(prompt);
        Console.WriteLine(response);
    }
}