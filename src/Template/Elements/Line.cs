using Newtonsoft.Json.Linq;

namespace Gaia.Document;

/// <summary>
/// The Line
/// </summary>
public class Line : TemplateElement
{
    #region Constructors

    /// <summary>
    /// Create a new instance of the <see cref="Line"/> class.
    /// </summary>
    public Line()
    {

    }

    /// <summary>
    /// Create a new instance of the <see cref="Line"/> class.
    /// </summary>
    /// <param name="lineType">The Line type <see cref="LineType"/></param>
    /// <param name="value">The value</param>
    /// <param name="color">The colour</param>
    public Line(LineType lineType, float value, string color)
    {
        LineType = lineType;
        Value = value;
        Color = color;
    }

    #endregion

    #region Properties

    /// <summary>
    /// The Line type.
    /// </summary>
    public LineType LineType { get; set; }

    /// <summary>
    /// The value.
    /// </summary>
    public float Value { get; set; }

    /// <summary>
    /// The colour.
    /// </summary>
    public string Color { get; set; }

    #endregion

    #region Method

    /// <inheritdoc />
    public override DocumentElement RenderModel(JToken model)
    {
        return new DocLine
        {
            Styles = Styles,
            LineType = LineType,
            Value = Value,
            Color = Color,
        };
    }

    #endregion
}

/// <summary>
/// The Line type.
/// </summary>
public enum LineType
{
    /// <summary>
    /// Horizontal
    /// </summary>
    Horizontal = 1,

    /// <summary>
    /// Vertical
    /// </summary>
    Vertical = 2
}