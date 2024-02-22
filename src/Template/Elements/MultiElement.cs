using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Gaia.Document;

/// <summary>
/// The Multi element of TemplateElement.
/// </summary>
public class MultiElement : TemplateElement
{
    #region Property

    /// <summary>
    /// The <see cref="TemplateElement"/> list in the TemplateElement.
    /// </summary>
    public List<TemplateElement> Elements { get; set; }

    #endregion
    /// <inheritdoc />
    public override DocumentElement RenderModel(JToken model)
    {
        List<DocumentElement> docElements = new();
        if (Elements is { Count: > 0 })
        {
            foreach (TemplateElement element in Elements)
            {
                JToken arrModel = GetElementData(model, element.Access);
                if (arrModel is JArray arr)
                {
                    foreach (JToken item in arr)
                    {
                        docElements.Add(element.RenderModel(item));
                    }
                }
                else
                {
                    docElements.Add(element.RenderModel(arrModel));
                }
            }
        }

        return new DocMultiElement
        {
            Elements = docElements,
            Editable = Editable,
            Deletable = Deletable,
            Deleted = Deleted
        };
    }
}