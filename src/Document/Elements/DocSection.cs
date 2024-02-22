using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable NonReadonlyMemberInGetHashCode
namespace Gaia.Document;

/// <summary>
/// The Section of DocumentElement.
/// </summary>
public class DocSection : DocumentElement, IEquatable<DocSection>
{
    #region Constructors

    /// <summary>
    /// Create a new instance of the <see cref="DocSection"/> class.
    /// </summary>
    public DocSection()
    {
    }

    #endregion

    #region Property

    /// <summary>
    /// The title level.
    /// </summary>
    public int? Level { get; set; }

    /// <summary>
    /// The section alias
    /// </summary>
    public string Alias { get; set; }

    /// <summary>
    /// The title of the <see cref="DocSection"/>.
    /// </summary>
    public DocumentElement Title { get; set; }

    /// <summary>
    /// The contents of the <see cref="DocSection"/>.
    /// </summary>
    public List<DocumentElement> Contents { get; set; }

    /// <summary>
    /// The ListModel.
    /// </summary>
    public ListModel? ListModel { get; set; }

    #endregion

    #region Method

    /// <inheritdoc />
    public override void RenderElement(IContainer container)
    {
        if (Contents is not { Count: > 0 }) return;
        container.Section(Alias).Column(column =>
        {
            // spacing
            column.Spacing(6);

            // Title
            column.Item().Element(c => Title?.RenderElement(c));
            if (Contents is not { Count: > 0 }) return;

            // Contents
            foreach (DocumentElement ele in Contents.Where(e => !(e.Deleted ?? false)))
            {
                column.Item().Element(i => ele.RenderElement(i));
            }
        });
    }

    #endregion

    #region IEquatable

    /// <inheritdoc />
    public bool Equals(DocSection other) =>
        other is not null &&
        (ReferenceEquals(this, other) || (Level == other.Level &&
                                          (Contents?.Count ?? 0) == (other.Contents?.Count ?? 0) &&
                                          (Contents is { Count: 0 } || Contents!.SequenceEqual(other.Contents!)) &&
                                          ListModel == other.ListModel)
        );

    /// <inheritdoc />
    public override bool Equals(object obj) => obj is not null && (ReferenceEquals(this, obj) || obj.GetType() == GetType() && Equals((DocSection)obj));

    /// <inheritdoc />
    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(ListModel);
        Contents?.ForEach(s => hash.Add(s));
        return hash.ToHashCode();
    }

    public static bool operator ==(DocSection left, DocSection right) => Equals(left, right);

    public static bool operator !=(DocSection left, DocSection right) => !Equals(left, right);

    #endregion
}