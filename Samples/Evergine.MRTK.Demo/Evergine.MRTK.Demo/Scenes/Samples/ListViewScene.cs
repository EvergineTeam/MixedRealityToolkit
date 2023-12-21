using Evergine.Common.Graphics;
using Evergine.Components.Graphics3D;
using Evergine.Framework;
using Evergine.Framework.Graphics;
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

        protected override void OnPostCreateXRScene()
        {
            base.OnPostCreateXRScene();

            this.customListView = this.Managers.EntityManager.FindAllByTag("configuration_custom").First().FindComponentInChildren<ListView>();
            this.autoListView = this.Managers.EntityManager.FindAllByTag("configuration_auto").First().FindComponentInChildren<ListView>();
            this.initialCustomListViewSize = this.customListView.Size;

            this.customWidthSlider = this.Managers.EntityManager.FindAllByTag("configuration_width").First().FindComponentInChildren<PinchSlider>();
            this.customHeightSlider = this.Managers.EntityManager.FindAllByTag("configuration_height").First().FindComponentInChildren<PinchSlider>();
            this.customWidthSlider.InitialValue = this.customHeightSlider.InitialValue = 1;

            var applyButton = this.Managers.EntityManager.FindAllByTag("controls_apply_button").First().FindComponentInChildren<PressableButton>();
            applyButton.ButtonPressed += ApplyButton_ButtonPressed;

            var headerVisibilityButton = this.Managers.EntityManager.FindAllByTag("controls_visibility_button").First().FindComponentInChildren<ToggleButton>();
            headerVisibilityButton.Toggled += HeaderVisibilityButton_Toggled;
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

        public class SampleTaskConfiguration : Component
        {
            [BindComponent(source: BindComponentSource.Children)]
            private ListView listView = null;

            [BindService]
            private AssetsService assetsService = null;

            public bool UseCustomAdapter { get; set; } = false;

            public int NumberOfElements { get; set; } = 40;

            protected override bool OnAttached()
            {
                bool attached = base.OnAttached();
                if (attached) 
                {
                    if (this.UseCustomAdapter)
                    {
                        this.CustomAdapterUsage();
                    }
                    else
                    {
                        this.BuiltInAdapterUsage();
                    }
                }

                return attached;
            }

            private void CustomAdapterUsage()
            {
                var allTasks = CreateDemoData();
                listView.DataSource = new SampleTasksAdapter(allTasks, this.assetsService);
                listView.Columns = new[]
                {
                    new ColumnDefinition { Title = "Name", PercentageSize = 0.4f },
                    new ColumnDefinition { Title = "Deadline", PercentageSize = 0.4f },
                    new ColumnDefinition { Title = "Status", PercentageSize = 0.2f }
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
