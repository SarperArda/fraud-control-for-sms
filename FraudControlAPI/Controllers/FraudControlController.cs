using Microsoft.AspNetCore.Mvc;
using CsvHelper;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

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
            using var reader = new StreamReader(file.OpenReadStream());
            using var csv = new CsvReader(reader, new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture));
            var inputRecords = csv.GetRecords<InputRecord>().ToList();

            var cancellationToken = new CancellationToken(); // Create a default CancellationToken

            var results = await _fraudControlService.AnalyzeMessagesAsync(inputRecords, cancellationToken);

            // Convert results to CSV format
            using var writer = new StringWriter();
            using var csvWriter = new CsvWriter(writer, new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture));
            csvWriter.WriteRecords(results);

            var resultContent = writer.ToString();
            var resultBytes = System.Text.Encoding.UTF8.GetBytes(resultContent);

            // Return CSV file as response
            return File(resultBytes, "text/csv", "results.csv");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
