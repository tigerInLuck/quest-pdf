using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Gaia.Document;

/// <summary>
/// The Row of TemplateElement.
/// </summary>
public class Row : TemplateElement
{
    #region Property

    public List<RowItem> Items { get; set; }

    #endregion

    #region Methods

    /// <inheritdoc />
    public override DocumentElement RenderModel(JToken model)
    {
        Items?.ForEach(e => RenderElement(e, model));
        throw new System.NotImplementedException();
    }

    #endregion
}

public enum RowItemType
{
    Auto,
    Constant,
    Relative
}

public class RowItem : TemplateElement
{
    #region Property

    public RowItemType Type { get; set; }

    public TemplateElement Element { get; set; }

    #endregion

    #region Method

    /// <inheritdoc />
    public override DocumentElement RenderModel(JToken model)
    {
        Element.RenderModel(model);
        throw new System.NotImplementedException();
    }

    #endregion
}