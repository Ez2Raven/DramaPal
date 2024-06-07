using System.Text;
using Microsoft.IdentityModel.Tokens;
using OpenAI.Chat;

namespace chatapp;

public static class ContentPartExtensions
{
    public static string toText(this ChatMessage chatMessage)
    {
        if (chatMessage.Content.IsNullOrEmpty())
        {
            return string.Empty;
        }

        StringBuilder contentBuilder = new StringBuilder();
        foreach (var item in chatMessage.Content)
        {
            contentBuilder.Append(item.Text);
        }

        return contentBuilder.ToString();
    }
}