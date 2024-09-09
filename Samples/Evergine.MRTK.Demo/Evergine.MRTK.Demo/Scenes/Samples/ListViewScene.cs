using Evergine.Common.Graphics;
using Evergine.Components.Fonts;
using Evergine.Components.Graphics3D;
using Evergine.Components.WorkActions;
using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.Framework.Prefabs;
using Evergine.Framework.Services;
using Evergine.Mathematics;
using Evergine.MRTK.SDK.Features.UX.Components.Lists;
using Evergine.MRTK.SDK.Features.UX.Components.PressableButtons;
using Evergine.MRTK.SDK.Features.UX.Components.Sliders;
using Evergine.MRTK.SDK.Features.UX.Components.ToggleButtons;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Evergine.MRTK.Demo.Scenes.Samples
{
    public class ListViewScene : DemoScene
    {
        private ListView customListView;
        private ListView autoListView;
        private Vector2 initialCustomListViewSize;
        private PinchSlider customWidthSlider;
        private PinchSlider customHeightSlider;
        private ListView selectionListView;
        private ToggleButton scrollToStartToggle;
        private ToggleButton scrollToCenterToggle;
        private ToggleButton scrollToEndToggle;
        private Text3DMesh selectedItemEventText;
        private Text3DMesh scrolledEventText;
        private int scrollCounter;

        protected override void OnPostCreateXRScene()
        {
            base.OnPostCreateXRScene();

            var assetsManager = this.Managers.AssetSceneManager;

            this.customListView = this.Managers.EntityManager.FindAllByTag("configuration_custom").First().FindComponentInChildren<ListView>();
            this.customListView.LoadingIndicator = assetsManager.Load<Prefab>(EvergineContent.Prefabs.Samples.customLoadingIndicator_weprefab).Instantiate();
            this.autoListView = this.Managers.EntityManager.FindAllByTag("configuration_auto").First().FindComponentInChildren<ListView>();
            this.initialCustomListViewSize = this.customListView.Size;

            this.customWidthSlider = this.Managers.EntityManager.FindAllByTag("configuration_width").First().FindComponentInChildren<PinchSlider>();
            this.customHeightSlider = this.Managers.EntityManager.FindAllByTag("configuration_height").First().FindComponentInChildren<PinchSlider>();
            this.customWidthSlider.InitialValue = this.customHeightSlider.InitialValue = 1;

            var applyButton = this.Managers.EntityManager.FindAllByTag("controls_apply_button").First().FindComponentInChildren<PressableButton>();
            applyButton.ButtonPressed += this.ApplyButton_ButtonPressed;

            var headerVisibilityButton = this.Managers.EntityManager.FindAllByTag("controls_visibility_button").First().FindComponentInChildren<ToggleButton>();
            headerVisibilityButton.Toggled += HeaderVisibilityButton_Toggled;

            this.selectionListView = this.Managers.EntityManager.FindAllByTag("selection_listView").First().FindComponentInChildren<ListView>();

            var changeSelectedIndexButton = this.Managers.EntityManager.FindAllByTag("controls_selectedIndex_button").First().FindComponentInChildren<PressableButton>();
            changeSelectedIndexButton.ButtonPressed += this.ChangeSelectedIndexButton_ButtonPressed;

            var changeSelectedItemButton = this.Managers.EntityManager.FindAllByTag("controls_selectedItem_button").First().FindComponentInChildren<PressableButton>();
            changeSelectedItemButton.ButtonPressed += this.ChangeSelectedItemButton_ButtonPressed;
            
            this.scrollToStartToggle = this.Managers.EntityManager.FindAllByTag("selection_scrollTo_Start_button").First().FindComponentInChildren<ToggleButton>();
            this.scrollToStartToggle.Toggled += this.ScrollTo_Toggled;
            this.scrollToCenterToggle = this.Managers.EntityManager.FindAllByTag("selection_scrollTo_Center_button").First().FindComponentInChildren<ToggleButton>();
            this.scrollToCenterToggle.Toggled += this.ScrollTo_Toggled;
            this.scrollToEndToggle = this.Managers.EntityManager.FindAllByTag("selection_scrollTo_End_button").First().FindComponentInChildren<ToggleButton>();
            this.scrollToEndToggle.Toggled += this.ScrollTo_Toggled;

            this.selectedItemEventText = this.Managers.EntityManager.FindAllByTag("controls_selectedItem_event_text").First().FindComponentInChildren<Text3DMesh>();
            this.scrolledEventText = this.Managers.EntityManager.FindAllByTag("controls_scrolled_event_text").First().FindComponentInChildren<Text3DMesh>();
            this.selectionListView.SelectedItemChanged += this.SelectionListView_SelectedChanged;
            this.selectionListView.Scrolled += this.SelectionListView_Scrolled;
        }

        private void SelectionListView_Scrolled(object sender, EventArgs e) =>
            this.scrolledEventText.Text = $"{++scrollCounter} times";

        private void SelectionListView_SelectedChanged(object sender, EventArgs e) =>
            this.selectedItemEventText.Text = this.selectionListView.SelectedItem?.ToString();

        private void ScrollTo_Toggled(object sender, EventArgs e)
        {
            ToggleButton toggle = null;
            ScrollToPosition position = default;

            if (sender == this.scrollToStartToggle)
            {
                toggle = this.scrollToStartToggle;
                position = ScrollToPosition.Start;
            }
            else if (sender == this.scrollToCenterToggle)
            {
                toggle = this.scrollToCenterToggle;
                position = ScrollToPosition.Center;
            }
            else if (sender == this.scrollToEndToggle)
            {
                toggle = this.scrollToEndToggle;
                position = ScrollToPosition.End;
            }

            if (toggle.IsOn)
            {
                this.selectionListView.ScrollTo(this.selectionListView.SelectedIndex, position);
            }
        }

        private void ApplyButton_ButtonPressed(object sender, EventArgs e)
        {
            var listViewSize = this.customListView.Size;
            listViewSize.X = this.initialCustomListViewSize.X * this.customWidthSlider.SliderValue;
            listViewSize.Y = this.initialCustomListViewSize.Y * this.customHeightSlider.SliderValue;

            this.customListView.Size = this.autoListView.Size = listViewSize;
        }

        private void HeaderVisibilityButton_Toggled(object sender, EventArgs e)
        {
            if (sender is ToggleButton toggle)
            {
                this.customListView.HeaderEnabled = this.autoListView.HeaderEnabled = toggle.IsOn;
            }
        }

        private void ChangeSelectedIndexButton_ButtonPressed(object sender, EventArgs e) =>
            this.selectionListView.SelectedIndex = new System.Random().Next(0, 10);

        private void ChangeSelectedItemButton_ButtonPressed(object sender, EventArgs e)
        {
            var dataSource = this.selectionListView.DataSource;
            var index = new System.Random().Next(0, 10);
            var value = dataSource.GetRowValue(index);
            this.selectionListView.SelectedItem = value;
        }

        public class SampleTaskConfiguration : Component
        {
            [BindComponent(source: BindComponentSource.Children)]
            private ListView listView = null;

            [BindService]
            private AssetsService assetsService = null;

            public bool UseCustomAdapter { get; set; } = false;

            public int NumberOfElements { get; set; } = 40;

            protected override void OnActivated()
            {
                base.OnActivated();

                WorkActionFactory.CreateWorkActionFromAction(
                    this.Owner.Scene,
                    () => this.listView.ShowLoadingIndicator = true)
                    .Delay(TimeSpan.FromSeconds(2))
                    .ContinueWithAction(() =>
                    {
                        if (this.UseCustomAdapter)
                        {
                            this.CustomAdapterUsage();
                        }
                        else
                        {
                            this.BuiltInAdapterUsage();
                        }
                    })
                    .ContinueWithAction(() => this.listView.ShowLoadingIndicator = false)
                    .Run();
            }

            private void CustomAdapterUsage()
            {
                var allTasks = CreateDemoData();
                listView.DataSource = new SampleTasksAdapter(allTasks, this.assetsService);
                listView.Columns = new[]
                {
                    new ColumnDefinition { Title = "Name", PercentageSize = 0.4f, HeaderTextColor = Color.Yellow },
                    new ColumnDefinition { Title = "Deadline", PercentageSize = 0.4f, HeaderTextColor = Color.Yellow },
                    new ColumnDefinition { Title = "Status", PercentageSize = 0.2f, HeaderTextColor = Color.White }
                };
            }

            private void BuiltInAdapterUsage()
            {
                var allTasks = CreateDemoData();
                listView.DataSource = new ArrayAdapter<SampleTask>(allTasks);
                listView.Columns = new[]
                {
                    new ColumnDefinition { Title = "Task", PercentageSize = 1f },
                };
            }

            private List<SampleTask> CreateDemoData()
            {
                var allTasks = new List<SampleTask>();
                for (int i = 1; i <= this.NumberOfElements; i++)
                {
                    allTasks.Add(new SampleTask
                    {
                        Name = $"Task #{i}",
                        Deadline = DateTime.Today.AddDays(i),
                        Status = (TaskStatus)(i % 3),
                    });
                }

                return allTasks;
            }
        }

        class SampleTask
        {
            public string Name { get; set; }

            public DateTime Deadline { get; set; }

            public TaskStatus Status { get; set; }

            public override string ToString() => $"{Name} -> {Status}";
        }

        enum TaskStatus
        {
            Pending,
            InProgress,
            Done,
        }

        class SampleTasksAdapter : ArrayAdapter<SampleTask>
        {
            private readonly TaskStatusCellRenderer statusRenderer;

            public SampleTasksAdapter(IList<SampleTask> data, AssetsService assetsService)
                : base(data)
            {
                statusRenderer = new TaskStatusCellRenderer(assetsService);
            }

            public override CellRenderer GetRenderer(int rowIndex, int columnIndex)
            {
                SampleTask task = this.GetTypedRowValue(rowIndex);

                switch (columnIndex)
                {
                    case 0:
                    case 1:
                        var textRenderer = TextCellRenderer.Instance;
                        textRenderer.Text = columnIndex == 0 ? task.Name : task.Deadline.ToShortDateString();
                        textRenderer.Color = this.GetTextColorByStatus(task.Status);
                        return textRenderer;
                    case 2:
                        statusRenderer.Status = task.Status;
                        return statusRenderer;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }

            private Color GetTextColorByStatus(TaskStatus status)
            {
                switch (status)
                {
                    case TaskStatus.Pending:
                        return Color.Orange;
                    default:
                        return Color.White;
                }
            }
        }

        class TaskStatusCellRenderer : CellRenderer
        {
            private readonly AssetsService assetsService;

            public TaskStatusCellRenderer(AssetsService assetsService)
            {
                this.assetsService = assetsService;
            }

            public TaskStatus Status { get; set; }

            public override void Render(Entity parent)
            {
                var statusMaterial = assetsService.Load<Material>(this.GetStatusMaterialId());

                parent.AddChild(new Entity()
                    .AddComponent(new Transform3D
                    {
                        LocalPosition = new Vector3(0.01f, -0.008f, 0),
                        LocalScale = new Vector3(0.008f, 0.008f, 0.001f),
                    })
                    .AddComponent(new MaterialComponent() { Material = statusMaterial })
                    .AddComponent(new PlaneMesh
                    {
                        PlaneNormal = PlaneMesh.NormalAxis.ZPositive,
                    })
                    .AddComponent(new MeshRenderer()));
            }

            private Guid GetStatusMaterialId()
            {
                switch (Status)
                {
                    case TaskStatus.Pending:
                        return EvergineContent.Materials.Samples.ListView.Task_Status_Red;
                    case TaskStatus.InProgress:
                        return EvergineContent.Materials.Samples.ListView.Task_Status_Yellow;
                    case TaskStatus.Done:
                        return EvergineContent.Materials.Samples.ListView.Task_Status_Green;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}
