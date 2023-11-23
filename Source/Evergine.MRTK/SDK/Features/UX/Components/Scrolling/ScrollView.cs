// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using Evergine.Common.Graphics;
using Evergine.Components.Graphics3D;
using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.Mathematics;
using Evergine.MRTK.Base.EventDatum.Input;
using Evergine.MRTK.Base.Interfaces.InputSystem.Handlers;
using Evergine.MRTK.Emulation;
using System;
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

        [BindComponent(source: BindComponentSource.ChildrenSkipOwner, tag: "PART_scrollviewer_content")]
        private Transform3D contentTransform = null;

        [BindComponent(source: BindComponentSource.ChildrenSkipOwner, tag: "PART_scrollviewer_bar")]
        private Transform3D scrollBarTransform = null;

        [BindComponent(source: BindComponentSource.ChildrenSkipOwner, tag: "PART_scrollviewer_bar")]
        private PlaneMesh scrollBarPlaneMesh = null;

        [BindComponent(source: BindComponentSource.ChildrenSkipOwner, tag: "PART_scrollviewer_background")]
        private PlaneMesh backgroundPlaneMesh = null;

        private float ContentYMax;
        private float ContentYMin;
        private float ContentSizeY;
        private Cursor currentCursor;
        private Vector3 initialOffset;
        private Vector3 lastCursorPosition;

        private bool interacting = false;
        private float velocityY;
        private Vector3 barOrigin;

        /// <summary>
        /// Gets or sets the content padding.
        /// </summary>
        public float ContentYPadding { get; set; } = 0.01f;

        /// <summary>
        /// Gets or sets the elastic time when the content go to the edges.
        /// </summary>
        public float ElasticTime { get; set; } = 0.1f;

        /// <summary>
        /// Gets or sets z content respect to the content z.
        /// </summary>
        public float ZContentDistance { get; set; } = 0.004f;

        /// <summary>
        /// Gets or sets the bar width.
        /// </summary>
        public float BarWidth { get; set; } = 0.004f;

        /// <summary>
        /// Gets or sets a value indicating whether the debug mode is enabled or not.
        /// </summary>
        public bool Debug { get; set; }

        /// <inheritdoc/>
        protected override bool OnAttached()
        {
            var result = base.OnAttached();
            return result;
        }

        /// <inheritdoc/>
        protected override void OnActivated()
        {
            base.OnActivated();

            this.RefreshScrollBar();

            this.barOrigin = new Vector3((this.backgroundPlaneMesh.Width * 0.5f) - this.scrollBarPlaneMesh.Width, this.backgroundPlaneMesh.Height * 0.5f, this.ZContentDistance);
            this.scrollBarTransform.LocalPosition = this.barOrigin;

            // Content
            var contentPosition = this.contentTransform.LocalPosition;
            contentPosition.Z = this.ZContentDistance;
            this.contentTransform.LocalPosition = contentPosition;

            // Update bar
            this.scrollBarPlaneMesh.Width = this.BarWidth;
        }

        /// <summary>
        /// Refreshes the scroll bar's scale and position according to current content.
        /// </summary>
        /// <param name="contentSizeY">Precalculated content size Y, or null to calculate it on demand.</param>
        public void RefreshScrollBar(float? contentSizeY = null)
        {
            if (contentSizeY.HasValue)
            {
                this.ContentSizeY = contentSizeY.Value;
            }
            else
            {
                float minY = float.MaxValue;
                float maxY = float.MinValue;
                var elements = this.contentTransform.Owner.ChildEntities.ToArray();
                foreach (var element in elements)
                {
                    var mesh = element.FindComponent<MeshComponent>(false);
                    var transform = element.FindComponent<Transform3D>();
                    if (mesh != null && transform != null)
                    {
                        var boundingBox = mesh.BoundingBox.Value;
                        boundingBox.Transform(transform.WorldTransform);
                        minY = MathHelper.Min(minY, boundingBox.Min.Y);
                        maxY = MathHelper.Max(maxY, boundingBox.Max.Y);
                    }
                }

                this.ContentYMin = minY - this.ContentYPadding;
                this.ContentYMax = maxY + this.ContentYPadding;
                this.ContentSizeY = Math.Abs(this.ContentYMax - this.ContentYMin);
            }

            var barScale = this.scrollBarTransform.Scale;
            barScale.Y = this.ContentSizeY > this.backgroundPlaneMesh.Height
                ? this.backgroundPlaneMesh.Height / this.ContentSizeY
                : 1f;
            this.scrollBarTransform.Scale = barScale;
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
        protected override void Update(TimeSpan gameTime)
        {
            // Content Inertia
            if (!this.interacting)
            {
                var localPosition = this.contentTransform.LocalPosition;

                if (localPosition.Y < 0)
                {
                    localPosition.Y = MathHelper.SmoothDamp(localPosition.Y, 0, ref this.velocityY, this.ElasticTime, (float)gameTime.TotalSeconds);
                }
                else if (localPosition.Y > this.ContentSizeY - this.backgroundPlaneMesh.Height)
                {
                    localPosition.Y = MathHelper.SmoothDamp(localPosition.Y, this.ContentSizeY - this.backgroundPlaneMesh.Height, ref this.velocityY, this.ElasticTime, (float)gameTime.TotalSeconds);
                }

                this.contentTransform.LocalPosition = localPosition;
            }

            // Bar
            var barPosition = this.scrollBarTransform.LocalPosition;
            float contentDeltaNormalized = MathHelper.Clamp(this.contentTransform.LocalPosition.Y / (this.ContentSizeY - this.backgroundPlaneMesh.Height), 0, 1);
            barPosition.Y = this.barOrigin.Y - (contentDeltaNormalized * (this.backgroundPlaneMesh.Height - (this.scrollBarTransform.Scale.Y * this.scrollBarPlaneMesh.Height)));
            this.scrollBarTransform.LocalPosition = barPosition;

            // Debug mode
            if (this.Debug)
            {
                var position = this.contentTransform.Position;
                var localPosition = this.contentTransform.LocalPosition;
                var min = new Vector3(position.X - (this.backgroundPlaneMesh.Width * 0.5f), localPosition.Y + this.ContentYMin, position.Z);
                var max = new Vector3(position.X + (this.backgroundPlaneMesh.Width * 0.5f), localPosition.Y + this.ContentYMax, position.Z);
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
    }
}
