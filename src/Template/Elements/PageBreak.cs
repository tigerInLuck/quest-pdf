using Newtonsoft.Json.Linq;

namespace Gaia.Document;

public class PageBreak : TemplateElement
{
    #region Method

    /// <inheritdoc />
    public override DocumentElement RenderModel(JToken model)
    {
        return new DocPageBreak()
        {
            Styles = Styles,
        };
    }

    #endregion
}