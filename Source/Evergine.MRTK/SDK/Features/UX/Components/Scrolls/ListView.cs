
using Evergine.Common.Graphics;
using Evergine.Components.Graphics3D;
using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.Framework.Services;
using Evergine.Mathematics;
using Evergine.MRTK.Base.EventDatum.Input;
using Evergine.MRTK.Base.Interfaces.InputSystem.Handlers;
using Evergine.MRTK.Emulation;
using System;
using System.Linq;

namespace Evergine.MRTK.SDK.Features.UX.Components.Scrolls
{
    public class ListView : Behavior, IMixedRealityPointerHandler, IMixedRealityTouchHandler
    {
        public event EventHandler SelectedChanged;

        [BindEntity(source: BindEntitySource.ChildrenSkipOwner, tag: "PART_scrollviewer_scrollarea")]
        private Entity scrollArea = null;

        [BindComponent(source: BindComponentSource.ChildrenSkipOwner, tag: "PART_scrollviewer_content")]
        private Transform3D contentTransform = null;

        [BindEntity(source: BindEntitySource.ChildrenSkipOwner, tag: "PART_scrollviewer_content")]
        private Entity content = null;

        [BindComponent(source: BindComponentSource.ChildrenSkipOwner, tag: "PART_scrollviewer_bar")]
        private Transform3D scrollBarTransform = null;

        [BindComponent(source: BindComponentSource.ChildrenSkipOwner, tag: "PART_scrollviewer_bar")]
        private PlaneMesh scrollBarPlaneMesh = null;

        [BindComponent(source: BindComponentSource.ChildrenSkipOwner, tag: "PART_scrollviewer_background")]
        private PlaneMesh backgroundPlaneMesh = null;

        [BindEntity(source: BindEntitySource.ChildrenSkipOwner, tag: "PART_scrollviewer_header")]
        private Entity header = null;

        [BindComponent(source: BindComponentSource.ChildrenSkipOwner, tag: "PART_scrollviewer_selection")]
        private PlaneMesh selectionPlaneMesh = null;

        [BindComponent(source: BindComponentSource.ChildrenSkipOwner, tag: "PART_scrollviewer_selection")]
        private Transform3D selectionTransform = null;

        [BindService]
        private AssetsService assetsService = null;

        private ListlViewData dataSource;
        private ListViewRender render;

        private Vector2 ContentSize;

        private Cursor currentCursor;
        private Vector3 initialOffset;
        private int selectedIndex;
        private Vector3 lastCursorPosition;

        private bool interacting = false;
        private float velocityY;
        private Vector3 contentOrigin;
        private Vector3 barOrigin;
        private RenderLayerDescription contentLayer;
        private RenderLayerDescription alphaLayer;
        private bool headerEnabled = false;

        public float ContentPadding { get; set; } = 0.02f;

        public float ElasticTime { get; set; } = 0.1f;

        public float ZContentDistance { get; set; } = 0.004f;

        public float RowHeight { get; set; } = 0.015f;

        public float BarWidth { get; set; } = 0.004f;

        public ListlViewData DataSource
        {
            get => this.dataSource;
            set
            {
                this.dataSource = value;

                if (this.IsAttached)
                {
                    this.Refresh();
                }
            }
        }

        public int SelectedIndex => this.selectedIndex;

        public string[] Selected => this.dataSource.Data[this.selectedIndex];

        public ListViewRender Render
        {
            get => this.render;
            set
            {
                this.render = value;
                if (this.IsAttached)
                {
                    this.Refresh();
                }
            }
        }

        public bool HeaderEnabled
        {
            get => this.headerEnabled;
            set
            {
                this.headerEnabled = value;
                if (this.IsAttached)
                {
                    this.header.IsEnabled = this.headerEnabled;
                }
            }
        }

        /// <inheritdoc/>
        protected override bool OnAttached()
        {
            var result = base.OnAttached();

            this.contentLayer = this.assetsService.Load<RenderLayerDescription>(LoadModelResourceIDs.RenderLayers.Content);
            this.alphaLayer = this.assetsService.Load<RenderLayerDescription>(DefaultResourcesIDs.AlphaRenderLayerID);
            this.Refresh();

            return result;
        }

        private void Refresh()
        {
            if (this.dataSource == null || this.render == null)
            {
                return;
            }

            if (this.dataSource.Data[0].Length != this.render.Columns.Count)
            {
                throw new InvalidOperationException("The number of data columns must match with the render columns");
            }

            // Clean
            var headerChildren = this.header.ChildEntities.ToArray();
            foreach (var child in headerChildren)
            {
                this.header.RemoveChild(child);
            }

            var contentChildren = this.content.ChildEntities.ToArray();
            foreach (var child in contentChildren)
            {
                this.content.RemoveChild(child);
            }

            // Content config
            var contentPosition = this.contentTransform.LocalPosition;
            contentPosition.Z = this.ZContentDistance;
            this.contentTransform.LocalPosition = contentPosition;
            Vector2 size = new Vector2(this.backgroundPlaneMesh.Width - (this.ContentPadding * 2.0f), 0);

            // Header
            var columns = this.render.Columns;
            Vector3 headerPosition = new Vector3((this.backgroundPlaneMesh.Width * -0.5f) + this.ContentPadding, this.RowHeight * 0.5f, this.ZContentDistance);
            Color headerTextColor = Color.DarkBlue;
            for (int j = 0; j < columns.Count; j++)
            {
                var column = columns[j];
                float columnWidth = size.X * column.percentageSize;
                var entity = TextCellRenderer.Instance.Render(column.Name, headerPosition, columnWidth, this.RowHeight, this.alphaLayer, headerTextColor);
                this.header.AddChild(entity);
                headerPosition.X += columnWidth;
            }
            this.header.IsEnabled = this.HeaderEnabled;

            // Content from data
            this.contentOrigin = new Vector3((this.backgroundPlaneMesh.Width * -0.5f) + this.ContentPadding, this.backgroundPlaneMesh.Height * 0.5f, 0);
            Vector3 currentPosition = this.contentOrigin;

            var data = this.dataSource.Data;
            for (int i = 0; i < data.Count; i++)
            {
                var rowData = data[i];
                for (int j = 0; j < columns.Count; j++)
                {
                    var column = columns[j];
                    var cellRender = column.cellRenderer;
                    float columnWidth = size.X * column.percentageSize;
                    var entity = cellRender.Render(rowData[j], currentPosition, columnWidth, this.RowHeight, this.contentLayer, Color.White);
                    this.content.AddChild(entity);
                    currentPosition.X += columnWidth;
                }

                currentPosition.X = this.contentOrigin.X;
                currentPosition.Y -= this.RowHeight;
                size.Y += this.RowHeight;
            }

            this.ContentSize = size;

            // Update bar
            this.scrollBarPlaneMesh.Width = this.BarWidth;

            var barScale = this.scrollBarTransform.Scale;
            barScale.Y = this.backgroundPlaneMesh.Height / this.ContentSize.Y;
            this.scrollBarTransform.Scale = barScale;

            this.barOrigin = new Vector3((this.backgroundPlaneMesh.Width * 0.5f) - this.scrollBarPlaneMesh.Width, this.backgroundPlaneMesh.Height * 0.5f, this.ZContentDistance);
            var barPosition = this.scrollBarTransform.LocalPosition;
            barPosition = this.barOrigin;
            this.scrollBarTransform.LocalPosition = barPosition;

            // Selection
            this.selectionPlaneMesh.Width = size.X;
            this.selectionPlaneMesh.Height = this.RowHeight;
            var selectionPosition = this.selectionTransform.LocalPosition;
            selectionPosition.X = this.contentOrigin.X - (this.ContentPadding * 0.25f);
            selectionPosition.Z = this.ZContentDistance * 0.25f;
            this.selectionTransform.LocalPosition = selectionPosition;
        }

        public void OnPointerDown(MixedRealityPointerEventData eventData)
        {
            if (eventData.EventHandled)
            {
                return;
            }

            if (this.currentCursor == null)
            {
                if (eventData.CurrentTarget == this.scrollArea)
                {
                    this.interacting = true;

                    this.currentCursor = eventData.Cursor;

                    this.initialOffset = this.contentTransform.LocalPosition - eventData.Position;
                    this.SelectedRow(eventData.Position);
                    this.contentTransform.LocalPosition = eventData.Position + this.initialOffset;
                    this.lastCursorPosition = eventData.Position;

                    eventData.SetHandled();
                }
            }
        }

        public void OnPointerDragged(MixedRealityPointerEventData eventData)
        {
            if (eventData.EventHandled)
            {
                return;
            }

            if (this.currentCursor == eventData.Cursor)
            {
                Vector3 delta = eventData.Position - this.lastCursorPosition;

                Vector3 newPosition = this.lastCursorPosition + delta + this.initialOffset;

                var position = this.contentTransform.LocalPosition;
                position.Y = newPosition.Y;
                this.contentTransform.LocalPosition = position;
                this.lastCursorPosition = this.lastCursorPosition + delta;

                eventData.SetHandled();
            }
        }

        public void OnPointerUp(MixedRealityPointerEventData eventData)
        {
            if (eventData.EventHandled)
            {
                return;
            }

            if (this.currentCursor == eventData.Cursor)
            {
                this.interacting = false;
                this.currentCursor = null;
                eventData.SetHandled();
            }
        }

        public void OnPointerClicked(MixedRealityPointerEventData eventData)
        {
        }

        public void OnTouchStarted(HandTrackingInputEventData eventData)
        {
            if (eventData.Cursor is not CursorTouch)
            {
                return;
            }

            this.interacting = true;

            this.currentCursor = eventData.Cursor;

            this.initialOffset = this.contentTransform.LocalPosition - eventData.Position;
            this.SelectedRow(eventData.Position);
            this.contentTransform.LocalPosition = eventData.Position + this.initialOffset;
            this.lastCursorPosition = eventData.Position;
        }

        public void OnTouchUpdated(HandTrackingInputEventData eventData)
        {
            if (eventData.Cursor is not CursorTouch)
            {
                return;
            }

            Vector3 delta = eventData.Position - this.lastCursorPosition;

            Vector3 newPosition = this.lastCursorPosition + delta + this.initialOffset;

            var position = this.contentTransform.LocalPosition;
            position.Y = newPosition.Y;
            this.contentTransform.LocalPosition = position;
            this.lastCursorPosition = this.lastCursorPosition + delta;
        }

        public void OnTouchCompleted(HandTrackingInputEventData eventData)
        {
            if (eventData.Cursor is not CursorTouch)
            {
                return;
            }

            this.interacting = false;
            this.currentCursor = null;
        }

        protected override void Update(TimeSpan gameTime)
        {
            // Content Inertia
            if (!this.interacting)
            {
                var position = this.contentTransform.LocalPosition;

                if (position.Y < 0)
                {
                    position.Y = MathHelper.SmoothDamp(position.Y, 0, ref this.velocityY, this.ElasticTime, (float)gameTime.TotalSeconds);
                }
                else if (position.Y > this.ContentSize.Y - this.backgroundPlaneMesh.Height)
                {
                    position.Y = MathHelper.SmoothDamp(position.Y, this.ContentSize.Y - this.backgroundPlaneMesh.Height, ref this.velocityY, this.ElasticTime, (float)gameTime.TotalSeconds);
                }

                this.contentTransform.LocalPosition = position;
            }

            // Bar
            var barPosition = this.scrollBarTransform.LocalPosition;
            float contentDeltaNormalized = MathHelper.Clamp(this.contentTransform.LocalPosition.Y / (this.ContentSize.Y - this.backgroundPlaneMesh.Height), 0, 1);
            barPosition.Y = this.barOrigin.Y + (contentDeltaNormalized * (this.backgroundPlaneMesh.Height - this.scrollBarTransform.Scale.Y));
            this.scrollBarTransform.LocalPosition = barPosition;

            // Selection
            var selectionPosition = this.selectionTransform.LocalPosition;
            selectionPosition.Y = (this.contentTransform.LocalPosition.Y + this.contentOrigin.Y) - (this.RowHeight * this.selectedIndex);
            this.selectionTransform.LocalPosition = selectionPosition;
        }

        private void SelectedRow(Vector3 pointerPosition)
        {
            var distance = (this.contentTransform.Position.Y + this.contentOrigin.Y) - pointerPosition.Y;
            int index = (int)(distance / this.RowHeight);
            if (index < 0 || index >= this.dataSource.Data.Count)
            {
                return;
            }

            this.selectedIndex = index;

            this.SelectedChanged?.Invoke(this, null);
        }
    }
}