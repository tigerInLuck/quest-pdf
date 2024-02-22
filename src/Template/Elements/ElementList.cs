using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Gaia.Document;

/// <summary>
/// The List of TemplateElement.
/// </summary>
public class ElementList : TemplateElement
{
    #region Constructors

    /// <summary>
    /// Create a new instance of the <see cref="ElementList"/> class.
    /// </summary>
    public ElementList()
    {

    }

    #endregion

    #region Property

    /// <summary>
    /// The elements in the List.
    /// </summary>
    public List<TemplateElement> Elements { get; set; }

    /// <summary>
    /// The list model.
    /// </summary>
    public ListModel ListModel { get; set; }

    #endregion

    /// <inheritdoc />
    public override DocumentElement RenderModel(JToken model)
    {
        return new DocList
        {
            Styles = Styles,
            Editable = Editable,
            ListModel = ListModel,
            Elements = Elements?.Select(p => RenderElement(p, model)).ToList()
        };
    }
}

public enum ListModel
{
    None = 0,

    Number = 1,

    UpperRoman = 2,

    LowerRoman = 3,

    Chinese = 4,

    UpperAlphabet = 5,

    LowerAlphabet = 6
}