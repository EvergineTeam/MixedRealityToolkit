// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using Evergine.Common.Attributes;
using Evergine.Common.Graphics;
using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.Framework.Services;
using Evergine.Mathematics;
using Evergine.MRTK.Base.EventDatum.Input;
using Evergine.MRTK.Base.Interfaces.InputSystem.Handlers;
using Evergine.MRTK.Emulation;
using Evergine.MRTK.SDK.Features.UX.Components.Scrolling;
using System;
using System.Linq;

namespace Evergine.MRTK.SDK.Features.UX.Components.Lists
{
    /// <summary>
    /// List view behavior.
    /// </summary>
    public class ListView : Behavior, IMixedRealityPointerHandler, IMixedRealityTouchHandler
    {
        [BindComponent(source: BindComponentSource.ChildrenSkipOwner, tag: "PART_listview_separator")]
        private Transform3D separatorTransform = null;

        [BindEntity(source: BindEntitySource.ChildrenSkipOwner, tag: "PART_listview_header")]
        private Entity header = null;

        [BindEntity(source: BindEntitySource.ChildrenSkipOwner, tag: "PART_listview_header_contents")]
        private Entity headerContents = null;

        [BindComponent(source: BindComponentSource.ChildrenSkipOwner, tag: "PART_listview_header")]
        private Transform3D headerContainerTransform = null;

        [BindComponent(source: BindComponentSource.ChildrenSkipOwner, tag: "PART_listview_header_background")]
        private Transform3D headerBackgroundTransform = null;

        [BindEntity(source: BindEntitySource.ChildrenSkipOwner, tag: "PART_listview_selection")]
        private Entity selectionEntity = null;

        [BindComponent(source: BindComponentSource.ChildrenSkipOwner, tag: "PART_listview_selection")]
        private Transform3D selectionTransform = null;

        [BindEntity(source: BindEntitySource.ChildrenSkipOwner, tag: "PART_listview_loading_holder")]
        private Entity loadingHolder = null;

        [BindComponent(source: BindComponentSource.ChildrenSkipOwner, tag: "PART_scrollviewer")]
        private ScrollView scrollView = null;

        [BindService]
        private AssetsService assetsService = null;

        private Vector2 size = new Vector2(0.25f, 0.18f);

        private ColumnDefinition[] columnDefinitions;
        private float[] columnSizes;
        private DataAdapter dataSource;

        private Cursor currentCursor;
        private int selectedIndex = -1;
        private Vector3 gestureStartPosition;

        private RenderLayerDescription contentLayer;
        private RenderLayerDescription alphaLayer;
        private bool headerEnabled = false;

        private Entity loadingIndicator;
        private Vector3 contentOrigin;

        /// <summary>
        /// Raised when selection changes.
        /// </summary>
        public event EventHandler SelectedItemChanged;

        /// <summary>
        /// Raised when ListView is scrolled.
        /// </summary>
        public event EventHandler Scrolled;

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
                this.columnSizes = null;

                if (this.IsAttached)
                {
                    this.Refresh();
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected element index.
        /// </summary>
        [IgnoreEvergine]
        [DontRenderProperty]
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
                    return null;
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
                this.OnHeaderEnabledUpdated();
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
            var attached = base.OnAttached();
            if (attached)
            {
                this.scrollView.Scrolled += this.ScrollView_Scrolled;

                this.ShowLoadingIndicator = false;

                this.loadingIndicator = this.loadingIndicator ?? this.loadingHolder.ChildEntities.FirstOrDefault();
                this.UpdateLoadingIndicator();

                this.contentLayer = this.assetsService.Load<RenderLayerDescription>(MRTKResourceIDs.RenderLayers.ScrollContent);
                this.alphaLayer = this.assetsService.Load<RenderLayerDescription>(DefaultResourcesIDs.AlphaRenderLayerID);
            }

            return attached;
        }

        /// <inheritdoc/>
        protected override void OnDetach()
        {
            base.OnDetach();

            this.scrollView.Scrolled -= this.ScrollView_Scrolled;
        }

        /// <inheritdoc/>
        protected override void Start()
        {
            base.Start();
            this.OnHeaderEnabledUpdated();
            this.UpdateSize();
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
            this.columnSizes = null;
            this.scrollView.Size = new Vector2(this.size.X, this.size.Y);
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
            this.CalculateColumnSizes();

            if (this.dataSource == null || this.columnDefinitions?.Any() != true || this.dataSource.Count == 0)
            {
                this.scrollView.DisplayScrollBar = Application.Current.IsEditor;
                this.selectionEntity.IsEnabled = false;
                return;
            }

            // Clean
            this.scrollView.ClearContents(child => child != this.selectionEntity);

            // Content config
            this.scrollView.ZContentDistance = this.ZContentDistance;

            // Header
            this.RefreshHeader();

            // Content from data
            this.contentOrigin = new Vector3((this.size.X * -0.5f) + this.ContentPadding, this.size.Y * 0.5f, 0);
            var currentPosition = this.contentOrigin;
            var cellSize = new Vector2(0, this.RowHeight);

            for (int rowIndex = 0; rowIndex < this.dataSource.Count; rowIndex++)
            {
                for (int columnIndex = 0; columnIndex < this.columnDefinitions.Length; columnIndex++)
                {
                    var cellRender = this.dataSource.GetRenderer(rowIndex, columnIndex);
                    cellSize.X = this.columnSizes[columnIndex];

                    var entity = cellRender.InternalRender(currentPosition, cellSize.X, this.RowHeight, this.contentLayer);
                    entity.Flags = HideFlags.DontSave | HideFlags.DontShow;
                    this.scrollView.AddContent(entity, currentPosition.ToVector2(), cellSize);
                    currentPosition.X += cellSize.X;
                }

                currentPosition.X = this.contentOrigin.X;
                currentPosition.Y -= this.RowHeight;
            }

            this.scrollView.Refresh();

            // Update bar
            this.scrollView.DisplayScrollBar = true;
            this.scrollView.BarWidth = this.BarWidth;

            // Selection
            var selectionScale = this.selectionTransform.LocalScale;
            selectionScale.X = this.size.X - this.ContentPadding;
            selectionScale.Y = this.RowHeight;
            this.selectionTransform.LocalScale = selectionScale;

            var selectionPosition = this.selectionTransform.LocalPosition;
            selectionPosition.X = (this.ContentPadding / 2) - (this.size.X / 2);
            selectionPosition.Z = -0.003f;
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

            Vector3 headerPosition = new Vector3((this.size.X * -0.5f) + this.ContentPadding, this.RowHeight * 0.5f, this.ZContentDistance);
            var textRenderer = TextCellRenderer.Instance;
            var recoverColor = textRenderer.Color;

            for (int columnIndex = 0; columnIndex < this.columnDefinitions.Length; columnIndex++)
            {
                var column = this.columnDefinitions[columnIndex];
                float columnWidth = this.columnSizes[columnIndex];
                textRenderer.Text = column.Title;
                textRenderer.Color = column.HeaderTextColor;

                var entity = textRenderer.InternalRender(headerPosition, columnWidth, this.RowHeight, this.alphaLayer);
                entity.Flags = HideFlags.DontSave | HideFlags.DontShow;
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
        public void ScrollTo(float positionY) => this.scrollView.ScrollTo(positionY);

        /// <inheritdoc/>
        public void OnPointerDown(MixedRealityPointerEventData eventData)
        {
            if (this.currentCursor == null && this.IsPartOfScrollViewHierarchy(eventData.CurrentTarget))
            {
                this.currentCursor = eventData.Cursor;
                this.gestureStartPosition = eventData.Position;

                eventData.SetHandled();
            }
        }

        /// <inheritdoc/>
        public void OnPointerDragged(MixedRealityPointerEventData eventData)
        {
        }

        /// <inheritdoc/>
        public void OnPointerUp(MixedRealityPointerEventData eventData)
        {
            if (this.currentCursor == eventData.Cursor)
            {
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

            this.currentCursor = eventData.Cursor;
            this.gestureStartPosition = eventData.Position;
        }

        /// <inheritdoc/>
        public void OnTouchUpdated(HandTrackingInputEventData eventData)
        {
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

            this.currentCursor = null;
        }

        /// <inheritdoc/>
        protected override void Update(TimeSpan gameTime)
        {
            var selectionPosition = this.selectionTransform.LocalPosition;
            selectionPosition.Y = this.contentOrigin.Y - (this.RowHeight * this.selectedIndex);
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

        private void CalculateColumnSizes()
        {
            if (this.columnDefinitions?.Any() != true)
            {
                return;
            }

            if (this.columnSizes != null)
            {
                return;
            }

            float availableWidth = this.size.X - (this.ContentPadding * 2);
            this.columnSizes = new float[this.columnDefinitions.Length];
            for (int i = 0; i < this.columnDefinitions.Length; i++)
            {
                var columnDef = this.columnDefinitions[i];
                this.columnSizes[i] = availableWidth * columnDef.PercentageSize;
            }
        }

        private void SetSelectedRow(Vector3 pointerPosition)
        {
            var transform = this.Owner.FindComponent<Transform3D>();
            Vector3 localPointerPosition = Vector3.TransformCoordinate(pointerPosition, transform.WorldToLocalTransform);

            int index = (int)Math.Floor((this.scrollView.ScrollPosition.Y + (this.size.Y / 2) - localPointerPosition.Y) / this.RowHeight);
            this.SetSelectedRow(index);
        }

        private void SetSelectedRow(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= this.dataSource?.Count)
            {
                this.selectionEntity.IsEnabled = false;
                return;
            }

            this.selectedIndex = rowIndex;
            this.selectionEntity.IsEnabled = true;
            this.SelectedItemChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnHeaderEnabledUpdated()
        {
            if (!this.IsAttached)
            {
                return;
            }

            this.header.IsEnabled = this.headerEnabled;
        }

        private void ValidateColumnDefinitions()
        {
            if (this.columnDefinitions != null && this.columnDefinitions.Sum(def => def.PercentageSize) != 1.0f)
            {
                throw new InvalidOperationException("Column sizes sum must be 1.0f");
            }
        }

        private bool IsPartOfScrollViewHierarchy(Entity entity)
        {
            Entity current = entity;
            if (current == this.scrollView.Owner)
            {
                return true;
            }

            while ((current = current.Parent) != null)
            {
                if (current == this.scrollView.Owner)
                {
                    return true;
                }
            }

            return false;
        }

        private void ScrollView_Scrolled(object sender, EventArgs args) => this.Scrolled?.Invoke(this, EventArgs.Empty);
    }
}
