// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using Evergine.Mathematics;
using System;
using System.Collections.Generic;

namespace Evergine.MRTK.SDK.Features.UX.Components.Scrolls
{
    /// <summary>
    /// List view renderer.
    /// </summary>
    public class ListViewRender
    {
        private List<ListViewColumn> columns;

        /// <summary>
        /// Gets the columns config.
        /// </summary>
        public List<ListViewColumn> Columns => this.columns;

        private float currentSize = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListViewRender"/> class.
        /// </summary>
        public ListViewRender()
        {
            this.columns = new List<ListViewColumn>();
        }

        /// <summary>
        /// Add a new column config.
        /// </summary>
        /// <param name="name">Column name.</param>
        /// <param name="size">Column percentage width.</param>
        /// <param name="cellRenderer">Column renderer.</param>
        /// <returns>Return the ListViewRender.</returns>
        public ListViewRender AddColumn(string name, float size, CellRenderer cellRenderer)
        {
            if (this.currentSize + size > 1.0f)
            {
                throw new ArgumentException("The all column sizes sum must be 1.0f");
            }

            this.columns.Add(new ListViewColumn()
            {
                Name = name,
                percentageSize = MathHelper.Clamp(size, 0.0f, 1.0f),
                cellRenderer = cellRenderer,
            });

            this.currentSize += size;

            return this;
        }
    }
}
