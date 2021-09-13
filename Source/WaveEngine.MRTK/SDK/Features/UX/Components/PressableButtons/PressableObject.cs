// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using System.Collections.Generic;
using System.Linq;
using WaveEngine.Common.Attributes;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Mathematics;
using WaveEngine.MRTK.Base.EventDatum.Input;
using WaveEngine.MRTK.Base.Interfaces.InputSystem.Handlers;
using WaveEngine.MRTK.Emulation;
using WaveEngine.MRTK.Services.InputSystem;

namespace WaveEngine.MRTK.SDK.Features.UX.Components.PressableButtons
{
    /// <summary>
    /// Represent a object that can be pressed.
    /// </summary>
    public abstract class PressableObject : Behavior, IMixedRealityTouchHandler, IMixedRealityPointerHandler
    {
        private enum PressSimulationState
        {
            None,
            Pressing,
            Releasing,
        }

        private PressSimulationState simulatePressState;

        private float simulatedPressAmount;

        private MixedRealityPointerEventData simulatedPointerEventData;

        private bool touchUpdatedRan;
        private HandTrackingInputEventData forceRunTouchCompletedEventData;
        private int delayedFrames;

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
        /// Gets or sets a value indicating whether <see cref="IMixedRealityPointerHandler"/>
        /// events will be used to simulate touch press.
        /// </summary>
        [RenderProperty(Tag = 1, Tooltip = "Simulate touch press using" + nameof(IMixedRealityPointerHandler) + " events.")]
        public bool SimulateTouchWithPointers { get; set; } = true;

        /// <summary>
        /// Gets or sets the duration of the simulated touch press when <see cref="SimulateTouchWithPointers"/> is enabled.
        /// </summary>
        [RenderProperty(AttachToTag = 1, AttachToValue = true, Tooltip = "Simulated touch press duration in seconds")]
        public TimeSpan SimulatedPressDuration { get; set; } = TimeSpan.FromMilliseconds(300);

        /// <summary>
        /// Gets a value indicating whether this object is being touched.
        /// </summary>
        protected bool IsTouching => this.cursorLocalPositions.Count > 0;

        /// <summary>
        /// The cursor local positions.
        /// </summary>
        protected Dictionary<Cursor, Vector3> cursorLocalPositions = new Dictionary<Cursor, Vector3>();

        /// <summary>
        /// The cursor distances.
        /// </summary>
        protected Dictionary<Cursor, float> cursorDistances = new Dictionary<Cursor, float>();

        /// <inheritdoc/>
        void IMixedRealityTouchHandler.OnTouchStarted(HandTrackingInputEventData eventData)
        {
            this.touchUpdatedRan = false;

            var cursor = eventData.Cursor;

            // Start presses only once
            if (this.cursorLocalPositions.ContainsKey(cursor))
            {
                return;
            }

            // Calculate cursor local transform
            var localPosition = Vector3.TransformCoordinate(eventData.PreviousPosition, this.GetWorldToLocalTransform());

            // Start touch if pressing from the set press direction or if the button can be pressed from every direction
            if (!this.EnforceFrontPush || this.PressedFromPressDirection(localPosition))
            {
                this.TouchStart(cursor, localPosition);
            }
        }

        /// <inheritdoc/>
        void IMixedRealityTouchHandler.OnTouchUpdated(HandTrackingInputEventData eventData)
        {
            this.touchUpdatedRan = true;

            var cursor = eventData.Cursor;

            if (this.cursorLocalPositions.ContainsKey(cursor))
            {
                this.DoTouchUpdated(eventData);
            }
        }

        /// <inheritdoc/>
        void IMixedRealityTouchHandler.OnTouchCompleted(HandTrackingInputEventData eventData)
        {
            var cursor = eventData.Cursor;

            if (this.cursorLocalPositions.ContainsKey(cursor))
            {
                if (!this.touchUpdatedRan)
                {
                    // Delay TouchCompleted for one frame to allow for events to fire.
                    this.DoTouchUpdated(eventData);

                    this.forceRunTouchCompletedEventData = eventData;
                }
                else
                {
                    this.DoTouchCompleted(eventData);
                }
            }
        }

        private void DoTouchUpdated(HandTrackingInputEventData eventData)
        {
            var localPosition = Vector3.TransformCoordinate(eventData.Position, this.GetWorldToLocalTransform());

            this.TouchUpdate(eventData.Cursor, localPosition);
        }

        private void DoTouchCompleted(HandTrackingInputEventData eventData)
        {
            var cursor = eventData.Cursor;

            if (this.cursorLocalPositions.ContainsKey(cursor))
            {
                this.TouchComplete(cursor);
            }
        }

        /// <inheritdoc/>
        void IMixedRealityPointerHandler.OnPointerDown(MixedRealityPointerEventData eventData)
        {
            if (!this.SimulateTouchWithPointers ||
                eventData.EventHandled ||
                eventData.CurrentTarget != this.Owner)
            {
                return;
            }

            if (eventData.Cursor is CursorTouch ||
                (this.simulatePressState != PressSimulationState.None &&
                 this.simulatedPointerEventData?.Cursor != eventData.Cursor))
            {
                return;
            }

            this.simulatePressState = PressSimulationState.Pressing;
            this.simulatedPointerEventData = eventData;
            var localPressPosition = this.GetSimulatedPressStepPosition(eventData.Position, 0);
            this.TouchStart(eventData.Cursor, localPressPosition);
        }

        /// <inheritdoc/>
        void IMixedRealityPointerHandler.OnPointerDragged(MixedRealityPointerEventData eventData)
        {
            if (!this.SimulateTouchWithPointers ||
                eventData.EventHandled ||
                eventData.CurrentTarget != this.Owner ||
                this.simulatedPointerEventData?.Cursor != eventData.Cursor)
            {
                return;
            }

            this.simulatedPointerEventData = eventData;
        }

        /// <inheritdoc/>
        void IMixedRealityPointerHandler.OnPointerUp(MixedRealityPointerEventData eventData)
        {
            if (!this.SimulateTouchWithPointers ||
                eventData.EventHandled ||
                eventData.CurrentTarget != this.Owner ||
                this.simulatedPointerEventData?.Cursor != eventData.Cursor)
            {
                return;
            }

            this.simulatedPointerEventData = eventData;
            this.simulatePressState = PressSimulationState.Releasing;
        }

        /// <inheritdoc/>
        void IMixedRealityPointerHandler.OnPointerClicked(MixedRealityPointerEventData eventData)
        {
        }

        private void TouchStart(Cursor cursor, Vector3 localPosition)
        {
            this.UpdateCursor(cursor, localPosition);
            this.InternalOnTouchStarted(cursor);
        }

        private void TouchUpdate(Cursor cursor, Vector3 localPosition)
        {
            this.UpdateCursor(cursor, localPosition);
            this.InternalOnTouchUpdated(cursor);
        }

        private void TouchComplete(Cursor cursor)
        {
            this.InternalOnTouchCompleted(cursor);

            this.cursorLocalPositions.Remove(cursor);
            this.cursorDistances.Remove(cursor);
        }

        /// <summary>
        /// Method invoked when the touch is started.
        /// </summary>
        /// <param name="cursor">The cursor.</param>
        protected virtual void InternalOnTouchStarted(Cursor cursor)
        {
        }

        /// <summary>
        /// Method invoked when the touch is updated.
        /// </summary>
        /// <param name="cursor">The cursor.</param>
        protected virtual void InternalOnTouchUpdated(Cursor cursor)
        {
        }

        /// <summary>
        /// Method invoked when the touch is completed.
        /// </summary>
        /// <param name="cursor">The cursor.</param>
        protected virtual void InternalOnTouchCompleted(Cursor cursor)
        {
        }

        /// <inheritdoc/>
        protected override void OnDeactivated()
        {
            base.OnDeactivated();

            this.cursorLocalPositions.Clear();
            this.cursorDistances.Clear();
        }

        /// <inheritdoc/>
        protected override void Update(TimeSpan gameTime)
        {
            if (this.forceRunTouchCompletedEventData != null)
            {
                if (this.delayedFrames++ == 2)
                {
                    this.DoTouchCompleted(this.forceRunTouchCompletedEventData);
                    this.forceRunTouchCompletedEventData = null;
                    this.delayedFrames = 0;
                }
            }

            switch (this.simulatePressState)
            {
                default:
                    return;

                case PressSimulationState.Pressing:
                    this.simulatedPressAmount += (float)(gameTime.TotalSeconds / this.SimulatedPressDuration.TotalSeconds);
                    break;

                case PressSimulationState.Releasing:
                    this.simulatedPressAmount -= (float)(gameTime.TotalSeconds / this.SimulatedPressDuration.TotalSeconds);
                    break;
            }

            this.simulatedPressAmount = MathHelper.Clamp(this.simulatedPressAmount, 0, 1);

            var cursor = this.simulatedPointerEventData.Cursor;
            if (this.simulatedPressAmount > 0)
            {
                var localPressPosition = this.GetSimulatedPressStepPosition(this.simulatedPointerEventData.Position, this.simulatedPressAmount);
                this.TouchUpdate(cursor, localPressPosition);
            }
            else
            {
                this.simulatedPointerEventData = null;
                this.simulatePressState = PressSimulationState.None;
                this.TouchComplete(cursor);
            }
        }

        private Vector3 GetSimulatedPressStepPosition(Vector3 cursorPosition, float amount)
        {
            var worldToLocalTransform = this.GetWorldToLocalTransform();
            var localCursorPosition = Vector3.TransformCoordinate(cursorPosition, worldToLocalTransform);
            var cursorPositionZeroPlane = this.ProjectOnZeroPlane(localCursorPosition);

            var pressDirection = this.nearInteractionTouchable.LocalPressDirection;
            var startPressPosition = cursorPositionZeroPlane + (pressDirection * this.StartPosition);
            var endPressPosition = cursorPositionZeroPlane + (pressDirection * this.EndPosition);
            var pressStepPosition = Vector3.SmoothStep(startPressPosition, endPressPosition, amount);

            return pressStepPosition;
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

        private bool PressedFromPressDirection(Vector3 position)
        {
            var distance = this.ProjectOnPressDirection(position);

            return distance > 0; // should be this.StartPosition, workaround for a physics issue for now
        }

        /// <summary>
        /// Gets the world to local transform matrix.
        /// </summary>
        /// <returns>The world to local transform matrix.</returns>
        protected Matrix4x4 GetWorldToLocalTransform()
        {
            return this.transform.WorldInverseTransform * this.nearInteractionTouchable.BoxCollider3DTransformInverse;
        }

        /// <summary>
        /// Gets the local to world transform matrix.
        /// </summary>
        /// <returns>The local to world transform matrix.</returns>
        protected Matrix4x4 GetLocalToWorldTransform()
        {
            return this.nearInteractionTouchable.BoxCollider3DTransform * this.transform.WorldTransform;
        }

        /// <summary>
        /// Projects a local point onto the plane perpendicular to press direction at zero point.
        /// </summary>
        /// <param name="localPosition">The local position to project.</param>
        /// <returns>The projected vector position.</returns>
        protected Vector3 ProjectOnZeroPlane(Vector3 localPosition)
        {
            var pressDirection = this.nearInteractionTouchable.LocalPressDirection;
            return localPosition - Vector3.Multiply(localPosition, Vector3.Abs(pressDirection));
        }

        private float ProjectOnPressDirection(Vector3 position)
        {
            return position.Dot(this.nearInteractionTouchable.LocalPressDirection);
        }

        private void UpdateCursor(Cursor cursor, Vector3 localPosition)
        {
            this.cursorLocalPositions[cursor] = localPosition;
            this.cursorDistances[cursor] = this.ProjectOnPressDirection(localPosition);
        }
    }
}
