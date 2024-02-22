using System.Collections.Generic;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Gaia.Document;

public class DocCatalog : DocumentElement
{
    #region Property

    /// <summary>
    /// The title
    /// </summary>
    public DocumentElement Title { get; set; }

    /// <summary>
    /// The document
    /// </summary>
    public PdfDocument Document { get; set; }

    /// <summary>
    /// Auto-gen sections list
    /// </summary>
    public List<DocSection> Sections { get; set; }

    #endregion

    #region Method

    /// <inheritdoc />
    public override void RenderElement(IContainer container)
    {
        container.Column(column =>
        {
            // spacing
            column.Spacing(6);

            // Title
            column.Item().Element(c => Title?.RenderElement(c));
            if (Sections is not { Count: > 0 }) return;

            // Contents
            foreach (DocSection ele in Sections)
            {
                if (ele.Title is DocParagraph paragraph)
                {
                    column.Item().Element(i =>
                    {
                        i.Row(row =>
                        {
                            // Indent

                            // page name
                            row.RelativeItem().SectionLink(ele.Alias).Text(text =>
                            {
                                if (ele.Level is > 0)
                                    text.Element().Width(ele.Level.Value * 12);
                                paragraph.Spans?.ForEach(span => RenderSpan(text, span));
                            });

                            // page num
                            row.ConstantItem(30).AlignMiddle().SectionLink(ele.Alias).Text(text =>
                            {
                                text.BeginPageNumberOfSection(ele.Alias).Format(Document.FormatPageNo);
                            });
                        });
                    });
                }
            }
        });
    }

    /// <summary>
    /// Render the span
    /// </summary>
    public static void RenderSpan(TextDescriptor text, DocSpan span, bool sub = false, bool sup = false)
    {
        bool isSub = span.Subscript ?? false;
        bool isSup = span.Superscript ?? false;
        if (span.Spans is { Count: > 0 })
        {
            span.Spans.ForEach(p => RenderSpan(text, p, isSub || sub, isSup || sup));
        }
        else
        {
            TextSpanDescriptor spanDesc = text.Span(span.ExpIndex is > 0 ? $"({span.ExpIndex})" : span.Value);

            if (isSub || sub) spanDesc = spanDesc.Subscript();
            if (isSup || sup) spanDesc = spanDesc.Superscript();

            spanDesc = span.DefaultStyles?.Aggregate(spanDesc, RenderExtension.RenderSpanStyle) ?? spanDesc;
            if (isSub && sub || isSup && sup)
                spanDesc.FontSize(8f);
        }
    }

    #endregion
}