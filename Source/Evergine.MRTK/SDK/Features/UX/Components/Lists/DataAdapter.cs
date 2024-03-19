// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

namespace Evergine.MRTK.SDK.Features.UX.Components.Lists
{
    /// <summary>
    /// Presents data to populate a <see cref="ListView"/> instance.
    /// </summary>
    public abstract class DataAdapter
    {
        /// <summary>
        /// Gets total number of elements.
        /// </summary>
        public abstract int Count { get; }

        /// <summary>
        /// Gets data object associated to a given row.
        /// </summary>
        /// <param name="rowIndex">Row index.</param>
        /// <returns>Associated object.</returns>
        public abstract object GetRowValue(int rowIndex);

        /// <summary>
        /// Gets index of a given value, if it exists.
        /// </summary>
        /// <param name="value">Search value.</param>
        /// <returns>Row index for a given value. If element is not part of adapter data, it returns -1.</returns>
        public abstract int IndexOf(object @value);

        /// <summary>
        /// Gets renderer instance to be used to draw column data for a given row.
        /// </summary>
        /// <param name="rowIndex">Row index.</param>
        /// <param name="columnIndex">Column index.</param>
        /// <returns>Rendering cell instance.</returns>
        public abstract CellRenderer GetRenderer(int rowIndex, int columnIndex);
    }
}
