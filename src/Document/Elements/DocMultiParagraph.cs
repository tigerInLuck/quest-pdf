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
public class DocMultiParagraph : DocumentElement, IEquatable<DocMultiParagraph>
{
    #region Property

    /// <summary>
    /// The <see cref="DocParagraph"/> list in the DocParagraph.
    /// </summary>
    public List<DocParagraph> Paragraphs { get; set; }

    /// <summary>
    /// The paragraph is empty
    /// </summary>
    [JsonIgnore]
    public bool IsEmpty => Paragraphs == null || Paragraphs.Count == 0 || Paragraphs.All(s => s.IsEmpty);

    #endregion

    #region Method

    /// <summary>
    /// Render the element to TextSpanDescriptor.
    /// </summary>
    public override void RenderElement(IContainer container)
    {
        if (Paragraphs is not { Count: > 0 }) return;
        container.Column(column =>
        {
            // spacing
            column.Spacing(6);

            // Contents
            foreach (DocParagraph ele in Paragraphs.Where(e => !(e.Deleted ?? false)))
            {
                column.Item().Element(i => ele.RenderElement(i));
            }
        });
    }

    #endregion

    #region IEquatable

    /// <inheritdoc />
    public bool Equals(DocMultiParagraph other) =>
        other is not null &&
        (ReferenceEquals(this, other) ||
         ((Paragraphs?.Count ?? 0) == (other.Paragraphs?.Count ?? 0) &&
          (Paragraphs is { Count: 0 } || Paragraphs!.SequenceEqual(other.Paragraphs!)))
        );

    /// <inheritdoc />
    public override bool Equals(object obj) => obj is not null && (ReferenceEquals(this, obj) || obj.GetType() == GetType() && Equals((DocParagraph)obj));

    /// <inheritdoc />
    public override int GetHashCode()
    {
        HashCode hash = new();
        Paragraphs?.ForEach(s => hash.Add(s));
        return hash.ToHashCode();
    }

    public static bool operator ==(DocMultiParagraph left, DocMultiParagraph right) => Equals(left, right);

    public static bool operator !=(DocMultiParagraph left, DocMultiParagraph right) => !Equals(left, right);

    #endregion
}