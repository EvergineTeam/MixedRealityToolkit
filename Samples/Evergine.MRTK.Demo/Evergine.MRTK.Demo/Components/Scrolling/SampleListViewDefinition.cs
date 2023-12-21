using Evergine.Framework;
using Evergine.MRTK.SDK.Features.UX.Components.Lists;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Evergine.MRTK.Demo.Components.Scrolling
{
    internal class SampleListViewDefinition : Component
    {
        [BindComponent(source: BindComponentSource.Children)]
        private ListView listView = null;

        protected override bool OnAttached()
        {
            bool attached = base.OnAttached();
            if (attached) 
            {
                var values = new List<Tuple<string, string, int>>();
                for (int i = 0; i < 40; i++)
                {
                    values.Add(Tuple.Create($"Column {i}.0", $"Column {i}.1", i));
                }

                listView.DataSource = new DemoAdapter(values);
                listView.Columns = new[]
                {
                    new ColumnDefinition { Title = "Title1", PercentageSize = 0.4f },
                    new ColumnDefinition { Title = "Title2", PercentageSize = 0.4f },
                    new ColumnDefinition { Title = "Title3", PercentageSize = 0.2f }
                };
            }

            return attached;
        }

        class DemoAdapter : ArrayAdapter<Tuple<string, string, int>>
        {
            public DemoAdapter(IList<Tuple<string, string, int>> data) : base(data)
            {
            }

            public override CellRenderer GetRenderer(int rowIndex, int columnIndex)
            {
                var element = this.Data.ElementAt(rowIndex);
                var renderer = TextCellRenderer.Instance;

                switch (columnIndex)
                {
                    case 0:
                        renderer.Text = element.Item1;
                        break;
                    case 1:
                        renderer.Text = element.Item2;
                        break;
                    case 2:
                        renderer.Text = $"#{element.Item3}";
                        break;
                    default:
                        throw new IndexOutOfRangeException($"Invalid column index: {nameof(columnIndex)}");
                }

                return renderer;
            }
        }
    }
}
