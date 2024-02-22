using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Gaia.Document;

/// <summary>
/// The Paragraph of TemplateElement.
/// </summary>
public class Paragraph : TemplateElement
{
    #region Constructors

    /// <summary>
    /// Create a new instance of the <see cref="Paragraph"/> class.
    /// </summary>
    public Paragraph()
    {

    }

    /// <summary>
    /// Create a new instance of the <see cref="Paragraph"/> class.
    /// </summary>
    public Paragraph(bool indent)
    {
        Indent = indent;
    }

    #endregion

    #region Property

    /// <summary>
    /// Whether the head-line is Indent.
    /// </summary>
    public bool Indent { get; set; }

    /// <summary>
    /// The <see cref="Span"/> list in the Paragraph.
    /// </summary>
    public List<Span> Spans { get; set; }

    #endregion

    /// <inheritdoc />
    public override DocumentElement RenderModel(JToken model) =>
        new DocParagraph
        {
            Spans = Spans?.Select(p => (DocSpan)RenderElement(p, model)).ToList(),
            Indent = Indent,
            Styles = Styles,
            Editable = Editable,
            Deletable = Deletable,
            Deleted = Deleted
        };
}