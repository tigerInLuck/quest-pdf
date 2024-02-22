using System.Collections.Generic;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Gaia.Document;

/// <summary>
/// The Show section of DocumentElement.
/// </summary>
public class DocShow : DocumentElement
{
    #region Constructor

    public DocShow()
    {
    }

    public DocShow(string condition)
    {
        Condition = condition;
    }

    #endregion

    #region Property

    /// <summary>
    /// The contents
    /// </summary>
    public List<DocumentElement> Contents { get; set; }

    /// <summary>
    /// The condition
    /// </summary>
    public string Condition { get; set; }

    /// <summary>
    /// Whether show the section
    /// </summary>
    public bool IsVisible { get; set; } = true;

    #endregion

    #region Method

    /// <inheritdoc />
    public override void RenderElement(IContainer container)
    {
        if (IsVisible && Contents is { Count: > 0 })
        {
            container.Column(c =>
            {
                foreach (DocumentElement ele in Contents)
                {
                    c.Item().Element(i => ele.RenderElement(i));
                }
            });
        }
    }

    #endregion
}