using Newtonsoft.Json.Linq;

namespace Gaia.Document;

public class Catalog : TemplateElement
{
    #region Property

    /// <summary>
    /// The title
    /// </summary>
    public TemplateElement Title { get; set; }

    #endregion

    #region Method

    /// <inheritdoc />
    public override DocumentElement RenderModel(JToken model)
    {
        return new DocCatalog()
        {
            Styles = Styles,
            Title = RenderElement(Title, model)
        };
    }

    #endregion
}