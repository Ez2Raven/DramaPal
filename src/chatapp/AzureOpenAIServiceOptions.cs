namespace chatapp;

public class AzureOpenAIServiceOptions
{
    public const string Position = "AzureOpenAIService";
    public string Endpoint { get; set; } = string.Empty;
    public string DeploymentOrModelName { get; set; } = string.Empty;
    public List<string> InitialPrompt { get; set; } = [];
}