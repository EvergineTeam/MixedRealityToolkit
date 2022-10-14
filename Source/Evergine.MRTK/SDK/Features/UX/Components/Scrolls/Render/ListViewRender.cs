using Evergine.Mathematics;
using System;
using System.Collections.Generic;

namespace Evergine.MRTK.SDK.Features.UX.Components.Scrolls
{
    public class ListViewRender
    {
        private List<ListViewColumn> columns;

        public List<ListViewColumn> Columns => this.columns;

        private float currentSize = 0;

        public ListViewRender()
        {
            this.columns = new List<ListViewColumn>();
        }

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