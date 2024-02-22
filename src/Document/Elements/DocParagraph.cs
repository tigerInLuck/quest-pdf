using Newtonsoft.Json;
using QuestPDF.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;
using IContainer = QuestPDF.Infrastructure.IContainer;

// ReSharper disable NonReadonlyMemberInGetHashCode
namespace Gaia.Document;

/// <summary>
/// The Paragraph of DocumentElement.
/// </summary>
public class DocParagraph : DocumentElement, IEquatable<DocParagraph>
{
    #region Property

    /// <summary>
    /// Whether the head-line is Indent.
    /// </summary>
    public bool Indent { get; set; }

    /// <summary>
    /// The <see cref="DocSpan"/> list in the Paragraph.
    /// </summary>
    public List<DocSpan> Spans { get; set; }

    /// <summary>
    /// The full text.
    /// </summary>
    [JsonIgnore]
    public string FullText => string.Join("", Spans.Select(p => p.FullText));

    /// <summary>
    /// The paragraph is empty
    /// </summary>
    [JsonIgnore]
    public bool IsEmpty => Spans == null || Spans.Count == 0 || Spans.All(s => s.IsEmpty);

    #endregion

    #region Method

    /// <summary>
    /// Render the element to TextSpanDescriptor.
    /// </summary>
    public override void RenderElement(IContainer container)
    {
        container = Styles?.Aggregate(container, RenderContainerStyle) ?? container;

        container.Text(text =>
        {
            if (Indent) text.Element().Width(25);
            Spans?.ForEach(span => RenderSpan(text, span));
        });
    }

    /// <summary>
    /// Render the span
    /// </summary>
    public static void RenderSpan(TextDescriptor text, DocSpan span, bool sub = false, bool sup = false)
    {
        bool isSub = span.Subscript ?? false;
        bool isSup = span.Superscript ?? false;
        if (span.Spans is {Count: > 0})
        {
            span.Spans.ForEach(p => RenderSpan(text, p, isSub || sub, isSup || sup));
        }
        else
        {
            TextSpanDescriptor spanDesc = text.Span(span.ExpIndex is > 0 ? $"({span.ExpIndex})" : span.Value);

            if (isSub || sub) spanDesc = spanDesc.Subscript();
            if (isSup || sup) spanDesc = spanDesc.Superscript();

            spanDesc = span.Styles?.Aggregate(spanDesc, RenderExtension.RenderSpanStyle) ?? spanDesc;
            if (isSub && sub || isSup && sup)
                spanDesc.FontSize(8f);
        }
    }

    #endregion

    #region IEquatable

    /// <inheritdoc />
    public bool Equals(DocParagraph other) =>
        other is not null &&
        (ReferenceEquals(this, other) ||
         (Indent == other.Indent &&
          (Spans?.Count ?? 0) == (other.Spans?.Count ?? 0) &&
          (Spans is { Count: 0 } || Spans!.SequenceEqual(other.Spans!)))
        );

    /// <inheritdoc />
    public override bool Equals(object obj) => obj is not null && (ReferenceEquals(this, obj) || obj.GetType() == GetType() && Equals((DocParagraph)obj));

    /// <inheritdoc />
    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(Indent);
        Spans?.ForEach(s => hash.Add(s));
        return hash.ToHashCode();
    }

    public static bool operator ==(DocParagraph left, DocParagraph right) => Equals(left, right);

    public static bool operator !=(DocParagraph left, DocParagraph right) => !Equals(left, right);

    #endregion
}