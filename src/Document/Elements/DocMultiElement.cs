using Newtonsoft.Json;
using QuestPDF.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;
using IContainer = QuestPDF.Infrastructure.IContainer;

// ReSharper disable NonReadonlyMemberInGetHashCode
namespace Gaia.Document;

/// <summary>
/// The Multi element of DocumentElement.
/// </summary>
public class DocMultiElement : DocumentElement, IEquatable<DocMultiElement>
{
    #region Property

    /// <summary>
    /// The <see cref="DocumentElement"/> list in the DocumentElement.
    /// </summary>
    public List<DocumentElement> Elements { get; set; }

    /// <summary>
    /// The DocumentElement is empty
    /// </summary>
    [JsonIgnore]
    public bool IsEmpty => Elements == null || Elements.Count == 0;

    #endregion

    #region Method

    /// <summary>
    /// Render the element to Descriptor.
    /// </summary>
    public override void RenderElement(IContainer container)
    {
        if (Elements is not { Count: > 0 }) return;
        container.Column(column =>
        {
            // spacing
            column.Spacing(6);

            // Contents
            foreach (DocumentElement ele in Elements.Where(e => !(e.Deleted ?? false)))
            {
                column.Item().Element(i => ele.RenderElement(i));
            }
        });
    }

    #endregion

    #region IEquatable

    /// <inheritdoc />
    public bool Equals(DocMultiElement other) =>
        other is not null &&
        (ReferenceEquals(this, other) ||
         ((Elements?.Count ?? 0) == (other.Elements?.Count ?? 0) &&
          (Elements is { Count: 0 } || Elements!.SequenceEqual(other.Elements!)))
        );

    /// <inheritdoc />
    public override bool Equals(object obj) => obj is not null && (ReferenceEquals(this, obj) || obj.GetType() == GetType() && Equals((DocumentElement)obj));

    /// <inheritdoc />
    public override int GetHashCode()
    {
        HashCode hash = new();
        Elements?.ForEach(s => hash.Add(s));
        return hash.ToHashCode();
    }

    public static bool operator ==(DocMultiElement left, DocMultiElement right) => Equals(left, right);

    public static bool operator !=(DocMultiElement left, DocMultiElement right) => !Equals(left, right);

    #endregion
}