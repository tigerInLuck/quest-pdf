using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Gaia.Document;

/// <summary>
/// The Span of TemplateElement.
/// </summary>
public class Span : TemplateElement
{
    #region Property

    /// <summary>
    /// The inner spans
    /// </summary>
    public List<Span> Spans { get; set; }

    /// <summary>
    /// The text value in the Span.
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// The format value of the <see cref="Value"/>.
    /// </summary>
    public string Format { get; set; } = "{0}";

    /// <summary>
    /// Whether the node data is Subscript.
    /// </summary>
    public bool? Subscript { get; set; }

    /// <summary>
    /// Whether the node data is Superscript.
    /// </summary>
    public bool? Superscript { get; set; }

    /// <summary>
    /// The Expression Id.
    /// </summary>
    public string Exp { get; set; }

    #endregion

    #region Method

    /// <inheritdoc />
    public override DocumentElement RenderModel(JToken model) => new DocSpan
    {
        // Common  
        Styles = Styles,
        Editable = Editable,

        // Span
        Exp = Exp,
        Subscript = Subscript,
        Superscript = Superscript,
        Spans = Spans is { Count: > 0 }
            ? Spans.Select(p => (DocSpan)RenderElement(p, model)).ToList()
            : IsComplexValue(Value)
                ? RenderComplexValue(Value)
                : IsComplexValue(model)
                    ? RenderComplexValue(model)
                    : null,
        Value = Spans is { Count: > 0 } || IsComplexValue(Value) || IsComplexValue(model) 
            ? null
            : !string.IsNullOrWhiteSpace(Value)
                ? Value
                : model is JValue { Value: not null } val
                    ? FormatValue(val)
                    : null,
    };

    bool IsComplexValue(JToken model) => string.IsNullOrWhiteSpace(Value) && model is JValue {Value: string str} && Regex.IsMatch(str, COMPLEX_PATTERN);
    bool IsComplexValue(string str) => !string.IsNullOrWhiteSpace(str) && Regex.IsMatch(str, COMPLEX_PATTERN);

    List<DocSpan> RenderComplexValue(JToken model) => RenderComplexValue(model.Value<string>());

    List<DocSpan> RenderComplexValue(string input)
    {
        List<DocSpan> docSpans = new();
        string[] splits = Regex.Split(input, COMPLEX_PATTERN, RegexOptions.IgnorePatternWhitespace);

        int startIndex = 0;
        int index = startIndex;
        string startTag = null;
        int tagDepth = 0;

        while (index < splits.Length)
        {
            string part = splits[index];
            if (Regex.IsMatch(part, COMPLEX_PATTERN))
            {
                bool isEndTag = part.StartsWith("</");
                string tag = part.Substring(isEndTag ? 2 : 1, part.Length - (isEndTag ? 3 : 2)).ToLower();

                if (isEndTag)
                {
                    if (tag.Equals(startTag))
                    {
                        tagDepth--;
                        if (tagDepth == 0)
                        {
                            string inner = string.Join(string.Empty, splits.Skip(startIndex).Take(index - startIndex));
                            if (!string.IsNullOrWhiteSpace(inner))
                            {
                                if (IsComplexValue(inner))
                                {
                                    docSpans.Add((DocSpan) new Span
                                    {
                                        Value = inner
                                    }.RenderModel(null));
                                }
                                else
                                {
                                    docSpans.Add(new DocSpan
                                    {
                                        Value = inner
                                    });
                                }

                                switch (tag)
                                {
                                    case "sub":
                                        docSpans.Last().Subscript = true;
                                        break;
                                    case "sup":
                                        docSpans.Last().Superscript = true;
                                        break;
                                }
                            }

                            startTag = null;
                            startIndex = index + 1;
                        }
                    }
                }
                else if(tagDepth == 0 || tag.Equals(startTag))
                {
                    if (tagDepth == 0)
                    {
                        // common
                        if (index > startIndex)
                        {
                            string common = string.Join(string.Empty, splits.Skip(startIndex).Take(index - startIndex));
                            if(!string.IsNullOrWhiteSpace(common)){
                                docSpans.Add(new DocSpan
                                {
                                    Value = common
                                });
                            }
                        }
                        startIndex = index + 1;
                    }

                    startTag = tag;
                    tagDepth++;
                }
            }
            index++;
        }

        // rest
        if (index > startIndex)
        {
            string common = string.Join(string.Empty, splits.Skip(startIndex).Take(index - startIndex));
            if (!string.IsNullOrWhiteSpace(common))
            {
                docSpans.Add(new DocSpan
                {
                    Value = common
                });
            }
        }

        return docSpans;
    }

    string FormatValue(JValue val)
    {
        if (val.Value is DateTime dataTime)
        {
            dataTime = dataTime.FromUtc();
            if (Regex.IsMatch(Format, @"{0:.*\|zh}"))
            {
                try
                {
                    return Regex.Replace(Format, @"{0:.*\|zh}", m =>
                    {
                        return Regex.Replace(string.Format(m.Value.Replace("|zh", ""), dataTime), @"\d+", d =>
                        {
                            int v = int.Parse(d.Value);
                            if (v > 99)
                            {
                                // Year
                                return Regex.Replace(d.Value, @"\d", s => numberMap[s.Value]);
                            }
                            else
                            {
                                // Month or day
                                return v < 10 ? numberMap[v.ToString()] : v < 20 ? $"十{numberMap[(v % 10).ToString()]}" : $"{numberMap[(v / 10).ToString()]}十{numberMap[(v % 10).ToString()]}";
                            }
                        });
                    });
                }
                catch
                {
                    //pass
                }
            }
            else
            {
                return string.Format(Format, dataTime);
            }
        }
        return string.Format(Format, val.Value);
    }

    static readonly Dictionary<string, string> numberMap = new()
    {
        { "0", "O" },
        { "1", "一" },
        { "2", "二" },
        { "3", "三" },
        { "4", "四" },
        { "5", "五" },
        { "6", "六" },
        { "7", "七" },
        { "8", "八" },
        { "9", "九" },
    };

    const string COMPLEX_PATTERN = @"(</?\w+>)";

    #endregion
}