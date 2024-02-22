using System;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace Gaia.Document;

/// <summary>
/// The DocumentElement of TemplateElement.
/// </summary>
public class DocExpression : DocumentElement, IEquatable<DocExpression>
{
    #region Constructor

    public DocExpression()
    {

    }

    /// <summary>
    /// Create a new instance of the <see cref="DocExpression"/> class.
    /// </summary>
    public DocExpression(string id, DocumentElement element)
    {
        Id = id;
        Element = element;
    }

    #endregion

    #region Properties

    /// <summary>
    /// The Id.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// The ref element.
    /// </summary>
    public DocumentElement Element { get; set; }

    /// <summary>
    /// The exp index, auto-gen
    /// </summary>
    public int Index { get; set; }

    #endregion

    #region Method

    /// <inheritdoc />
    public override void RenderElement(IContainer container)
    {
        container.Row(row =>
        {
            Element?.RenderElement(row.RelativeItem());
            row.ConstantItem(30).AlignMiddle().Text($"({Index})");
        });
    }

    #endregion

    #region IEquatable

    /// <inheritdoc />
    public bool Equals(DocExpression other) =>
        other is not null &&
        (ReferenceEquals(this, other) ||
         (Id == other.Id &&
          Element == other.Element &&
          Editable == other.Editable)
        );

    /// <inheritdoc />
    public override bool Equals(object obj) => obj is not null && (ReferenceEquals(this, obj) || obj.GetType() == GetType() && Equals((DocExpression)obj));

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Id, Element, Editable);

    public static bool operator ==(DocExpression left, DocExpression right) => Equals(left, right);

    public static bool operator !=(DocExpression left, DocExpression right) => !Equals(left, right);

    #endregion
}