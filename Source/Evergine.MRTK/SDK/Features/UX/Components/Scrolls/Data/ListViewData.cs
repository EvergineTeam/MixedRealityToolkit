// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using System;
using System.Collections.Generic;

namespace Evergine.MRTK.SDK.Features.UX.Components.Scrolls
{
    /// <summary>
    /// List view data.
    /// </summary>
    public class ListViewData
    {
        private int numColumns;
        private List<string[]> data;

        /// <summary>
        /// Gets the rows data.
        /// </summary>
        public List<string[]> Data => this.data;

        /// <summary>
        /// Gets the number of columns.
        /// </summary>
        public int NumColumns => this.numColumns;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListViewData"/> class.
        /// </summary>
        /// <param name="columns">Number of columns.</param>
        public ListViewData(int columns)
        {
            this.numColumns = columns;
            this.data = new List<string[]>();
        }

        /// <summary>
        /// Add a new row.
        /// </summary>
        /// <param name="values">string array.</param>
        /// <returns>Return the ListViewData.</returns>
        public ListViewData Add(params string[] values)
        {
            if (values.Length != this.numColumns)
            {
                throw new ArgumentException($"Number of columns must be {this.numColumns}");
            }

            this.data.Add(values);

            return this;
        }
    }
}
