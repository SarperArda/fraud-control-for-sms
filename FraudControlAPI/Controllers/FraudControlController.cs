using Microsoft.AspNetCore.Mvc;

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
        var cancellationToken = new CancellationTokenSource().Token;
        try
        {
            var results = await _fraudControlService.AnalyzeMessagesAsync(inputRecords, cancellationToken);
            return Ok(results);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
