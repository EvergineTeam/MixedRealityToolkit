// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using System.Collections.Generic;
using System.Linq;

namespace Evergine.MRTK.SDK.Features.UX.Components.Lists
{
    /// <summary>
    /// Generic adapter for arrays and lists. It will be used to display data by
    /// a single column, using associated object .ToString() implementation for each row.
    /// </summary>
    /// <typeparam name="T">Data type.</typeparam>
    public class ArrayAdapter<T> : DataAdapter
    {
        private IList<T> data;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayAdapter{T}"/> class.
        /// </summary>
        /// <param name="data">Data to be displayed.</param>
        public ArrayAdapter(IList<T> data)
        {
            this.data = data;
        }

        /// <summary>
        /// Gets data.
        /// </summary>
        public IList<T> Data { get => this.data; }

        /// <inheritdoc/>
        public override int Count => this.data.Count();

        /// <inheritdoc/>
        public override object GetRowValue(int rowIndex) => this.GetTypedRowValue(rowIndex);

        /// <inheritdoc/>
        public override int IndexOf(object @value) => this.data.IndexOf((T)@value);

        /// <summary>
        /// Gets typed row value.
        /// </summary>
        /// <param name="rowIndex">Row index.</param>
        /// <returns>Typed row value.</returns>
        public virtual T GetTypedRowValue(int rowIndex) => this.data.ElementAt(rowIndex);

        /// <inheritdoc/>
        public override CellRenderer GetRenderer(int rowIndex, int columnIndex)
        {
            var textRenderer = TextCellRenderer.Instance;
            var element = this.GetRowValue(rowIndex);
            textRenderer.Text = element?.ToString();

            return textRenderer;
        }
    }
}
