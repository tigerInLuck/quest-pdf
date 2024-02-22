using System.Collections.Generic;

namespace Gaia.Document;

/// <summary>
/// The document entity.
/// </summary>
public class DocumentEntity : DocumentElement
{
    #region Property

    /// <summary>
    /// The document header
    /// </summary>
    public DocumentElement Header { get; set; }

    /// <summary>
    /// The document contents.
    /// </summary>
    public List<DocumentElement> Contents { get; set; }

    /// <summary>
    /// The document footer
    /// </summary>
    public DocumentElement Footer { get; set; }

    #endregion
}