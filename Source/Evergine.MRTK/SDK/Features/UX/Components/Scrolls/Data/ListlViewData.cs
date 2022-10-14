using System;
using System.Collections.Generic;

namespace Evergine.MRTK.SDK.Features.UX.Components.Scrolls
{
    public class ListlViewData
    {
        private int numColumns;
        private List<string[]> data;

        public List<string[]> Data => this.data;

        public int NumColumns => this.numColumns;

        public ListlViewData(int columns)
        {
            this.numColumns = columns;
            this.data = new List<string[]>();
        }

        public ListlViewData Add(params string[] values)
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