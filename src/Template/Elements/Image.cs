using Newtonsoft.Json.Linq;

namespace Gaia.Document;

/// <summary>
/// The Image of TemplateElement.
/// </summary>
public class Image : TemplateElement
{
    #region Constructors

    /// <summary>
    /// Create a new instance of the <see cref="Image"/> class.
    /// </summary>
    public Image()
    {

    }

    /// <summary>
    /// Create a new instance of the <see cref="Image"/> class.
    /// </summary>
    public Image(string path)
    {
        Path = path;
    }

    #endregion

    #region Properties

    /// <summary>
    /// The file path of the image.
    /// </summary>
    public string Path { get; set; }

    #endregion

    #region Method

    /// <inheritdoc />
    public override DocumentElement RenderModel(JToken model)
    {
        return new DocImage
        {
            Styles = Styles,
            Editable = Editable,
            Source = Path ?? model?.Value<string>()
        };
    }

    #endregion
}