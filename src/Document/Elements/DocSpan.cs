using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using QuestPDF.Helpers;

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace Gaia.Document;

/// <summary>
/// The Span of DocumentElement.
/// </summary>
public class DocSpan : DocumentElement, IEquatable<DocSpan>
{
    #region Property

    /// <summary>
    /// The inner spans
    /// </summary>
    public List<DocSpan> Spans { get; set; }

    /// <summary>
    /// The text value in the Span.
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// Whether the node data is Subscript.
    /// </summary>
    public bool? Subscript { get; set; }

    /// <summary>
    /// Whether the node data is Superscript.
    /// </summary>
    public bool? Superscript { get; set; }

    /// <summary>
    /// The Expression Id.
    /// </summary>
    public string Exp { get; set; }

    /// <summary>
    /// The expression index.
    /// </summary>
    public int? ExpIndex { get; set; }

    /// <summary>
    /// The full text.
    /// </summary>
    [JsonIgnore]
    public string FullText => Spans is {Count: > 0} ? string.Join("", Spans.Select(p => p.FullText)) : Value;

    /// <summary>
    /// The span is empty
    /// </summary>
    [JsonIgnore]
    public bool IsEmpty => Spans is { Count: > 0 } ? Spans.TrueForAll(p => p.IsEmpty) : (string.IsNullOrEmpty(Value) && Exp == null);

    /// <inheritdoc />
    public override List<Style> DefaultStyles { get; set; } = new()
    {
        new FontFamily(DefaultFont.DEFAULT_FONT_FAMILY),
        new FontSize(DefaultFont.DEFAULT_FONT_SIZE),
        new FontColor(Colors.Black)
    };

    #endregion

    #region IEquatable

    /// <inheritdoc />
    public bool Equals(DocSpan other) =>
        other is not null &&
        (ReferenceEquals(this, other) ||
         (Spans is { Count: > 0 }
            ? (Spans.Count == (other.Spans?.Count ?? 0) && Spans.SequenceEqual(other.Spans!))
            : (Value == other.Value &&
              Editable == other.Editable &&
              Subscript == other.Subscript &&
              Superscript == other.Superscript &&
              Exp == other.Exp)
            )
         );

    /// <inheritdoc />
    public override bool Equals(object obj) => obj is not null && (ReferenceEquals(this, obj) || obj.GetType() == GetType() && Equals((DocSpan)obj));

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Value, Editable, Subscript, Superscript);

    public static bool operator ==(DocSpan left, DocSpan right) => Equals(left, right);

    public static bool operator !=(DocSpan left, DocSpan right) => !Equals(left, right);

    #endregion
}