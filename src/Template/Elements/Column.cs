using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Gaia.Document;

/// <summary>
/// The Column of TemplateElement.
/// </summary>
public class Column : TemplateElement
{
    #region Property

    /// <summary>
    /// The column items
    /// </summary>
    public List<DocumentElement> Items { get; set; }

    public float Spacing { get; set; }

    #endregion

    #region Method

    /// <inheritdoc />
    public override DocumentElement RenderModel(JToken model)
    {
        throw new System.NotImplementedException();
    }

    #endregion
}