using System.Collections.Generic;
using System.Linq;
using WaveEngine.Common.Attributes;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Mathematics;
using WaveEngine.MRTK.Base.EventDatum.Input;
using WaveEngine.MRTK.Base.Interfaces.InputSystem.Handlers;
using WaveEngine.MRTK.Services.InputSystem;

namespace WaveEngine.MRTK.SDK.Features.UX.Components.PressableButtons
{
    public abstract class PressableObject : Behavior, IMixedRealityTouchHandler
    {
        [BindComponent]
        protected Transform3D transform = null;

        [BindComponent]
        protected NearInteractionTouchable nearInteractionTouchable;

        [RenderProperty(Tooltip = "The distance at which the object will start reacting to presses. This distance is local to the BoxCollider and projected onto the LocalPressDirection, and ranges from 0.5 to -0.5")]
        public float StartPosition { get; set; } = 0.5f;

        [RenderProperty(Tooltip = "The distance at which the object will issue a release event when in a pressed state. This distance is local to the BoxCollider and projected onto the LocalPressDirection, and ranges from 0.5 to -0.5")]
        public float ReleasePosition { get; set; } = 0.3f;

        [RenderProperty(Tooltip = "The distance at which the object will issue a press event when in a released state. This distance is local to the BoxCollider and projected onto the LocalPressDirection, and ranges from 0.5 to -0.5")]
        public float PressPosition { get; set; } = 0.1f;

        [RenderProperty(Tooltip = "The distance at which the object will stop reacting to presses. This distance is local to the BoxCollider and projected onto the LocalPressDirection, and ranges from 0.5 to -0.5")]
        public float EndPosition { get; set; } = -0.1f;

        [RenderProperty(Tooltip = "Ensures that the button can only be pushed from the front. Touching the button from the back or side is prevented.")]
        public bool EnforceFrontPush { get; set; } = true;

        protected bool IsTouching => this.cursorLocalPositions.Count > 0;

        protected Dictionary<Entity, Vector3> cursorLocalPositions = new Dictionary<Entity, Vector3>();

        protected Dictionary<Entity, float> cursorDistances = new Dictionary<Entity, float>();

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

        protected virtual void InternalOnTouchStarted(Entity cursor) { }
        protected virtual void InternalOnTouchUpdated(Entity cursor) { }
        protected virtual void InternalOnTouchCompleted(Entity cursor) { }

        protected override void OnDeactivated()
        {
            base.OnDeactivated();

            this.cursorLocalPositions.Clear();
            this.cursorDistances.Clear();
        }

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
