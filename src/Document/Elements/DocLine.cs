using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Gaia.Document;

public class DocLine : DocumentElement
{
    #region Properties

    /// <summary>
    /// The Line type.
    /// </summary>
    public LineType LineType { get; set; }

    /// <summary>
    /// The value.
    /// </summary>
    public float Value { get; set; }

    /// <summary>
    /// The colour.
    /// </summary>
    public string Color { get; set; }

    #endregion

    #region Method

    /// <inheritdoc />
    public override void RenderElement(IContainer container)
    {
        if (LineType == LineType.Horizontal)
            container.LineHorizontal(Value).LineColor(Color);
        else
            container.LineVertical(Value).LineColor(Color);
    }

    #endregion
}