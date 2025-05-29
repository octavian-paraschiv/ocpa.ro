using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ocpa.ro.api.Helpers.Content.CustomRenderers;

public class ImagePopupDetails
{
    [JsonPropertyName("src")]
    public string Source { get; set; }

    [JsonPropertyName("width")]
    public string Width { get; set; }

    [JsonPropertyName("height")]
    public string Height { get; set; }
}

public class ImagePopupRenderer : CustomRendererBase
{
    public override string BlockType => "imgpopup";

    protected override string RenderBlock(string block)
    {
        block = (block ?? "").TrimStart('{').TrimEnd('}');

        var imgPopupDetails = JsonSerializer.Deserialize<ImagePopupDetails>("{ " + block + " }");

        StringBuilder sb = new StringBuilder();

        sb.Append("<img ");

        if (imgPopupDetails?.Source?.Length > 0)
            sb.Append($" src='{imgPopupDetails.Source}'");

        if (imgPopupDetails?.Width?.Length > 0)
            sb.Append($" width='{imgPopupDetails.Width}'");

        if (imgPopupDetails?.Height?.Length > 0)
            sb.Append($" height='{imgPopupDetails.Height}'");

        sb.Append(" class='modal-popup-image' />");

        return sb.ToString();
    }
}
