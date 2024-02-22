using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Gaia.Document;

public class DocPageBreak : DocumentElement
{
    #region Method

    /// <inheritdoc />
    public override void RenderElement(IContainer container)
    {
        container.PageBreak();
    }

    #endregion
}