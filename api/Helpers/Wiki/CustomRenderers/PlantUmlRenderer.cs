namespace ocpa.ro.api.Helpers.Wiki.CustomRenderers;

public class PlantUmlRenderer : CustomRendererBase
{
    public override string BlockType => "plantuml";

    protected override string RenderBlock(string block)
    {
        var factory = new PlantUml.Net.RendererFactory();
        var renderer = factory.CreateRenderer();

        //var imageBytes = renderer.Render(block, PlantUml.Net.OutputFormat.Png);
        //var base64 = Convert.ToBase64String(imageBytes);
        //return $"<img src='data:image/png;base64, {base64}' />";

        var uri = renderer.RenderAsUri(block, PlantUml.Net.OutputFormat.Png);
        return $"<img src='{uri.ToString()}' />";
    }
}
