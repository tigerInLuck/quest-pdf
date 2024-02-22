using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Gaia.Document;

/// <summary>
/// The SingleRow of TemplateElement.
/// </summary>
public class SingleRow : TemplateElement
{
    #region Constructors

    /// <summary>
    /// Create a new instance of the <see cref="SingleRow"/> class.
    /// </summary>
    public SingleRow()
    {

    }

    #endregion

    #region Property

    /// <summary>
    /// The Elements of the <see cref="SingleRow"/>.
    /// </summary>
    public List<TemplateElement> Elements { get; set; }

    public float[] Widths { get; set; }

    #endregion

    /// <inheritdoc />
    public override DocumentElement RenderModel(JToken model)
    {
        return new DocSingleRow
        {
            Elements = Elements?.Select(p => RenderElement(p, model)).ToList(),
            Widths = Widths
        };
    }
}