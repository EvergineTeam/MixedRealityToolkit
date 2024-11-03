// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using Evergine.Common.Attributes;
using Evergine.Common.Graphics;
using Evergine.Components.Fonts;
using Evergine.Components.Graphics3D;
using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.Framework.Physics3D;
using Evergine.Mathematics;
using Evergine.MRTK.Base.EventDatum.Input;
using Evergine.MRTK.Base.Interfaces.InputSystem.Handlers;
using Evergine.MRTK.Emulation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Evergine.MRTK.SDK.Features.UX.Components.Scrolling
{
    /// <summary>
    /// Simple scroll viewer.
    /// </summary>
    public class ScrollView : Behavior, IMixedRealityPointerHandler, IMixedRealityTouchHandler
    {
        [BindEntity(source: BindEntitySource.ChildrenSkipOwner, tag: "PART_scrollviewer_scrollarea")]
        private Entity scrollArea = null;

        [BindComponent(source: BindComponentSource.ChildrenSkipOwner, tag: "PART_scrollviewer_scrollarea")]
        private BoxCollider3D scrollAreaCollider = null;

        [BindComponent(source: BindComponentSource.ChildrenSkipOwner, tag: "PART_scrollviewer_content")]
        private Transform3D contentTransform = null;

        [BindEntity(source: BindEntitySource.ChildrenSkipOwner, tag: "PART_scrollviewer_content")]
        private Entity content = null;

        [BindEntity(source: BindEntitySource.ChildrenSkipOwner, tag: "PART_scrollviewer_horizontal_bar")]
        private Entity horizontalScrollBarEntity = null;

        [BindEntity(source: BindEntitySource.ChildrenSkipOwner, tag: "PART_scrollviewer_vertical_bar")]
        private Entity verticalScrollBarEntity = null;

        [BindComponent(source: BindComponentSource.ChildrenSkipOwner, tag: "PART_scrollviewer_horizontal_bar")]
        private Transform3D horizontalScrollBarTransform = null;

        [BindComponent(source: BindComponentSource.ChildrenSkipOwner, tag: "PART_scrollviewer_vertical_bar")]
        private Transform3D verticalScrollBarTransform = null;

        [BindComponent(source: BindComponentSource.ChildrenSkipOwner, tag: "PART_scrollviewer_background")]
        private Transform3D backgroundPlaneTransform = null;

        private Cursor currentCursor;
        private Vector3 initialOffset;
        private Vector3 lastCursorPosition;

        private bool interacting = false;
        private float velocityX;
        private float velocityY;

        private Vector2 size = new Vector2(0.25f, 0.18f);
        private ScrollBarVisibility verticalScrollBarVisibility;
        private ScrollBarVisibility horizontalScrollBarVisibility;
        private float zContentDistance = 0.004f;
        private float barWidth = 0.004f;
        private Vector2 contentSize;
        private Dictionary<Entity, Vector2> measuredContents = new Dictionary<Entity, Vector2>();

        /// <summary>
        /// Raise when ScrollView is scrolled.
        /// </summary>
        public event EventHandler Scrolled;

        /// <summary>
        /// Gets or sets the content padding.
        /// </summary>
        public float ContentPadding { get; set; } = 0.01f;

        /// <summary>
        /// Gets or sets the elastic time when the content go to the edges.
        /// </summary>
        public float ElasticTime { get; set; } = 0.1f;

        /// <summary>
        /// Gets or sets z content respect to the content z.
        /// </summary>
        public float ZContentDistance
        {
            get => this.zContentDistance;

            set
            {
                if (this.zContentDistance != value)
                {
                    this.zContentDistance = value;
                    this.UpdateZContentDistance();
                }
            }
        }

        /// <summary>
        /// Gets or sets the bar width.
        /// </summary>
        public float BarWidth
        {
            get => this.barWidth;

            set
            {
                if (this.barWidth != value)
                {
                    this.barWidth = value;
                    this.UpdateBarWidth();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the debug mode is enabled or not.
        /// </summary>
        public bool Debug { get; set; }

        /// <summary>
        /// Gets or sets ScrollView size. Note that on resizing, content offsets will
        /// not be automatically recalculated. You should update local positions manually.
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
        /// Gets or sets a value indicating whether scroll bar should be displayed.
        /// </summary>
        [IgnoreEvergine]
        [DontRenderProperty]
        [Obsolete($"Use {nameof(this.HorizontalScrollBarVisibility)} or {nameof(this.VerticalScrollBarVisibility)} instead")]
        public bool DisplayScrollBar
        {
            get => this.verticalScrollBarVisibility != ScrollBarVisibility.Hidden;

            set
            {
                this.verticalScrollBarVisibility = value ? ScrollBarVisibility.Auto : ScrollBarVisibility.Hidden;
                this.UpdateDisplayScrollBarsVisibility();
            }
        }

        /// <summary>
        /// Gets or sets horizontal scroll bar visibility.
        /// </summary>
        public ScrollBarVisibility HorizontalScrollBarVisibility
        {
            get => this.horizontalScrollBarVisibility;

            set
            {
                if (this.horizontalScrollBarVisibility != value)
                {
                    this.horizontalScrollBarVisibility = value;
                    this.UpdateDisplayScrollBarsVisibility();
                }
            }
        }

        /// <summary>
        /// Gets or sets vertical scroll bar visibility.
        /// </summary>
        public ScrollBarVisibility VerticalScrollBarVisibility
        {
            get => this.verticalScrollBarVisibility;

            set
            {
                if (this.verticalScrollBarVisibility != value)
                {
                    this.verticalScrollBarVisibility = value;
                    this.UpdateDisplayScrollBarsVisibility();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether horizontal scroll is enabled.
        /// </summary>
        public bool HorizontalScrollEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether vertical scroll is enabled.
        /// </summary>
        public bool VerticalScrollEnabled { get; set; } = true;

        /// <summary>
        /// Gets current scroll position.
        /// </summary>
        [IgnoreEvergine]
        [DontRenderProperty]
        public Vector2 ScrollPosition { get; private set; }

        /// <inheritdoc/>
        protected override void OnActivated()
        {
            base.OnActivated();

            this.MeasureContents();
            this.UpdateSize();
            this.UpdateZContentDistance();
            this.UpdateBarWidth();
            this.Refresh();
        }

        /// <inheritdoc/>
        public void OnPointerClicked(MixedRealityPointerEventData eventData)
        {
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
                    this.initialOffset = this.contentTransform.LocalPosition - eventData.Position;
                    this.contentTransform.LocalPosition = eventData.Position + this.initialOffset;
                    this.lastCursorPosition = eventData.Position;

                    this.ScrollPosition = new Vector2(
                        Math.Max(0, this.lastCursorPosition.X),
                        Math.Max(0, this.lastCursorPosition.Y));

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
                delta.X = this.HorizontalScrollEnabled ? delta.X : 0;
                delta.Y = this.VerticalScrollEnabled ? delta.Y : 0;

                Vector3 newPosition = this.lastCursorPosition + delta + this.initialOffset;

                var position = this.contentTransform.LocalPosition;
                position.X = newPosition.X;
                position.Y = newPosition.Y;
                this.contentTransform.LocalPosition = position;
                this.lastCursorPosition = this.lastCursorPosition + delta;

                eventData.SetHandled();

                this.ScrollPosition = new Vector2(
                    Math.Max(0, position.X),
                    Math.Max(0, position.Y));
                this.Scrolled?.Invoke(this, EventArgs.Empty);
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
        public void OnTouchStarted(HandTrackingInputEventData eventData)
        {
            if (!(eventData.Cursor is CursorTouch))
            {
                return;
            }

            this.interacting = true;

            this.currentCursor = eventData.Cursor;

            this.initialOffset = this.contentTransform.LocalPosition - eventData.Position;
            this.contentTransform.LocalPosition = eventData.Position + this.initialOffset;
            this.lastCursorPosition = eventData.Position;

            this.ScrollPosition = new Vector2(
                Math.Max(0, this.lastCursorPosition.X),
                Math.Max(0, this.lastCursorPosition.Y));
        }

        /// <inheritdoc/>
        public void OnTouchUpdated(HandTrackingInputEventData eventData)
        {
            if (!(eventData.Cursor is CursorTouch))
            {
                return;
            }

            Vector3 delta = eventData.Position - this.lastCursorPosition;
            delta.X = this.HorizontalScrollEnabled ? delta.X : 0;
            delta.Y = this.VerticalScrollEnabled ? delta.Y : 0;

            Vector3 newPosition = this.lastCursorPosition + delta + this.initialOffset;

            var position = this.contentTransform.LocalPosition;
            position.Y = newPosition.Y;
            this.contentTransform.LocalPosition = position;
            this.lastCursorPosition = this.lastCursorPosition + delta;

            this.ScrollPosition = new Vector2(
                Math.Max(0, position.X),
                Math.Max(0, position.Y));
            this.Scrolled?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Clears scrollview contents.
        /// </summary>
        /// <param name="clearCriteria">Criteria to filter which entities should be removed from contents. If
        /// it evaluates to <see langword="true"/> entity will be removed; not removed otherwise.</param>
        public void ClearContents(Func<Entity, bool> clearCriteria = null)
        {
            var contentChildren = this.content.ChildEntities?.ToArray();
            if (contentChildren != null)
            {
                foreach (var child in contentChildren)
                {
                    if (clearCriteria == null || clearCriteria.Invoke(child))
                    {
                        this.content.RemoveChild(child);
                    }
                }
            }

            this.contentSize = default;
        }

        /// <summary>
        /// Adds an entity into content container.
        /// </summary>
        /// <param name="entity">Entity to be added.</param>
        /// <param name="contentPosition">Content position.</param>
        /// <param name="contentSize">Content size.</param>
        public void AddContent(Entity entity, Vector2 contentPosition, Vector2 contentSize)
        {
            this.content.AddChild(entity);
            this.measuredContents[entity] = contentSize;

            var topLeftPos = new Vector2(-this.size.X * 0.5f, this.size.Y * 0.5f);
            var relativePos = topLeftPos - contentPosition;

            this.contentSize.X = Math.Max(this.contentSize.X, relativePos.X + contentSize.X);
            this.contentSize.Y = Math.Max(this.contentSize.Y, relativePos.Y + contentSize.Y);
        }

        /// <summary>
        /// Refreshes scrollview layout.
        /// </summary>
        public void Refresh()
        {
            this.InnerUpdateScrollBarPositionAndScale(true);
            this.InnerUpdateScrollBarPositionAndScale(false);
        }

        private void InnerUpdateScrollBarPositionAndScale(bool isHorizontalBar)
        {
            var barTransform = isHorizontalBar ? this.horizontalScrollBarTransform : this.verticalScrollBarTransform;
            var barVisibility = isHorizontalBar ? this.horizontalScrollBarVisibility : this.verticalScrollBarVisibility;
            var barEntity = isHorizontalBar ? this.horizontalScrollBarEntity : this.verticalScrollBarEntity;
            var barScale = barTransform.LocalScale;
            var scaleFactorBackgroundDimension = isHorizontalBar ? this.backgroundPlaneTransform.LocalScale.X : this.backgroundPlaneTransform.LocalScale.Y;
            var contentSizeDimension = isHorizontalBar ? this.contentSize.X : this.contentSize.Y;

            var barScaleFactor = scaleFactorBackgroundDimension > contentSizeDimension ? 1 : scaleFactorBackgroundDimension / contentSizeDimension;
            var barFullSpaceDimension = (isHorizontalBar ? this.size.X : this.size.Y) * barScaleFactor;
            if (isHorizontalBar)
            {
                barScale.X = barFullSpaceDimension == 0 ? this.size.X : barFullSpaceDimension;
            }
            else
            {
                barScale.Y = barFullSpaceDimension == 0 ? this.size.Y : barFullSpaceDimension;
            }

            barTransform.LocalScale = barScale;

            var barPosition = isHorizontalBar
                ? new Vector3(-this.backgroundPlaneTransform.LocalScale.X * 0.5f, this.barWidth - (this.backgroundPlaneTransform.LocalScale.Y * 0.5f), this.ZContentDistance)
                : new Vector3((this.backgroundPlaneTransform.LocalScale.X * 0.5f) - this.barWidth, this.backgroundPlaneTransform.LocalScale.Y * 0.5f, this.ZContentDistance);
            barTransform.LocalPosition = barPosition;
            barEntity.IsEnabled = barVisibility == ScrollBarVisibility.Hidden
                ? false
                : barVisibility == ScrollBarVisibility.Auto ? barScaleFactor != 1 : true;
        }

        /// <inheritdoc/>
        protected override void Update(TimeSpan gameTime)
        {
            // Content Inertia
            if (!this.interacting)
            {
                this.InternalUpdateContentIntertiaOnUpdateCycle(true, gameTime);
                this.InternalUpdateContentIntertiaOnUpdateCycle(false, gameTime);
            }

            // Bar
            this.InternalUpdateBarPositionOnUpdateCycle(true);
            this.InternalUpdateBarPositionOnUpdateCycle(false);

            // Debug mode
            if (this.Debug)
            {
                var position = this.contentTransform.Position;
                var localPosition = this.contentTransform.LocalPosition;
                var min = new Vector3(position.X - (this.backgroundPlaneTransform.LocalScale.X * 0.5f), position.Y - this.contentSize.Y + (this.backgroundPlaneTransform.LocalScale.Y / 2), position.Z);
                var max = new Vector3(position.X + (this.backgroundPlaneTransform.LocalScale.X * 0.5f), position.Y + (this.backgroundPlaneTransform.LocalScale.Y / 2), position.Z);
                this.Managers.RenderManager.LineBatch3D.DrawRectangle(min, max, Color.Red);

                var elements = this.contentTransform.Owner.ChildEntities.ToArray();
                foreach (var element in elements)
                {
                    var mesh = element.FindComponent<MeshComponent>(false);
                    var transform = element.FindComponent<Transform3D>();
                    if (mesh != null && transform != null)
                    {
                        var boundingBox = mesh.BoundingBox.Value;
                        boundingBox.Transform(transform.WorldTransform);
                        this.Managers.RenderManager.LineBatch3D.DrawBoundingBox(boundingBox, Color.Yellow);
                    }
                }
            }
        }

        private void InternalUpdateContentIntertiaOnUpdateCycle(bool isHorizontalBar, TimeSpan gameTime)
        {
            var localPosition = this.contentTransform.LocalPosition;

            if (isHorizontalBar)
            {
                if (localPosition.X > 0)
                {
                    localPosition.X = MathHelper.SmoothDamp(localPosition.X, 0, ref this.velocityX, this.ElasticTime, (float)gameTime.TotalSeconds);
                }
                else if (localPosition.X < this.size.X - this.contentSize.X)
                {
                    localPosition.X = MathHelper.SmoothDamp(localPosition.X, this.size.X - this.contentSize.X, ref this.velocityX, this.ElasticTime, (float)gameTime.TotalSeconds);
                }
            }
            else
            {
                if (localPosition.Y < 0)
                {
                    localPosition.Y = MathHelper.SmoothDamp(localPosition.Y, 0, ref this.velocityY, this.ElasticTime, (float)gameTime.TotalSeconds);
                }
                else if (localPosition.Y > this.contentSize.Y - this.size.Y)
                {
                    localPosition.Y = MathHelper.SmoothDamp(localPosition.Y, this.contentSize.Y - this.size.Y, ref this.velocityY, this.ElasticTime, (float)gameTime.TotalSeconds);
                }
            }

            this.contentTransform.LocalPosition = localPosition;
        }

        private void InternalUpdateBarPositionOnUpdateCycle(bool isHorizontalBar)
        {
            var targetTransform = isHorizontalBar ? this.horizontalScrollBarTransform : this.verticalScrollBarTransform;
            var barPosition = targetTransform.LocalPosition;
            float contentDeltaNormalized = isHorizontalBar
                ? MathHelper.Clamp(this.contentTransform.LocalPosition.X / (this.size.X - this.contentSize.X), 0, 1)
                : MathHelper.Clamp(this.contentTransform.LocalPosition.Y / (this.contentSize.Y - this.size.Y), 0, 1);

            if (isHorizontalBar)
            {
                barPosition.X = MathHelper.Lerp(-this.size.X / 2, (this.size.X / 2) - targetTransform.Scale.X, contentDeltaNormalized);
            }
            else
            {
                barPosition.Y = MathHelper.Lerp(this.size.Y / 2, -(this.size.Y / 2) + targetTransform.Scale.Y, contentDeltaNormalized);
            }

            targetTransform.LocalPosition = barPosition;
        }

        /// <summary>
        /// Scrolls the scrollview to a position.
        /// </summary>
        /// <param name="position">Position of the scroll area.</param>
        public void ScrollTo(Vector2 position)
        {
            var contentPosition = this.contentTransform.LocalPosition;
            contentPosition.X = position.X;
            contentPosition.Y = position.Y;
            this.contentTransform.LocalPosition = contentPosition;
            this.Scrolled?.Invoke(this, EventArgs.Empty);
        }

        private void UpdateSize()
        {
            this.scrollAreaCollider.Size = new Vector3(this.size.X, this.size.Y, this.scrollAreaCollider.Size.Z);
            this.backgroundPlaneTransform.LocalScale = new Vector3(this.size.X, this.size.Y, this.backgroundPlaneTransform.LocalScale.Z);
            this.Refresh();
        }

        private void UpdateDisplayScrollBarsVisibility()
        {
            if (!this.IsAttached)
            {
                return;
            }

            this.horizontalScrollBarEntity.IsEnabled = this.horizontalScrollBarVisibility != ScrollBarVisibility.Hidden;
            this.verticalScrollBarEntity.IsEnabled = this.verticalScrollBarVisibility != ScrollBarVisibility.Hidden;
            this.Refresh();
        }

        private void UpdateZContentDistance()
        {
            var contentPosition = this.contentTransform.LocalPosition;
            contentPosition.Z = this.zContentDistance;
            this.contentTransform.LocalPosition = contentPosition;
        }

        private void UpdateBarWidth()
        {
            this.InnerUpdateBarWidth(true);
            this.InnerUpdateBarWidth(false);
            this.InnerUpdateScrollBarPositionAndScale(true);
            this.InnerUpdateScrollBarPositionAndScale(false);
        }

        private void InnerUpdateBarWidth(bool isHorizontalBar)
        {
            var targetTransform = isHorizontalBar ? this.horizontalScrollBarTransform : this.verticalScrollBarTransform;
            var scrollBarScale = targetTransform.LocalScale;

            if (isHorizontalBar)
            {
                scrollBarScale.Y = this.BarWidth;
            }
            else
            {
                scrollBarScale.X = this.BarWidth;
            }

            targetTransform.LocalScale = scrollBarScale;
        }

        private void MeasureContents()
        {
            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;

            var children = this.content.ChildEntities.ToArray();
            foreach (var child in children)
            {
                var transform = child.FindComponent<Transform3D>();

                if (!this.measuredContents.ContainsKey(child))
                {
                    var mesh = child.FindComponent<MeshComponent>(false);
                    if (mesh != null && transform != null)
                    {
                        var boundingBox = mesh.BoundingBox.Value;
                        boundingBox.Transform(transform.WorldTransform);
                        this.measuredContents.Add(child, (boundingBox.Max - boundingBox.Min).ToVector2());
                    }
                }

                if (this.measuredContents.ContainsKey(child))
                {
                    var origin = new Vector2(.5f, .5f);
                    if (child.FindComponent<Text3DMesh>() is Text3DMesh text3d)
                    {
                        origin = text3d.Origin;
                    }

                    var size = this.measuredContents[child];
                    minX = Math.Min(minX, transform.LocalPosition.X - (origin.X * size.X));
                    maxX = Math.Max(maxX, transform.LocalPosition.X + ((1 - origin.X) * size.X));
                    minY = Math.Min(minY, transform.LocalPosition.Y - (origin.Y * size.Y));
                    maxY = Math.Max(maxY, transform.LocalPosition.Y + ((1 - origin.Y) * size.Y));
                }
            }

            this.contentSize.X = maxX == float.MinValue || minX == float.MaxValue
                ? 0
                : Math.Abs(maxX - minX) + this.ContentPadding;
            this.contentSize.Y = maxY == float.MinValue || minY == float.MaxValue
                ? 0
                : Math.Abs(maxY - minY) + this.ContentPadding;
        }
    }
}
