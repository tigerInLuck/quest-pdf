using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace Gaia.Document;

/// <summary>
/// The List of DocumentElement.
/// </summary>
public class DocList : DocumentElement, IEquatable<DocList>
{
    #region Constructors

    /// <summary>
    /// Create a new instance of the <see cref="DocList"/> class.
    /// </summary>
    public DocList()
    {

    }

    #endregion

    #region Property

    /// <summary>
    /// The elements in the List.
    /// </summary>
    public List<DocumentElement> Elements { get; set; }

    /// <summary>
    /// The list model.
    /// </summary>
    public ListModel ListModel { get; set; }

    #endregion

    #region Method

    /// <inheritdoc />
    public override void RenderElement(IContainer container)
    {
        if (Elements is not { Count: > 0 }) return;
        int n = 0;
        container.Column(column =>
        {
            foreach (DocumentElement docElement in Elements.Where(e => !(e.Deleted ?? false)))
            {
                column.Item().Row(row =>
                {
                    row.Spacing(5);
                    row.AutoItem().Text(ListModel == ListModel.Number ? $"{++n}" : "*");
                    docElement.RenderElement(row.RelativeItem());
                });
            }
        });
    }

    #endregion

    #region IEquatable

    /// <inheritdoc />
    public bool Equals(DocList other) =>
        other is not null &&
        (ReferenceEquals(this, other) || ((Elements?.Count ?? 0) == (other.Elements?.Count ?? 0) &&
                                          (Elements is { Count: 0 } || Elements!.SequenceEqual(other.Elements!)) &&
                                          ListModel == other.ListModel)
            );

    /// <inheritdoc />
    public override bool Equals(object obj) => obj is not null && (ReferenceEquals(this, obj) || obj.GetType() == GetType() && Equals((DocList)obj));

    /// <inheritdoc />
    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(ListModel);
        Elements?.ForEach(s => hash.Add(s));
        return hash.ToHashCode();
    }

    public static bool operator ==(DocList left, DocList right) => Equals(left, right);

    public static bool operator !=(DocList left, DocList right) => !Equals(left, right);

    #endregion
}