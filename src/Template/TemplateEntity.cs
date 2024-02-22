using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Gaia.Document;

/// <summary>
/// Represents a template structure of General Document.
/// </summary>
public class TemplateEntity : TemplateElement
{
    #region Property

    /// <summary>
    /// The document header
    /// </summary>
    public TemplateElement Header { get; set; }

    /// <summary>
    /// The document contents
    /// </summary>
    public List<TemplateElement> Contents { get; set; }

    /// <summary>
    /// The document footer
    /// </summary>
    public TemplateElement Footer { get; set; }

    #endregion

    #region Method

    /// <inheritdoc />
    public override DocumentElement RenderModel(JToken model) =>
        new DocumentEntity()
        {
            Styles = Styles,
            Header = RenderElement(Header, model),
            Contents = Contents?.Select(p => RenderElement(p, model)).ToList(),
            Footer = RenderElement(Footer, model),
        };

    #endregion
}