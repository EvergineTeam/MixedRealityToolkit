// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

namespace Evergine.MRTK.SDK.Features.UX.Components.Lists
{
    /// <summary>
    /// List view column.
    /// </summary>
    public struct ListViewColumn
    {
        /// <summary>
        /// Gets or sets the column name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the percentage column size.
        /// </summary>
        public float percentageSize { get; set; }

        /// <summary>
        /// Gets or sets the cell renderer.
        /// </summary>
        public CellRenderer cellRenderer { get; set; }
    }
}
