using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Gaia.Document;

/// <summary>
/// The Section of TemplateElement.
/// </summary>
public class Section : TemplateElement
{
    #region Constructors

    /// <summary>
    /// Create a new instance of the <see cref="Section"/> class.
    /// </summary>
    public Section()
    {

    }

    #endregion

    #region Property

    /// <summary>
    /// The title level.
    /// </summary>
    public int? Level { get; set; }

    /// <summary>
    /// The title of the <see cref="Section"/>.
    /// </summary>
    public TemplateElement Title { get; set; }

    /// <summary>
    /// The contents of the <see cref="Section"/>.
    /// </summary>
    public List<TemplateElement> Contents { get; set; }

    /// <summary>
    /// The ListModel.
    /// </summary>
    public ListModel? ListModel { get; set; }

    #endregion

    /// <inheritdoc />
    public override DocumentElement RenderModel(JToken model)
    {
        return new DocSection
        {
            Title = Title?.RenderModel(model),
            Contents = Contents?.Select(p => RenderElement(p, model)).ToList(),
            ListModel = ListModel,
            Styles = Styles,
            Editable = Editable,
            Deletable = Deletable,
            Deleted = Deleted
        };
    }
}