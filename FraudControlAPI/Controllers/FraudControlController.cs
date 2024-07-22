using Microsoft.AspNetCore.Mvc;
using CsvHelper;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json;

[Route("api/[controller]")]
[ApiController]
public class FraudControlController : ControllerBase
{
    private readonly FraudControlService _fraudControlService;

    public FraudControlController(FraudControlService fraudControlService)
    {
        _fraudControlService = fraudControlService;
    }

    [HttpPost("analyze")]
    public async Task<IActionResult> AnalyzeMessages([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        try
        {
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            List<InputRecord> inputRecords;

            switch (fileExtension)
            {
                case ".csv":
                    using (var reader = new StreamReader(file.OpenReadStream()))
                    using (var csv = new CsvReader(reader, new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)))
                    {
                        inputRecords = csv.GetRecords<InputRecord>().ToList();
                    }
                    break;

                case ".json":
                    using (var reader = new StreamReader(file.OpenReadStream()))
                    {
                        var jsonContent = await reader.ReadToEndAsync();
                        inputRecords = JsonConvert.DeserializeObject<List<InputRecord>>(jsonContent);
                    }
                    break;

                default:
                    return BadRequest("Unsupported file format. Please upload a CSV or JSON file.");
            }

            var cancellationToken = new CancellationToken(); // Create a default CancellationToken

            var results = await _fraudControlService.AnalyzeMessagesAsync(inputRecords, cancellationToken);

            string resultContent;
            byte[] resultBytes;

            switch (fileExtension)
            {
                case ".csv":
                    using (var writer = new StringWriter())
                    using (var csvWriter = new CsvWriter(writer, new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)))
                    {
                        csvWriter.WriteRecords(results);
                        resultContent = writer.ToString();
                    }
                    resultBytes = System.Text.Encoding.UTF8.GetBytes(resultContent);
                    return File(resultBytes, "text/csv", "results.csv");

                case ".json":
                    resultContent = JsonConvert.SerializeObject(results);
                    resultBytes = System.Text.Encoding.UTF8.GetBytes(resultContent);
                    return File(resultBytes, "application/json", "results.json");

                default:
                    return StatusCode(500, "An error occurred while processing the file.");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
/*
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json;

[Route("api/[controller]")]
[ApiController]
public class FraudControlController : ControllerBase
{
    private readonly FraudControlService _fraudControlService;

    public FraudControlController(FraudControlService fraudControlService)
    {
        _fraudControlService = fraudControlService;
    }

    [HttpPost("analyze")]
    public async Task<IActionResult> AnalyzeMessages([FromBody] List<InputRecord> inputRecords)
    {
        if (inputRecords == null || !inputRecords.Any())
        {
            return BadRequest("No input records provided.");
        }

        try
        {
            var cancellationToken = new CancellationToken(); // Create a default CancellationToken

            var results = await _fraudControlService.AnalyzeMessagesAsync(inputRecords, cancellationToken);

            return Ok(results); // Return results as JSON
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
**/