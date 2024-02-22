using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable CollectionNeverUpdated.Global

namespace Gaia.Document;

#region Table

public class Table : TemplateElement
{
    #region Property

    /// <summary>
    /// The table header rows
    /// </summary>
    public List<TableRow> Headers { get; set; }

    /// <summary>
    /// The table rows
    /// </summary>
    public List<TableRow> Rows { get; set; }

    #endregion

    #region Method

    /// <inheritdoc />
    public override DocumentElement RenderModel(JToken model) =>
        new DocTable()
        {
            Styles = Styles,
            Headers = Headers?.SelectMany(h => RenderRows(h, model)).ToList(),
            Rows = Rows?.SelectMany(r => RenderRows(r, model)).ToList()
        };

    // Render row
    List<DocTableRow> RenderRows(TableRow row, JToken model)
    {
        List<DocTableRow> rows = new ();
        bool disableAutoRowSpan = false;
        model = GetElementData(model, row.Access);

        if (model is JArray arr)
        {
            // Multi rows
            foreach (JToken data in arr)
            {
                List<DocTableRow> renderRows = RendRowCells(row, data);
                rows.AddRange(renderRows);
                if (renderRows.Count > 1) disableAutoRowSpan = true;
            }
        }
        else
        {
            // Single row
            List<DocTableRow> renderRows = RendRowCells(row, model);
            rows.AddRange(renderRows);
            if (renderRows.Count > 1) disableAutoRowSpan = true;
        }

        // auto row
        if(!disableAutoRowSpan)
            AutoRowSpan(row, rows);

        return rows;
    }

    void AutoRowSpan(TableRow row, List<DocTableRow> rows)
    {
        // Auto row span
        if (rows.Count > 1 && row.Cells?.FirstOrDefault(p => p.AutoRowSpan) != null)
        {
            for (int i = 1; i < rows.Count; i++)
            {
                DocTableRow curr = rows[i];
                DocTableRow prev = rows[i - 1];
                for (int j = 0; j < row.Cells.Count; j++)
                {
                    if (!row.Cells[j].AutoRowSpan) continue;
                    DocTableCell currCell = curr.Cells[j];
                    DocTableCell prevCell = prev.Cells[j];
                    if (currCell.RowSpan != 0 && (currCell.ColSpan ?? 1) == (prevCell.ColSpan ?? 1) && currCell.Element.Equals(prevCell.Element))
                    {
                        currCell.RowSpan = 0;
                        int k = i - 1;
                        while (prevCell.RowSpan == 0 && k > 0)
                        {
                            prevCell = rows[--k].Cells[j];
                        }
                        prevCell.RowSpan = (prevCell.RowSpan ?? 1) + 1;
                    }
                }
            }

            // Clear
            foreach (DocTableRow t in rows)
            {
                for (int j = 0; j < row.Cells.Count; j++)
                {
                    DocTableCell currCell = t.Cells[j];
                    if (currCell.RowSpan != 0 && currCell.ColSpan != 0) continue;
                    currCell.Element = null;
                    currCell.Styles = null;
                }
            }
        }

    }

    // Render row with non-JArray
    List<DocTableRow> RendRowCells(TableRow row, JToken model)
    {
        List<DocTableRow> rows = null;
        if (model is JObject && row.Cells is { Count: > 0 })
        {
            // Check if has complex cell
            int rowCount = 0;
            foreach (JToken cellData in row.Cells.Select(rowCell => GetElementData(model, rowCell.Access)))
            {
                if (cellData is JArray arr)
                {
                    rowCount = Math.Max(rowCount, arr.Count);
                }
            }

            // Only allow two array layer
            if (rowCount > 0)
            {
                // Pre-locate the rows
                rows = new List<DocTableRow>(rowCount);
                for (int i = 0; i < rowCount; i++)
                    rows.Add(new DocTableRow
                    {
                        Cells = new List<DocTableCell>(row.Cells.Count)
                    });

                // Generate Cells
                for (int i = 0; i < row.Cells.Count; i++)
                {
                    RowCell template = row.Cells[i];
                    JToken cellData = GetElementData(model, template.Access);
                    int rowIndex = 0;
                    if (cellData is JArray arr)
                    {
                        // Fill rows
                        for (; rowIndex < rowCount; rowIndex++)
                        {
                            rows[rowIndex].Cells.Add(arr.Count > rowIndex
                                ? (DocTableCell)template.RenderModel(arr[rowIndex])
                                : new DocTableCell
                                {
                                    ColSpan = template.ColSpan,
                                });
                        }
                    }
                    else
                    {
                        rows[rowIndex].Cells.Add((DocTableCell)template.RenderModel(cellData));

                        // fill empty cell
                        rows[rowIndex].Cells[i].RowSpan = rowCount - rowIndex;
                        for (++rowIndex; rowIndex < rowCount; rowIndex++)
                        {
                            rows[rowIndex].Cells.Add(new DocTableCell
                            {
                                RowSpan = 0,
                                ColSpan = template.ColSpan,
                            });
                        }
                    }
                }
            }
        }
        
        rows ??= new List<DocTableRow>
        {
            (DocTableRow) row.RenderModel(model)
        };

        // Auto col span
        if (row.Cells.Any(c => c.AutoCellMerge != AutoCellMerge.None))
        {
            foreach (DocTableRow r in rows)
            {
                for (int i = 0; i < row.Cells.Count; i++)
                {
                    DocTableCell currCell = r.Cells[i];
                    if (row.Cells[i].AutoCellMerge == AutoCellMerge.None || currCell.RowSpan == 0 || currCell.ColSpan == 0) continue;

                    // Check empty
                    DocumentElement element = currCell.Element;
                    if (element is not (null or DocParagraph { IsEmpty: true })) continue;

                    // Merge
                    DocTableCell mergeCell = null;
                    if (row.Cells[i].AutoCellMerge == AutoCellMerge.Left)
                    {
                        int j = i;
                        while (j > 0)
                        {
                            mergeCell = r.Cells[--j];
                            if (mergeCell.RowSpan == 0 || (mergeCell.RowSpan ?? 1) != (currCell.RowSpan ?? 1))
                            {
                                // Not mergable
                                mergeCell = null;
                                break;
                            }
                            else if ((mergeCell.ColSpan ?? 1) > 0)
                            {
                                //Mergable
                                break;
                            }
                            mergeCell = null;
                        }
                    }
                    else
                    {
                        int j = i + 1;
                        while (j < row.Cells.Count)
                        {
                            mergeCell = r.Cells[j++];
                            if (mergeCell.RowSpan == 0 || (mergeCell.RowSpan ?? 1) != (currCell.RowSpan ?? 1))
                            {
                                // Not mergable
                                mergeCell = null;
                                break;
                            }
                            else if ((mergeCell.ColSpan ?? 1) > 0)
                            {
                                //Mergable
                                break;
                            }
                            mergeCell = null;
                        }
                    }
                    if (mergeCell != null)
                    {
                        mergeCell.ColSpan = (mergeCell.ColSpan ?? 1) + (currCell.ColSpan ?? 1);
                        currCell.ColSpan = 0;
                    }
                }
            }
        }

        // Auto row span
        if (rows.Count > 1)
            AutoRowSpan(row, rows);

        return rows;
    }
    
    #endregion
}

#endregion

#region Row

/// <summary>
/// The Row of TemplateElement.
/// </summary>
public class TableRow : TemplateElement
{
    #region Property

    public List<RowCell> Cells { get; set; }
    
    #endregion

    #region Methods

    /// <inheritdoc />
    public override DocumentElement RenderModel(JToken model) =>
        new DocTableRow
        {
            Styles = Styles,
            Cells = Cells?.Select(c => (DocTableCell)RenderElement(c, model)).ToList()
        };

    #endregion
}

#endregion

#region Cell

/// <summary>
/// The Cell of TemplateElement.
/// </summary>
public class RowCell : TemplateElement
{
    #region Property

    /// <summary>
    /// The element in the <see cref="RowCell"/>.
    /// </summary>
    public TemplateElement Element { get; set; }

    /// <summary>
    /// Whether auto gen row span based on data
    /// </summary>
    public bool AutoRowSpan { get; set; }

    /// <summary>
    /// Whether auto gen col span if the next cell is empty
    /// </summary>
    public AutoCellMerge AutoCellMerge { get; set; } = AutoCellMerge.None;
    
    /// <summary>
    /// The RowSpan.
    /// </summary>
    public int? RowSpan { get; set; }

    /// <summary>
    /// The ColSpan.
    /// </summary>
    public int? ColSpan { get; set; }

    #endregion

    /// <inheritdoc />
    public override DocumentElement RenderModel(JToken model) =>
        new DocTableCell()
        {
            Styles = Styles,
            RowSpan = RowSpan,
            ColSpan = ColSpan,
            Element = RenderElement(Element, model)
        };
}

/// <summary>
/// Auto cell merge rule
/// </summary>
public enum AutoCellMerge
{
    None = 0,
    Left = 1,
    Right = 2,
}

#endregion