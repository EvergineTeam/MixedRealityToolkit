// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using Evergine.Common.Graphics;
using Evergine.Common.Input.Pointer;
using Evergine.Components.Graphics3D;
using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.Framework.Services;
using Evergine.Mathematics;
using Evergine.MRTK.Base.EventDatum.Input;
using Evergine.MRTK.Base.Interfaces.InputSystem.Handlers;
using Evergine.MRTK.Emulation;
using System;
using System.Diagnostics;
using System.Linq;

namespace Evergine.MRTK.SDK.Features.UX.Components.Scrolls
{
    /// <summary>
    /// List view behavior.
    /// </summary>
    public class ListView : Behavior, IMixedRealityPointerHandler, IMixedRealityTouchHandler
    {
        /// <summary>
        /// Raise when the current element selected change.
        /// </summary>
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

        private ListViewData dataSource;
        private ListViewRender render;

        private Vector2 ContentSize;

        private Cursor currentCursor;
        private Vector3 eventDataPosition;
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
        private float barSize;

        /// <summary>
        /// Gets or sets the content padding.
        /// </summary>
        public float ContentPadding { get; set; } = 0.02f;

        /// <summary>
        /// Gets or sets the elastic time when the content go to the edges.
        /// </summary>
        public float ElasticTime { get; set; } = 0.1f;

        /// <summary>
        /// Gets or sets z content respect to the content z.
        /// </summary>
        public float ZContentDistance { get; set; } = 0.004f;

        /// <summary>
        /// Gets or sets the row height.
        /// </summary>
        public float RowHeight { get; set; } = 0.015f;

        /// <summary>
        /// Gets or sets the bar width.
        /// </summary>
        public float BarWidth { get; set; } = 0.004f;

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        public ListViewData DataSource
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

        /// <summary>
        /// Gets the selected element index.
        /// </summary>
        public int SelectedIndex => this.selectedIndex;

        /// <summary>
        /// Gets the selected row data.
        /// </summary>
        public string[] Selected
        {
            get
            {
                if (this.selectedIndex >= 0 &&
                    this.selectedIndex < this.dataSource.Data.Count)
                {
                    return this.dataSource.Data[this.selectedIndex];
                }
                else
                {
                    return this.dataSource.Data[0];
                }
            }
        }

        /// <summary>
        /// Gets or sets the columns render config.
        /// </summary>
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

        /// <summary>
        /// Gets or sets a value indicating whether the Header is visible or not.
        /// </summary>
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

            this.contentLayer = this.assetsService.Load<RenderLayerDescription>(MRTKResourceIDs.RenderLayers.ScrollContent);
            this.alphaLayer = this.assetsService.Load<RenderLayerDescription>(DefaultResourcesIDs.AlphaRenderLayerID);
            this.Refresh();

            return result;
        }

        /// <summary>
        /// Refresh the layout.
        /// </summary>
        public void Refresh()
        {
            if (this.dataSource == null || this.render == null || this.dataSource.Data.Count == 0)
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
            barScale.Y = this.backgroundPlaneMesh.Height > this.ContentSize.Y ? 1 : this.backgroundPlaneMesh.Height / this.ContentSize.Y;
            this.barSize = this.backgroundPlaneMesh.Height * barScale.Y;
            this.scrollBarTransform.Scale = barScale;

            this.barOrigin = new Vector3((this.backgroundPlaneMesh.Width * 0.5f) - this.scrollBarPlaneMesh.Width, this.backgroundPlaneMesh.Height * 0.5f, this.ZContentDistance);
            var barPosition = this.scrollBarTransform.LocalPosition;
            barPosition = this.barOrigin;
            this.scrollBarTransform.LocalPosition = barPosition;

            // Selection
            this.selectionPlaneMesh.Width = this.backgroundPlaneMesh.Width - this.ContentPadding;
            this.selectionPlaneMesh.Height = this.RowHeight;
            var selectionPosition = this.selectionTransform.LocalPosition;
            selectionPosition.X = this.contentOrigin.X - (this.ContentPadding * 0.25f);
            selectionPosition.Z = this.ZContentDistance * 0.25f;
            this.selectionTransform.LocalPosition = selectionPosition;
        }

        /// <inheritdoc/>
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

                    this.eventDataPosition = eventData.Position;
                    this.initialOffset = this.contentTransform.LocalPosition - eventData.Position;
                    this.SelectedRow(eventData.Position);
                    this.contentTransform.LocalPosition = eventData.Position + this.initialOffset;
                    this.lastCursorPosition = eventData.Position;

                    eventData.SetHandled();
                }
            }
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public void OnPointerClicked(MixedRealityPointerEventData eventData)
        {
        }

        /// <inheritdoc/>
        public void OnTouchStarted(HandTrackingInputEventData eventData)
        {
            if (!(eventData.Cursor is CursorTouch))
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

        /// <inheritdoc/>
        public void OnTouchUpdated(HandTrackingInputEventData eventData)
        {
            if (!(eventData.Cursor is CursorTouch))
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

        /// <inheritdoc/>
        public void OnTouchCompleted(HandTrackingInputEventData eventData)
        {
            if (!(eventData.Cursor is CursorTouch))
            {
                return;
            }

            this.interacting = false;
            this.currentCursor = null;
        }

        /// <inheritdoc/>
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
            if (this.scrollBarTransform.Scale.Y != 1)
            {
                var barPosition = this.scrollBarTransform.LocalPosition;
                float contentDeltaNormalized = MathHelper.Clamp(this.contentTransform.LocalPosition.Y / (this.ContentSize.Y - this.backgroundPlaneMesh.Height), 0, 1);
                ////barPosition.Y = this.barOrigin.Y + (contentDeltaNormalized * (this.backgroundPlaneMesh.Height - this.scrollBarTransform.Scale.Y));
                barPosition.Y = this.barOrigin.Y - (contentDeltaNormalized * (this.backgroundPlaneMesh.Height - this.barSize));
                Debug.WriteLine($"ContentDeltaNormalized: {contentDeltaNormalized}");
                this.scrollBarTransform.LocalPosition = barPosition;
            }

            // Selection
            var selectionPosition = this.selectionTransform.LocalPosition;
            selectionPosition.Y = (this.contentTransform.LocalPosition.Y + this.contentOrigin.Y) - (this.RowHeight * this.selectedIndex);
            this.selectionTransform.LocalPosition = selectionPosition;
        }

        private void SelectedRow(Vector3 pointerPosition)
        {
            var originTransformed = this.contentTransform.Position + (this.contentTransform.WorldTransform.Up * this.contentOrigin.Y);
            var pointerPositionTransformed = this.contentTransform.Position + Vector3.Project(pointerPosition - this.contentTransform.Position, this.contentTransform.WorldTransform.Up);
            var distance = Vector3.Distance(originTransformed, pointerPositionTransformed);
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
