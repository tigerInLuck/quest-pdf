using Newtonsoft.Json.Linq;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Gaia.Document.DocumentHelper;

namespace Gaia.Document;

/// <summary>
/// The document element.
/// </summary>
public abstract class DocumentElement
{
    #region Constructors

    static DocumentElement()
    {
        RegisterDocumentParts(typeof(DocumentElement).Assembly);
    }

    #endregion

    #region Property

    List<Style> styles;

    /// <summary>
    /// Element styles
    /// </summary>
    public List<Style> Styles
    {
        get
        {
            if (styles is { Count: > 0 })
            {
                if (DefaultStyles is { Count: > 0 })
                {
                    List<Style> combine = new(styles);
                    combine.AddRange(DefaultStyles.Where(s => styles.All(v => v.GetType() != s.GetType())));
                    return combine;
                }
                else
                {
                    return styles;
                }
            }
            else
            {
                return DefaultStyles;
            }
        }
        set
        {
            styles = value;
        }
    }

    /// <summary>
    /// The default style.
    /// </summary>
    public virtual List<Style> DefaultStyles { get; set; }

    /// <summary>
    /// Whether the node data can be edit.
    /// </summary>
    public bool? Editable { get; set; }
    
    /// <summary>
    /// Whether the node is deletable
    /// </summary>
    public bool? Deletable { get; set; }

    /// <summary>
    /// Whether the node is deleted
    /// </summary>
    public bool? Deleted { get; set; }

    #endregion

    #region Method

    /// <summary>
    /// Render the elment
    /// </summary>
    public virtual void RenderElement(IContainer container)
    {
    }

    /// <summary>
    /// Render the common style
    /// </summary>
    protected virtual IContainer RenderContainerStyle<T>(IContainer container, T style) where T : Style
    {
        switch (style)
        {
            case Width width:
                return container.Width(width.Value);
            case Height height:
                return container.Height(height.Value);
            case Alignment align:
                container = align.HorizontalAlign switch
                {
                    HorizontalAlignment.Left => container.AlignLeft(),
                    HorizontalAlignment.Center => container.AlignCenter(),
                    HorizontalAlignment.Right => container.AlignRight(),
                    _ => throw new ArgumentOutOfRangeException()
                };
                container = align.VerticalAlign switch
                {
                    VerticalAlignment.Top => container.AlignTop(),
                    VerticalAlignment.Middle => container.AlignMiddle(),
                    VerticalAlignment.Bottom => container.AlignBottom(),
                    _ => throw new ArgumentOutOfRangeException()
                };
                break;
            case Padding padding:
                if (padding.Value.Top != 0)
                    container = container.PaddingTop(padding.Value.Top);
                if (padding.Value.Bottom != 0)
                    container = container.PaddingBottom(padding.Value.Bottom);
                if (padding.Value.Left != 0)
                    container = container.PaddingLeft(padding.Value.Left);
                if (padding.Value.Right != 0)
                    container = container.PaddingRight(padding.Value.Right);
                break;
        }
        return container;
    }

    /// <summary>
    /// Dump the object to the JSON package.
    /// </summary>
    public JObject DumpData()
    {
        Type type = GetType();
        string typeName = type.Name.ToLower();

        JObject data = new()
        {
            // Gets Name
            {
                TYPE_HOLDER,
                typeName.StartsWith("document")? "document": typeName.StartsWith("doc") ? typeName[3..]: typeName

                //typeName.StartsWith("documententity")
                //    ? (typeName.Length == 8
                //        ? typeName
                //        : typeName[8..])
                //    : typeName.StartsWith("docentity")
                //        ? typeName[3..]
                //        : typeName
            }
        };

        // Gets the elements
        foreach (PropertyInfo propInfo in type.GetProperties().Where(p => p.CanRead && p.CanWrite))
        {
            if (propInfo.Name.Equals(nameof(Styles), StringComparison.OrdinalIgnoreCase)) continue;

            Type propType = propInfo.PropertyType;
            bool isList = false;

            // Check if the value is list
            if (propType.IsSubclassOfGenericType(typeof(List<>)))
            {
                isList = true;
                propType = propType.GetGenericArguments()[0];
            }

            // Check if the type is nullable
            if (propType.IsSubclassOfGenericType(typeof(Nullable<>)))
            {
                propType = propType.GetGenericArguments()[0];
            }

            // Elements
            if (propType == typeof(DocumentElement) || propType.IsSubclassOf(typeof(DocumentElement)))
            {
                if (isList)
                {
                    object elements = propInfo.GetValue(this);
                    if (elements is IList { Count: > 0 } list)
                    {
                        JArray array = new();
                        foreach (object o in list)
                        {
                            if (o is DocumentElement ele)
                            {
                                array.Add(ele.DumpData());
                            }
                        }
                        data.Add(propInfo.Name.ToLower(), array);
                    }
                }
                else
                {
                    DocumentElement element = (DocumentElement)propInfo.GetValue(this);
                    if (element != null)
                    {
                        data.Add(propInfo.Name.ToLower(), element.DumpData());
                    }
                }
            }
            // Values
            else if (propType.IsValueOrArrayValueType())
            {
                // Get the property value
                object value = propInfo.GetValue(this);
                if (value == null) continue;
                data.Add(propInfo.Name.ToLower(), JToken.FromObject(value));
            }
        }

        // Gets the styles data
        if (Styles is { Count: > 0 })
        {
            JObject s = new();
            Styles.ForEach(style => s.Add(style.GetType().Name.ToLower(), style.DumpData()));
            data.Add(STYLE_HOLDER, s);
        }

        return data;
    }

    /// <summary>
    /// Load object from the specific JSON package.
    /// </summary>
    public void LoadData(JObject data)
    {
        Type type = GetType();

        // Load Styles
        if (data.TryGetValue(STYLE_HOLDER, StringComparison.OrdinalIgnoreCase, out JToken token) && token is JObject styleCon)
        {
            List<Style> newStyleList = new();
            foreach ((string s, JToken val) in styleCon)
            {
                Type styleType = s.GetStyleType();
                if (styleType == null) continue;
                Style newStyle = (Style)Activator.CreateInstance(styleType);
                if (newStyle == null) continue;
                newStyle.LoadData(val);
                newStyleList.Add(newStyle);
            }
            Styles = newStyleList;
        }

        // Load Settings
        foreach (PropertyInfo propInfo in type.GetProperties().Where(p => p.CanRead && p.CanWrite))
        {
            if (propInfo.Name.Equals(nameof(Styles), StringComparison.OrdinalIgnoreCase)) continue;
            if (!data.TryGetValue(propInfo.Name.ToLower(), StringComparison.OrdinalIgnoreCase, out JToken valToken)) continue;

            Type propType = propInfo.PropertyType;
            bool isList = false;

            // Check if the value is list
            if (propType.IsSubclassOfGenericType(typeof(List<>)))
            {
                isList = true;
                propType = propType.GetGenericArguments()[0];
            }

            // Check if the type is nullable
            if (propType.IsSubclassOfGenericType(typeof(Nullable<>)))
            {
                propType = propType.GetGenericArguments()[0];
            }

            // Elements
            if (propType == typeof(DocumentElement) || propType.IsSubclassOf(typeof(DocumentElement)))
            {
                if (isList)
                {
                    if (valToken is not JArray array) continue;
                    IList elements = (IList)Activator.CreateInstance(propInfo.PropertyType);
                    foreach (JToken arr in array)
                    {
                        if (arr is not JObject eleData || !eleData.TryGetValue(TYPE_HOLDER, StringComparison.OrdinalIgnoreCase, out JToken eleName)) continue;
                        string eleTypeName = !eleName.ToString().StartsWith(PREFIX_DOC) ? $"{PREFIX_DOC}{eleName}" : eleName.ToString();
                        Type eleType = eleTypeName.GetDocumentElementType();
                        if (eleType == null) continue;
                        DocumentElement element = (DocumentElement)Activator.CreateInstance(eleType);
                        if (element == null) continue;
                        element.LoadData(eleData);
                        elements.Add(element);
                    }
                    propInfo.SetValue(this, elements);
                }
                else
                {
                    if (valToken is not JObject eleData || !eleData.TryGetValue(TYPE_HOLDER, StringComparison.OrdinalIgnoreCase, out JToken eleName)) continue;
                    string eleTypeName = !eleName.ToString().StartsWith(PREFIX_DOC) ? $"{PREFIX_DOC}{eleName}" : eleName.ToString();
                    Type eleType = eleTypeName.GetDocumentElementType();
                    if (eleType == null) continue;
                    DocumentElement element = (DocumentElement)Activator.CreateInstance(eleType);
                    if (element == null) continue;
                    element.LoadData(eleData);
                    propInfo.SetValue(this, element);
                }
            }
            // Values
            else if (propType.IsValueOrArrayValueType())
            {
                // Get the property value
                if (valToken != null)
                {
                    try
                    {
                        MethodInfo fromJson = fromJsonMap.GetOrAdd(propType, t =>
                        {
                            MethodInfo method = typeof(StringExtensions).GetMethod(nameof(StringExtensions.FromJson), BindingFlags.Static | BindingFlags.Public);
                            return method!.MakeGenericMethod(t);
                        });
                        propInfo.SetValue(this, fromJson.Invoke(null, new object[] { valToken.ToString(), JsonSerializationOptions.None }));
                    }
                    catch
                    {
                        throw new Exception($"Load style {type.Name} - {propInfo.Name} failed, can't be convert to a {propType.Name} data.");
                    }
                }
                /*else if (!isNullable)
                {
                    throw new Exception($"Load style {type.Name} - {propInfo.Name} failed, value is not provided.");
                }*/
            }
        }
    }

    #endregion

    #region Utility

    static readonly ConcurrentDictionary<Type, MethodInfo> fromJsonMap = new();

    #endregion
}