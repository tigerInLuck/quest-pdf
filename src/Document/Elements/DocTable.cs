using QuestPDF.Elements.Table;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Collections.Generic;
using System.Linq;

namespace Gaia.Document;

#region Table

/// <summary>
/// The Table of DocumentElement.
/// </summary>
public class DocTable : DocumentElement
{
    #region Property

    /// <summary>
    /// The table header rows
    /// </summary>
    public List<DocTableRow> Headers { get; set; }

    /// <summary>
    /// The table rows
    /// </summary>
    public List<DocTableRow> Rows { get; set; }

    #endregion

    /// <inheritdoc />
    public override void RenderElement(IContainer container)
    {
        // get max length of columns
        if (Rows is not { Count: > 0 }) return;
        int? columnCount = Rows?.Count > 0 ? Rows.Select(p => p.Cells?.Count ?? 0).Max() : 0;
        if (columnCount is null or 0) return;

        // create table
        container.Border(1, Unit.Mil).Table(table =>
        {
            table.ColumnsDefinition(column =>
            {
                for (int i = 0; i < columnCount; i++)
                {
                    column.RelativeColumn();
                }
            });

            // header
            if (Headers is { Count: > 0 })
            {
                foreach (DocTableRow row in Headers)
                {
                    if (row.Cells is not { Count: > 0 }) continue;
                    foreach (DocTableCell cell in row.Cells)
                    {
                        if (cell?.Element == null) continue;
                        if (cell.RowSpan == 0 || cell.ColSpan == 0) continue;
                        ITableCellContainer cellContainer = table.Cell();
                        if (cell.RowSpan is not null) cellContainer = cellContainer.RowSpan((uint)cell.RowSpan.Value);
                        if (cell.ColSpan is not null) cellContainer = cellContainer.ColumnSpan((uint)cell.ColSpan.Value);
                        cell.Element?.RenderElement(cellContainer.Border(1, Unit.Mil).Background(Colors.Grey.Lighten2));
                    }
                }
            }

            // rows
            foreach (DocTableRow row in Rows)
            {
                if (row.Cells is not { Count: > 0 }) continue;
                foreach (DocTableCell cell in row.Cells)
                {
                    if (cell?.Element == null) continue;
                    if (cell.RowSpan == 0 || cell.ColSpan == 0) continue;
                    ITableCellContainer cellContainer = table.Cell();
                    if (cell.RowSpan is not null) cellContainer = cellContainer.RowSpan((uint)cell.RowSpan.Value);
                    if (cell.ColSpan is not null) cellContainer = cellContainer.ColumnSpan((uint)cell.ColSpan.Value);
                    cell.Element?.RenderElement(cellContainer.Border(1, Unit.Mil));
                }
            }
        });
    }
}

#endregion

#region Row

/// <summary>
/// The Table Row of DocumentElement.
/// </summary>
public class DocTableRow : DocumentElement
{
    #region Property

    /// <summary>
    /// The Cells in a <see cref="DocTableRow"/>.
    /// </summary>
    public List<DocTableCell> Cells { get; set; }

    #endregion

    /// <inheritdoc />
    public override void RenderElement(IContainer container)
    {

    }
}

#endregion

#region Cell

/// <summary>
/// The Table Cell of DocumentElement.
/// </summary>
public class DocTableCell : DocumentElement
{
    #region Property

    /// <summary>
    /// The element in the <see cref="DocTableCell"/>.
    /// </summary>
    public DocumentElement Element { get; set; }

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
    public override void RenderElement(IContainer container)
    {
        Element?.RenderElement(container);
    }
}

#endregion