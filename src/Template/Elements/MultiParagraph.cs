using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Gaia.Document;

/// <summary>
/// The Paragraph of TemplateElement.
/// </summary>
public class MultiParagraph : TemplateElement
{
    #region Property

    /// <summary>
    /// The <see cref="Paragraph"/> list in the Paragraph.
    /// </summary>
    public List<Paragraph> Paragraphs { get; set; }

    #endregion

    /// <inheritdoc />
    public override DocumentElement RenderModel(JToken model)
    {
        List<DocParagraph> docParagraphs = new();
        if (Paragraphs is { Count: > 0 })
        {
            foreach (Paragraph p in Paragraphs)
            {
                JToken arrModel = GetElementData(model, p.Access);
                if (arrModel is JArray arr)
                {
                    foreach (JToken item in arr)
                    {
                        docParagraphs.Add((DocParagraph)p.RenderModel(item));
                    }
                }
                else
                {
                    docParagraphs.Add((DocParagraph)p.RenderModel(arrModel));
                }
            }
        }

        return new DocMultiParagraph
        {
            Paragraphs = docParagraphs,
            Editable = Editable,
            Deletable = Deletable,
            Deleted = Deleted
        };
    }
}