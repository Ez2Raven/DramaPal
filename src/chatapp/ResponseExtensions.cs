using Markdig;

namespace chatapp;

public static class ResponseExtensions
{
    private static readonly MarkdownPipeline s_markdownPipeline = 
        new MarkdownPipelineBuilder()
            .ConfigureNewLine("\n")
            .UseAdvancedExtensions()
            .UseEmojiAndSmiley()
            .UseSoftlineBreakAsHardlineBreak()
            .Build();
    
    public static string ToHtml(this string markdown) => 
        !string.IsNullOrWhiteSpace(markdown) ? Markdown.ToHtml(markdown, s_markdownPipeline)
            : "";
}