using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// ReSharper disable NonReadonlyMemberInGetHashCode
namespace Gaia.Document;

/// <summary>
/// The Image of DocumentElement.
/// </summary>
public class DocImage : DocumentElement, IEquatable<DocImage>
{
    #region Constructors

    /// <summary>
    /// Create a new instance of the <see cref="DocImage"/> class.
    /// </summary>
    public DocImage()
    {
    }

    /// <summary>
    /// Create a new instance of the <see cref="DocImage"/> class.
    /// </summary>
    public DocImage(string source)
    {
        Source = source;
    }

    #endregion

    #region Properties

    /// <summary>
    /// The file path of the image.
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    /// The file stream of the image.
    /// </summary>
    public Stream ImageStream { get; set; }

    /// <summary>
    /// The byte array of the image.
    /// </summary>
    public byte[] ImageBytes { get; set; }

    /// <inheritdoc />
    public override List<Style> DefaultStyles { get; set; } = new()
    {
        new ImageScaling(ScalingSettings.FitWidth),
        new ImageCompression(ImageCompressionSettings.Best)
    };

    #endregion

    #region Method

    /// <inheritdoc />
    public override void RenderElement(IContainer container)
    {
        if (string.IsNullOrWhiteSpace(Source)) return;
        ImageBytes = ResourcesManager.GetImage(Source).GetAwaiter().GetResult();
        ImageDescriptor desc = container.Image(ImageBytes);
        Styles?.Aggregate(desc, RenderExtension.RenderImageStyle);
    }

    #endregion

    #region IEquatable

    /// <inheritdoc />
    public bool Equals(DocImage other) =>
        other is not null &&
        (ReferenceEquals(this, other) ||
         (Source == other.Source &&
          Editable == other.Editable)
        );

    /// <inheritdoc />
    public override bool Equals(object obj) => obj is not null && (ReferenceEquals(this, obj) || obj.GetType() == GetType() && Equals((DocImage)obj));

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Source, Editable);

    public static bool operator ==(DocImage left, DocImage right) => Equals(left, right);

    public static bool operator !=(DocImage left, DocImage right) => !Equals(left, right);

    #endregion
}