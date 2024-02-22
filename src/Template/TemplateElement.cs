using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Gaia.Document.DocumentHelper;

namespace Gaia.Document;

/// <summary>
/// The template element.
/// </summary>
public abstract class TemplateElement
{
    #region Constructors

    static TemplateElement()
    {
        RegisterDocumentParts(typeof(TemplateElement).Assembly);
    }

    #endregion

    #region Property

    /// <summary>
    /// The element access path like `Item.SubItemA`.
    /// </summary>
    public string Access { get; set; } = ".";

    /// <summary>
    /// The element styles.
    /// </summary>
    public List<Style> Styles { get; set; }

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

    #region Abstract Method

    /// <summary>
    /// Render by the model.
    /// </summary>
    public abstract DocumentElement RenderModel(JToken model);

    #endregion

    #region Method

    /// <summary>
    /// Render the element with model
    /// </summary>
    protected DocumentElement RenderElement(TemplateElement element, JToken model) => element?.RenderModel(GetElementData(model, element.Access));

    /// <summary>
    /// Add style.
    /// </summary>
    public TemplateElement AddStyle(Style style)
    {
        Styles ??= new List<Style>();
        Styles.Add(style);
        return this;
    }

    /// <summary>
    /// Add style.
    /// </summary>
    public TemplateElement AddStyles(IEnumerable<Style> style)
    {
        Styles ??= new List<Style>();
        Styles.AddRange(style);
        return this;
    }

    /// <summary>
    /// Get element data by access path.
    /// </summary>
    protected static JToken GetElementData(JToken model, string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return null;
        if (path.Equals(".")) return model;
        foreach (ReadOnlySpan<char> p in path.AsSpan().Split("."))
        {
            if (model is JObject obj && obj.TryGetValue(p.ToString(), StringComparison.OrdinalIgnoreCase, out JToken val))
            {
                model = val;
            }
            else
            {
                model = null;
                break;
            }
        }
        return model;
    }

    /// <summary>
    /// Dump the object to the JSON package.
    /// </summary>
    public JObject DumpData()
    {
        Type type = GetType();
        JObject data = new()
        {
            // Type Name
            { TYPE_HOLDER, type.Name.ToLower() }
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
            if (propType == typeof(TemplateElement) || propType.IsSubclassOf(typeof(TemplateElement)))
            {
                if (isList)
                {
                    object elements = propInfo.GetValue(this);
                    if (elements is IList { Count: > 0 } list)
                    {
                        JArray array = new();
                        foreach (object o in list)
                        {
                            if (o is TemplateElement ele)
                            {
                                array.Add(ele.DumpData());
                            }
                        }
                        data.Add(propInfo.Name.ToLower(), array);
                    }
                }
                else
                {
                    TemplateElement element = (TemplateElement)propInfo.GetValue(this);
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
            JObject styles = new();
            Styles.ForEach(style => styles.Add(style.GetType().Name.ToLower(), style.DumpData()));
            data.Add(STYLE_HOLDER, styles);
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
        if (data.TryGetValue(STYLE_HOLDER, StringComparison.OrdinalIgnoreCase, out JToken token) && token is JObject styles)
        {
            foreach ((string s, JToken val) in styles)
            {
                Type styleType = s.GetStyleType();
                if (styleType == null) continue;
                Style newStyle = (Style)Activator.CreateInstance(styleType);
                if (newStyle == null) continue;
                newStyle.LoadData(val);
                Styles ??= new List<Style>();
                Styles.Add(newStyle);
            }
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
            if (propType == typeof(TemplateElement) || propType.IsSubclassOf(typeof(TemplateElement)))
            {
                if (isList)
                {
                    if (valToken is not JArray array) continue;
                    IList elements = (IList)Activator.CreateInstance(propInfo.PropertyType);
                    foreach (JToken arr in array)
                    {
                        // Load element by type name
                        if (arr is not JObject eleData || !eleData.TryGetValue(TYPE_HOLDER, StringComparison.OrdinalIgnoreCase, out JToken eleName)) continue;
                        Type eleType = eleName.ToString().GetTemplateElementType();
                        if (eleType == null) continue;
                        TemplateElement element = (TemplateElement)Activator.CreateInstance(eleType);
                        if (element == null) continue;
                        element.LoadData(eleData);
                        elements.Add(element);
                    }
                    propInfo.SetValue(this, elements);
                }
                else
                {
                    // Load element by type name
                    if (valToken is not JObject eleData || !eleData.TryGetValue(TYPE_HOLDER, StringComparison.OrdinalIgnoreCase, out JToken eleName)) continue;
                    Type eleType = eleName.ToString().GetTemplateElementType();
                    if (eleType == null) continue;
                    TemplateElement element = (TemplateElement)Activator.CreateInstance(eleType);
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