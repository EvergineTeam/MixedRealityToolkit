// Copyright © 2019 Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System.Collections.Generic;
using System.Linq;
using WaveEngine.Common.Attributes;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Mathematics;
using WaveEngine.MixedReality.Toolkit.Input;
using WaveEngine.MRTK.Services.InputSystem;

namespace WaveEngine.MRTK.SDK.Features.UX.Components.PressableButtons
{
    /// <summary>
    /// Represent a object that can be pressed.
    /// </summary>
    public abstract class PressableObject : Behavior, IMixedRealityTouchHandler
    {
        /// <summary>
        /// The transform component.
        /// </summary>
        [BindComponent]
        protected Transform3D transform = null;

        /// <summary>
        /// The near interaction touchable component.
        /// </summary>
        [BindComponent]
        protected NearInteractionTouchable nearInteractionTouchable;

        /// <summary>
        /// Gets or sets the distance at which the object will start reacting to presses. This distance is local to the BoxCollider and projected onto the LocalPressDirection, and ranges from 0.5 to -0.5.
        /// </summary>
        [RenderProperty(Tooltip = "The distance at which the object will start reacting to presses. This distance is local to the BoxCollider and projected onto the LocalPressDirection, and ranges from 0.5 to -0.5")]
        public float StartPosition { get; set; } = 0.5f;

        /// <summary>
        /// Gets or sets the distance at which the object will issue a release event when in a pressed state. This distance is local to the BoxCollider and projected onto the LocalPressDirection, and ranges from 0.5 to -0.5.
        /// </summary>
        [RenderProperty(Tooltip = "The distance at which the object will issue a release event when in a pressed state. This distance is local to the BoxCollider and projected onto the LocalPressDirection, and ranges from 0.5 to -0.5")]
        public float ReleasePosition { get; set; } = 0.3f;

        /// <summary>
        /// Gets or sets distance at which the object will issue a press event when in a released state. This distance is local to the BoxCollider and projected onto the LocalPressDirection, and ranges from 0.5 to -0.5.
        /// </summary>
        [RenderProperty(Tooltip = "The distance at which the object will issue a press event when in a released state. This distance is local to the BoxCollider and projected onto the LocalPressDirection, and ranges from 0.5 to -0.5")]
        public float PressPosition { get; set; } = 0.1f;

        /// <summary>
        /// Gets or sets distance at which the object will stop reacting to presses. This distance is local to the BoxCollider and projected onto the LocalPressDirection, and ranges from 0.5 to -0.5.
        /// </summary>
        [RenderProperty(Tooltip = "The distance at which the object will stop reacting to presses. This distance is local to the BoxCollider and projected onto the LocalPressDirection, and ranges from 0.5 to -0.5")]
        public float EndPosition { get; set; } = -0.1f;

        /// <summary>
        /// Gets or sets a value indicating whether the button can only be pushed from the front. Touching the button from the back or side is prevented.
        /// </summary>
        [RenderProperty(Tooltip = "Ensures that the button can only be pushed from the front. Touching the button from the back or side is prevented.")]
        public bool EnforceFrontPush { get; set; } = true;

        /// <summary>
        /// Gets a value indicating whether this object is being touched.
        /// </summary>
        protected bool IsTouching => this.cursorLocalPositions.Count > 0;

        /// <summary>
        /// The cursor local positions.
        /// </summary>
        protected Dictionary<Entity, Vector3> cursorLocalPositions = new Dictionary<Entity, Vector3>();

        /// <summary>
        /// The cursor distances.
        /// </summary>
        protected Dictionary<Entity, float> cursorDistances = new Dictionary<Entity, float>();

        /// <inheritdoc/>
        void IMixedRealityTouchHandler.OnTouchStarted(HandTrackingInputEventData eventData)
        {
            var cursor = eventData.Cursor;

            // Start presses only once
            if (this.cursorLocalPositions.ContainsKey(cursor))
            {
                return;
            }

            // Calculate cursor local transform
            var localPosition = this.GetLocalPosition(eventData.Position);

            // Calculate the press vector, obtained by keeping only the largest coordinate in the cursor local transform
            var projectionX = new Vector3(localPosition.X, 0, 0);
            var projectionY = new Vector3(0, localPosition.Y, 0);
            var projectionZ = new Vector3(0, 0, localPosition.Z);

            var projections = new Vector3[] { projectionX, projectionY, projectionZ };

            var longestProjection = projections
                                    .OrderByDescending(p => p.LengthSquared())
                                    .First();

            // Start touch if pressing from the set press direction or if the button can be pressed from every direction
            if (!this.EnforceFrontPush || this.PressedFromPressDirection(longestProjection))
            {
                this.UpdateCursor(cursor, localPosition);

                this.InternalOnTouchStarted(cursor);
            }
        }

        /// <inheritdoc/>
        void IMixedRealityTouchHandler.OnTouchUpdated(HandTrackingInputEventData eventData)
        {
            var cursor = eventData.Cursor;

            if (this.cursorLocalPositions.ContainsKey(cursor))
            {
                var localTransform = this.GetLocalPosition(eventData.Position);

                this.UpdateCursor(cursor, localTransform);

                this.InternalOnTouchUpdated(cursor);
            }
        }

        /// <inheritdoc/>
        void IMixedRealityTouchHandler.OnTouchCompleted(HandTrackingInputEventData eventData)
        {
            var cursor = eventData.Cursor;

            if (this.cursorLocalPositions.ContainsKey(cursor))
            {
                this.InternalOnTouchCompleted(cursor);

                this.cursorLocalPositions.Remove(cursor);
                this.cursorDistances.Remove(cursor);
            }
        }

        /// <summary>
        /// Method invoked when the touch is started.
        /// </summary>
        /// <param name="cursor">The cursor.</param>
        protected virtual void InternalOnTouchStarted(Entity cursor)
        {
        }

        /// <summary>
        /// Method invoked when the touch is updated.
        /// </summary>
        /// <param name="cursor">The cursor.</param>
        protected virtual void InternalOnTouchUpdated(Entity cursor)
        {
        }

        /// <summary>
        /// Method invoked when the touch is completed.
        /// </summary>
        /// <param name="cursor">The cursor.</param>
        protected virtual void InternalOnTouchCompleted(Entity cursor)
        {
        }

        /// <inheritdoc/>
        protected override void OnDeactivated()
        {
            base.OnDeactivated();

            this.cursorLocalPositions.Clear();
            this.cursorDistances.Clear();
        }

        /// <summary>
        /// Check the button state with the specified values.
        /// </summary>
        /// <param name="currentPressedState">The current press state.</param>
        /// <param name="currentPosition">The current position.</param>
        /// <param name="newPressedState">The new press state.</param>
        /// <returns>If the state has been changed.</returns>
        protected bool GetNewPressedState(bool currentPressedState, float currentPosition, out bool newPressedState)
        {
            if (!currentPressedState && currentPosition < this.PressPosition)
            {
                newPressedState = true;
                return true;
            }
            else if (currentPressedState && currentPosition > this.ReleasePosition)
            {
                newPressedState = false;
                return true;
            }

            newPressedState = false;
            return false;
        }

        private bool PressedFromPressDirection(Vector3 longestProjection)
        {
            var projectionOnCurrentPressDirection = this.ProjectOnPressDirection(longestProjection);
            return MathHelper.FloatEquals(projectionOnCurrentPressDirection, longestProjection.Length());
        }

        private Vector3 GetLocalPosition(Vector3 position)
        {
            var matrix = this.transform.WorldInverseTransform * this.nearInteractionTouchable.BoxCollider3DTransformInverse;
            return Vector3.TransformCoordinate(position, matrix);
        }

        private float ProjectOnPressDirection(Vector3 position)
        {
            return position.Dot(this.nearInteractionTouchable.LocalPressDirection);
        }

        private void UpdateCursor(Entity cursor, Vector3 localPosition)
        {
            this.cursorLocalPositions[cursor] = localPosition;
            this.cursorDistances[cursor] = this.ProjectOnPressDirection(localPosition);
        }
    }
}
