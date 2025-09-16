using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ocpa.ro.application.Renderers;

public class ImagePopupDetails
{
    [JsonPropertyName("src")]
    public string Source { get; set; }

    [JsonPropertyName("width")]
    public string Width { get; set; }

    [JsonPropertyName("height")]
    public string Height { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }
}

public class ImagePopupRenderer : ContentRendererBase
{
    public override string BlockType => "imgpopup";

    protected override string RenderBlock(string block)
    {
        block = (block ?? "").TrimStart('{').TrimEnd('}');

        var imgPopupDetails = JsonSerializer.Deserialize<ImagePopupDetails>("{ " + block + " }");

        StringBuilder sb = new StringBuilder();

        bool hasTitle = imgPopupDetails?.Title?.Length > 0;

        if (hasTitle)
        {
            sb.AppendLine("<table cellpadding='0' cellspacing='0'>");
            sb.AppendLine("<tr>");
            sb.AppendLine("<td>");
        }

        sb.Append("<img ");

        if (imgPopupDetails?.Source?.Length > 0)
            sb.Append($" src='{imgPopupDetails.Source}'");

        if (imgPopupDetails?.Width?.Length > 0)
            sb.Append($" width='{imgPopupDetails.Width}'");

        if (imgPopupDetails?.Height?.Length > 0)
            sb.Append($" height='{imgPopupDetails.Height}'");

        if (hasTitle)
        {
            sb.AppendLine($" class='modal-popup-image' title='{imgPopupDetails.Title}' />");


            sb.AppendLine("</td>");
            sb.AppendLine("</tr>");

            sb.AppendLine("<tr>");
            sb.AppendLine("<td>");
            sb.AppendLine($"<label class='modal-popup-title'>{imgPopupDetails.Title}</h4>");
            sb.AppendLine("</td>");
            sb.AppendLine("</tr>");

            sb.AppendLine("</table>");
        }
        else
            sb.AppendLine(" class='modal-popup-image' />");

        return sb.ToString();
    }
}
