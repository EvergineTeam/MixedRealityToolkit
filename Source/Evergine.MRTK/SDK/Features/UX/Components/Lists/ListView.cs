// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using Evergine.Common.Attributes;
using Evergine.Common.Graphics;
using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.Framework.Physics3D;
using Evergine.Framework.Services;
using Evergine.Mathematics;
using Evergine.MRTK.Base.EventDatum.Input;
using Evergine.MRTK.Base.Interfaces.InputSystem.Handlers;
using Evergine.MRTK.Emulation;
using System;
using System.Linq;

namespace Evergine.MRTK.SDK.Features.UX.Components.Lists
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

        [BindComponent(source: BindComponentSource.ChildrenSkipOwner, tag: "PART_scrollviewer_scrollarea")]
        private BoxCollider3D scrollAreaCollider = null;

        [BindComponent(source: BindComponentSource.ChildrenSkipOwner, tag: "PART_scrollviewer_content")]
        private Transform3D contentTransform = null;

        [BindEntity(source: BindEntitySource.ChildrenSkipOwner, tag: "PART_scrollviewer_content")]
        private Entity content = null;

        [BindEntity(source: BindEntitySource.ChildrenSkipOwner, tag: "PART_scrollviewer_bar")]
        private Entity scrollBarEntity = null;

        [BindComponent(source: BindComponentSource.ChildrenSkipOwner, tag: "PART_scrollviewer_bar")]
        private Transform3D scrollBarTransform = null;

        [BindComponent(source: BindComponentSource.ChildrenSkipOwner, tag: "PART_scrollviewer_background")]
        private Transform3D backgroundPlaneTransform = null;

        [BindComponent(source: BindComponentSource.ChildrenSkipOwner, tag: "PART_scrollviewer_separator")]
        private Transform3D separatorTransform = null;

        [BindEntity(source: BindEntitySource.ChildrenSkipOwner, tag: "PART_scrollviewer_header")]
        private Entity header = null;

        [BindEntity(source: BindEntitySource.ChildrenSkipOwner, tag: "PART_scrollviewer_header_contents")]
        private Entity headerContents = null;

        [BindComponent(source: BindComponentSource.ChildrenSkipOwner, tag: "PART_scrollviewer_header")]
        private Transform3D headerContainerTransform = null;

        [BindComponent(source: BindComponentSource.ChildrenSkipOwner, tag: "PART_scrollviewer_header_background")]
        private Transform3D headerBackgroundTransform = null;

        [BindEntity(source: BindEntitySource.ChildrenSkipOwner, tag: "PART_scrollviewer_selection")]
        private Entity selectionEntity = null;

        [BindComponent(source: BindComponentSource.ChildrenSkipOwner, tag: "PART_scrollviewer_selection")]
        private Transform3D selectionTransform = null;

        [BindEntity(source: BindEntitySource.ChildrenSkipOwner, tag: "PART_scrollviewer_loading_holder")]
        private Entity loadingHolder = null;

        [BindService]
        private AssetsService assetsService = null;

        private Vector2 size = new Vector2(0.25f, 0.18f);

        private ColumnDefinition[] columnDefinitions;
        private DataAdapter dataSource;

        private Vector2 ContentSize;

        private Cursor currentCursor;
        private Vector3 initialOffset;
        private int selectedIndex = -1;
        private Vector3 lastCursorPosition;
        private Vector3 gestureStartPosition;

        private bool interacting = false;
        private float velocityY;
        private Vector3 contentOrigin;
        private Vector3 barOrigin;
        private RenderLayerDescription contentLayer;
        private RenderLayerDescription alphaLayer;
        private bool headerEnabled = false;
        private float barSize;

        private Entity loadingIndicator;

        /// <summary>
        /// Gets or sets ListView size.
        /// </summary>
        public Vector2 Size
        {
            get => this.size;
            set
            {
                this.size = value;
                if (this.IsAttached)
                {
                    this.UpdateSize();
                }
            }
        }

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
        [IgnoreEvergine]
        public DataAdapter DataSource
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
        /// Gets or sets the selected element index.
        /// </summary>
        public int SelectedIndex
        {
            get => this.selectedIndex;
            set
            {
                if (this.selectedIndex != value)
                {
                    this.SetSelectedRow(value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected item.
        /// </summary>
        [IgnoreEvergine]
        public object SelectedItem
        {
            get
            {
                if (this.dataSource == null || this.dataSource.Count == 0)
                {
                    return null;
                }

                if (this.selectedIndex >= 0 &&
                    this.selectedIndex < this.dataSource.Count)
                {
                    return this.dataSource.GetRowValue(this.selectedIndex);
                }
                else
                {
                    return this.dataSource.GetRowValue(0);
                }
            }

            set
            {
                this.selectedIndex = this.dataSource.IndexOf(value);
                this.SetSelectedRow(this.selectedIndex);
            }
        }

        /// <summary>
        /// Gets or sets column definitions.
        /// </summary>
        [IgnoreEvergine]
        public ColumnDefinition[] Columns
        {
            get => this.columnDefinitions;
            set
            {
                this.columnDefinitions = value;
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

        /// <summary>
        /// Gets or sets a value indicating whether loading indicator is shown.
        /// </summary>
        [IgnoreEvergine]
        [DontRenderProperty]
        public bool ShowLoadingIndicator
        {
            get => this.loadingHolder.IsEnabled;

            set => this.loadingHolder.IsEnabled = value;
        }

        /// <summary>
        /// Gets or sets loading indicator entity.
        /// </summary>
        [IgnoreEvergine]
        public Entity LoadingIndicator
        {
            get => this.loadingIndicator;

            set
            {
                if (this.loadingIndicator != value)
                {
                    this.loadingIndicator = value;
                    this.UpdateLoadingIndicator();
                }
            }
        }

        /// <inheritdoc/>
        protected override bool OnAttached()
        {
            var result = base.OnAttached();

            this.ShowLoadingIndicator = false;

            this.loadingIndicator = this.loadingIndicator ?? this.loadingHolder.ChildEntities.FirstOrDefault();
            this.UpdateLoadingIndicator();

            this.contentLayer = this.assetsService.Load<RenderLayerDescription>(MRTKResourceIDs.RenderLayers.ScrollContent);
            this.alphaLayer = this.assetsService.Load<RenderLayerDescription>(DefaultResourcesIDs.AlphaRenderLayerID);
            this.Refresh();

            return result;
        }

        private void UpdateLoadingIndicator()
        {
            var current = this.loadingHolder?.ChildEntities.FirstOrDefault();
            if (this.loadingHolder != null && current != this.loadingIndicator)
            {
                this.loadingHolder.RemoveChild(current);
                this.loadingHolder.AddChild(this.loadingIndicator);
            }
        }

        private void UpdateSize()
        {
            this.scrollAreaCollider.Size = new Vector3(this.size.X, this.size.Y, this.scrollAreaCollider.Size.Z);
            this.backgroundPlaneTransform.LocalScale = new Vector3(this.size.X, this.size.Y, this.backgroundPlaneTransform.LocalScale.Z);
            var headerContainerPosition = this.headerContainerTransform.LocalPosition;
            headerContainerPosition.Y = (this.size.Y / 2) + (this.RowHeight / 2);
            this.headerContainerTransform.LocalPosition = headerContainerPosition;
            this.separatorTransform.LocalScale = new Vector3(this.size.X, this.separatorTransform.LocalScale.Y, this.separatorTransform.LocalScale.Z);
            this.headerBackgroundTransform.LocalScale = new Vector3(this.size.X, this.headerBackgroundTransform.LocalScale.Y, this.headerBackgroundTransform.LocalScale.Z);

            this.Refresh();
        }

        /// <summary>
        /// Refresh the layout.
        /// </summary>
        public void Refresh()
        {
            this.ValidateColumnDefinitions();

            if (this.dataSource == null || this.columnDefinitions?.Any() != true || this.dataSource.Count == 0)
            {
                this.scrollBarEntity.IsEnabled = Application.Current.IsEditor;
                this.selectionEntity.IsEnabled = false;
                return;
            }

            // Clean
            var contentChildren = this.content.ChildEntities?.ToArray();
            if (contentChildren != null)
            {
                foreach (var child in contentChildren)
                {
                    this.content.RemoveChild(child);
                }
            }

            // Content config
            var contentPosition = this.contentTransform.LocalPosition;
            contentPosition.Z = this.ZContentDistance;
            this.contentTransform.LocalPosition = contentPosition;
            Vector2 size = this.CalculateContentSize();

            // Header
            this.RefreshHeader();

            // Content from data
            this.contentOrigin = new Vector3((this.backgroundPlaneTransform.LocalScale.X * -0.5f) + this.ContentPadding, this.backgroundPlaneTransform.LocalScale.Y * 0.5f, 0);
            Vector3 currentPosition = this.contentOrigin;

            for (int rowIndex = 0; rowIndex < this.dataSource.Count; rowIndex++)
            {
                for (int columnIndex = 0; columnIndex < this.columnDefinitions.Length; columnIndex++)
                {
                    var column = this.columnDefinitions[columnIndex];
                    var cellRender = this.dataSource.GetRenderer(rowIndex, columnIndex);
                    float columnWidth = size.X * column.PercentageSize;
                    var entity = cellRender.InternalRender(currentPosition, columnWidth, this.RowHeight, this.contentLayer);
                    entity.Flags = HideFlags.DontSave;
                    this.content.AddChild(entity);
                    currentPosition.X += columnWidth;
                }

                currentPosition.X = this.contentOrigin.X;
                currentPosition.Y -= this.RowHeight;
                size.Y += this.RowHeight;
            }

            this.ContentSize = size;

            // Update bar
            this.scrollBarEntity.IsEnabled = true;

            var scrollBarScale = this.scrollBarTransform.LocalScale;
            scrollBarScale.X = this.BarWidth;
            this.scrollBarTransform.LocalScale = scrollBarScale;

            var barScale = this.scrollBarTransform.LocalScale;
            var barScaleFactor = this.backgroundPlaneTransform.LocalScale.Y > this.ContentSize.Y ? 1 : this.backgroundPlaneTransform.LocalScale.Y / this.ContentSize.Y;
            this.barSize = this.size.Y * barScaleFactor;
            barScale.Y = this.barSize;
            this.scrollBarTransform.LocalScale = barScale;

            this.barOrigin = new Vector3((this.backgroundPlaneTransform.LocalScale.X * 0.5f) - this.BarWidth, this.backgroundPlaneTransform.LocalScale.Y * 0.5f, this.ZContentDistance);
            this.scrollBarTransform.LocalPosition = this.barOrigin;

            // Selection
            var selectionScale = this.selectionTransform.LocalScale;
            selectionScale.X = this.backgroundPlaneTransform.LocalScale.X - this.ContentPadding;
            selectionScale.Y = this.RowHeight;
            this.selectionTransform.LocalScale = selectionScale;

            var selectionPosition = this.selectionTransform.LocalPosition;
            selectionPosition.X = this.contentOrigin.X - (this.ContentPadding * 0.25f);
            selectionPosition.Z = this.ZContentDistance * 0.25f;
            this.selectionTransform.LocalPosition = selectionPosition;
        }

        /// <summary>
        /// Refresh header layout.
        /// </summary>
        public void RefreshHeader()
        {
            var headerChildren = this.headerContents.ChildEntities?.ToArray();
            if (headerChildren != null)
            {
                foreach (var child in headerChildren)
                {
                    this.headerContents.RemoveChild(child);
                }
            }

            Vector2 size = this.CalculateContentSize();
            Vector3 headerPosition = new Vector3((this.backgroundPlaneTransform.LocalScale.X * -0.5f) + this.ContentPadding, this.RowHeight * 0.5f, this.ZContentDistance);
            var textRenderer = TextCellRenderer.Instance;
            var recoverColor = textRenderer.Color;

            for (int columnIndex = 0; columnIndex < this.columnDefinitions.Length; columnIndex++)
            {
                var column = this.columnDefinitions[columnIndex];
                float columnWidth = size.X * column.PercentageSize;
                textRenderer.Text = column.Title;
                textRenderer.Color = column.HeaderTextColor;

                var entity = textRenderer.InternalRender(headerPosition, columnWidth, this.RowHeight, this.alphaLayer);
                entity.Flags = HideFlags.DontSave;
                this.headerContents.AddChild(entity);
                headerPosition.X += columnWidth;
            }

            textRenderer.Color = recoverColor;
            this.headerContents.IsEnabled = this.HeaderEnabled;
        }

        /// <summary>
        /// Scrolls the list to make a row visible.
        /// </summary>
        /// <param name="rowIndex">Row index.</param>
        /// <param name="position">Scroll position.</param>
        public void ScrollTo(int rowIndex, ScrollToPosition position)
        {
            if (rowIndex >= 0)
            {
                float scrollYPosition = rowIndex * this.RowHeight;
                int numberOfInScreenItems = (int)(this.size.Y / this.RowHeight);

                switch (position)
                {
                    case ScrollToPosition.Start:
                        break;
                    case ScrollToPosition.Center:
                        scrollYPosition -= numberOfInScreenItems / 2 * this.RowHeight;
                        break;
                    case ScrollToPosition.End:
                        // If size.Y is not a multiple of RowHeight, end alignment will only show
                        // part of the item. We calculate extra scroll distance to avoid this.
                        float extraDistance = this.RowHeight - (this.size.Y % this.RowHeight);
                        scrollYPosition -= (numberOfInScreenItems * this.RowHeight) - extraDistance;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(position));
                }

                this.ScrollTo(scrollYPosition);
            }
        }

        /// <summary>
        /// Scrolls the list to make a row visible.
        /// </summary>
        /// <param name="item">Target item.</param>
        /// <param name="position">Scroll position.</param>
        public void ScrollTo(object item, ScrollToPosition position)
        {
            var itemIndex = this.dataSource.IndexOf(item);
            this.ScrollTo(itemIndex, position);
        }

        /// <summary>
        /// Scrolls the list to a position.
        /// </summary>
        /// <param name="positionY">Position Y of the scroll area.</param>
        public void ScrollTo(float positionY)
        {
            var contentPosition = this.contentTransform.LocalPosition;
            contentPosition.Y = positionY;
            this.contentTransform.LocalPosition = contentPosition;
        }

        private Vector2 CalculateContentSize()
        {
            return new Vector2(this.backgroundPlaneTransform.LocalScale.X - (this.ContentPadding * 2.0f), 0);
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
                    this.gestureStartPosition = eventData.Position;
                    this.initialOffset = this.contentTransform.LocalPosition - eventData.Position;
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

                if (!ThatHasBeenADraggingGesture(this.gestureStartPosition, eventData.Position))
                {
                    this.SetSelectedRow(eventData.Position);
                }

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
            this.gestureStartPosition = eventData.Position;
            this.initialOffset = this.contentTransform.LocalPosition - eventData.Position;
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

            if (!ThatHasBeenADraggingGesture(this.gestureStartPosition, eventData.Position))
            {
                this.SetSelectedRow(eventData.Position);
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
                else if (position.Y > this.ContentSize.Y - this.backgroundPlaneTransform.LocalScale.Y)
                {
                    position.Y = MathHelper.SmoothDamp(position.Y, this.ContentSize.Y - this.backgroundPlaneTransform.LocalScale.Y, ref this.velocityY, this.ElasticTime, (float)gameTime.TotalSeconds);
                }

                this.contentTransform.LocalPosition = position;
            }

            // Bar
            if (this.scrollBarTransform.Scale.Y != 1)
            {
                var barPosition = this.scrollBarTransform.LocalPosition;
                float contentDeltaNormalized = MathHelper.Clamp(this.contentTransform.LocalPosition.Y / (this.ContentSize.Y - this.backgroundPlaneTransform.LocalScale.Y), 0, 1);
                barPosition.Y = this.barOrigin.Y - (contentDeltaNormalized * (this.backgroundPlaneTransform.LocalScale.Y - this.barSize));
                this.scrollBarTransform.LocalPosition = barPosition;
            }

            // Selection
            var selectionPosition = this.selectionTransform.LocalPosition;
            selectionPosition.Y = this.contentTransform.LocalPosition.Y + this.contentOrigin.Y - (this.RowHeight * this.selectedIndex);
            this.selectionTransform.LocalPosition = selectionPosition;
        }

        private static bool ThatHasBeenADraggingGesture(Vector3 initialPosition, Vector3 finalPosition)
        {
            /*
             * Distance that pointer/touch should be moved to consider that selection is effectively
             * being performed by user, and he is not just dragging the list.
             */
            const float SelectionChangeGestureDiff = 0.01f;

            // we just consider Y component, as other parts of class code
            var diffY = Math.Abs(initialPosition.Y - finalPosition.Y);
            return diffY > SelectionChangeGestureDiff;
        }

        private void SetSelectedRow(Vector3 pointerPosition)
        {
            var originTransformed = this.contentTransform.Position + (this.contentTransform.WorldTransform.Up * this.contentOrigin.Y);
            var pointerPositionTransformed = this.contentTransform.Position + Vector3.Project(pointerPosition - this.contentTransform.Position, this.contentTransform.WorldTransform.Up);
            var distance = Vector3.Distance(originTransformed, pointerPositionTransformed);
            int index = (int)(distance / this.RowHeight);
            this.SetSelectedRow(index);
        }

        private void SetSelectedRow(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= this.dataSource.Count)
            {
                this.selectionEntity.IsEnabled = false;
                return;
            }

            this.selectedIndex = rowIndex;
            this.selectionEntity.IsEnabled = true;
            this.SelectedChanged?.Invoke(this, null);
        }

        private void ValidateColumnDefinitions()
        {
            if (this.columnDefinitions != null && this.columnDefinitions.Sum(def => def.PercentageSize) != 1.0f)
            {
                throw new InvalidOperationException("Column sizes sum must be 1.0f");
            }
        }
    }
}
