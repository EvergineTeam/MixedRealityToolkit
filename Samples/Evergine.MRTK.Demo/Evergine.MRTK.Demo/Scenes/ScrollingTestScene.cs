using Evergine.Framework;
using Evergine.MRTK.SDK.Features.UX.Components.Lists;
using System.Linq;

namespace Evergine.MRTK.Demo.Scenes
{
    public class ScrollingTestScene : DemoScene
    {
        protected override void OnPostCreateXRScene()
        {
            // ListView               
            var listView = this.Managers.EntityManager.FindAllByTag("ListView").First();

            var data = new ListViewData(2);
            for (int i = 0; i < 40; i++)
            {
                data.Add($"Column {i}.0", $"Column {i}.1");
            }

            var listViewComponent = listView.FindComponent<ListView>();

            if (listViewComponent != null)
            {
                listViewComponent.DataSource = data;
                listViewComponent.Render = new ListViewRender()
                                .AddColumn("Title1", 0.6f, TextCellRenderer.Instance)
                                .AddColumn("Title2", 0.4f, TextCellRenderer.Instance);                
            }

            // ScrollView
            var scrollView = this.Managers.EntityManager.FindAllByTag("ScrollView").First();
            
        }
    }
}
