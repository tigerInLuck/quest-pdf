using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Gaia.Document;

/// <summary>
/// The show section of TemplateElement.
/// </summary>
public class Show : TemplateElement
{
    #region Constructor

    public Show()
    {
    }

    public Show(string condition)
    {
        Condition = condition;
    }

    #endregion

    #region Property

    /// <summary>
    /// The contents
    /// </summary>
    public List<TemplateElement> Contents { get; set; }

    /// <summary>
    /// The condition
    /// </summary>
    public string Condition { get; set; }

    /// <summary>
    /// Whether show the section
    /// </summary>
    public bool IsVisible { get; set; } = true;

    #endregion

    #region Method

    /// <inheritdoc />
    public override DocumentElement RenderModel(JToken model)
    {
        if (Condition != null)
        {
            if (model.GetObjectField(Condition) is bool isVisible)
            {
                IsVisible = isVisible;
            }
            else
            {
                IsVisible = false;
            }
        }
        else
        {
            IsVisible = true;
        }

        // Only render contents if needed
        if (IsVisible)
        {
            Contents?.ForEach(c => RenderElement(c, model));
        }

        return null;
    }

    #endregion
}