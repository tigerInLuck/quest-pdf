using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable NonReadonlyMemberInGetHashCode
namespace Gaia.Document;

/// <summary>
/// The DocSingleRow of DocumentElement.
/// </summary>
public class DocSingleRow : DocumentElement, IEquatable<DocSingleRow>
{
    #region Constructors

    /// <summary>
    /// Create a new instance of the <see cref="DocSingleRow"/> class.
    /// </summary>
    public DocSingleRow()
    {
    }

    #endregion

    #region Property

    /// <summary>
    /// The contents of the <see cref="DocSingleRow"/>.
    /// </summary>
    public List<DocumentElement> Elements { get; set; }

    public float[] Widths { get; set; }

    #endregion

    #region Method

    /// <inheritdoc />
    public override void RenderElement(IContainer container)
    {
        container.Row(row =>
        {
            if (Elements is not { Count: > 0 }) return;

            // Contents
            for (int i = 0; i < Elements.Count; i++)
            {
                int index = i;
                float width = Widths != null && Widths.Length > index ? Widths[index] : 0f;
                switch (width)
                {
                    case > 1:
                        row.ConstantItem(width).Element(p => Elements[index].RenderElement(p));
                        break;
                    case > 0:
                        row.RelativeItem(width * 10).Element(p => Elements[index].RenderElement(p));
                        break;
                    default:
                        row.RelativeItem().Element(p => Elements[index].RenderElement(p));
                        break;
                }
            }
        });
    }

    #endregion

    #region IEquatable

    /// <inheritdoc />
    public bool Equals(DocSingleRow other) =>
        other is not null &&
        (ReferenceEquals(this, other) || ((Elements?.Count ?? 0) == (other.Elements?.Count ?? 0) &&
                                          (Elements is { Count: 0 } || Elements!.SequenceEqual(other.Elements!)))
        );

    /// <inheritdoc />
    public override bool Equals(object obj) => obj is not null && (ReferenceEquals(this, obj) || obj.GetType() == GetType() && Equals((DocSingleRow)obj));

    /// <inheritdoc />
    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(Elements);
        Elements?.ForEach(s => hash.Add(s));
        return hash.ToHashCode();
    }

    public static bool operator ==(DocSingleRow left, DocSingleRow right) => Equals(left, right);

    public static bool operator !=(DocSingleRow left, DocSingleRow right) => !Equals(left, right);

    #endregion
}