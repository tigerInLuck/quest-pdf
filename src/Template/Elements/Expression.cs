using Newtonsoft.Json.Linq;

namespace Gaia.Document;

/// <summary>
/// The Expression of TemplateElement.
/// </summary>
public class Expression : TemplateElement
{
    #region Constructor

    public Expression()
    {

    }

    /// <summary>
    /// Create a new instance of the <see cref="Expression"/> class.
    /// </summary>
    public Expression(string id, TemplateElement element)
    {
        Id = id;
        Element = element;
    }

    #endregion

    #region Properties

    /// <summary>
    /// The Id.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// The ref element.
    /// </summary>
    public TemplateElement Element { get; set; }

    #endregion

    /// <inheritdoc />
    public override DocumentElement RenderModel(JToken model)
    {
        return new DocExpression
        {
            Id = Id,
            Element = RenderElement(Element, model),
            Styles = Styles,
            Editable = Editable
        };
    }
}