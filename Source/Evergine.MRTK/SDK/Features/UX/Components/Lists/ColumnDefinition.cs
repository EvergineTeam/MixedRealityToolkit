// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using Evergine.Common.Graphics;

namespace Evergine.MRTK.SDK.Features.UX.Components.Lists
{
    /// <summary>
    /// List view column.
    /// </summary>
    public class ColumnDefinition
    {
        /// <summary>
        /// Gets or sets the column title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the percentage column size.
        /// </summary>
        public float PercentageSize { get; set; }

        /// <summary>
        /// Gets or sets header text color.
        /// </summary>
        public Color HeaderTextColor { get; set; } = Color.DarkBlue;
    }
}
