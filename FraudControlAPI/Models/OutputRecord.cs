public class OutputRecord
{
    public string Message { get; set; } = string.Empty;
    public float GeminiScore { get; set; }
    public float TensorFlowScore { get; set; }
    public float IPQSScore { get; set; }
    public float OpenAIScore { get; set; }
    public int FinalScore { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
}