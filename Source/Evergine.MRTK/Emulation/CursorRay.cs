// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using System;
using Evergine.Common.Attributes;
using Evergine.Common.Graphics;
using Evergine.Components.Primitives;
using Evergine.Components.XR;
using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.Framework.Physics3D;
using Evergine.Framework.XR;
using Evergine.Mathematics;
using Evergine.MRTK.Behaviors;
using Evergine.MRTK.SDK.Features;

namespace Evergine.MRTK.Emulation
{
    /// <summary>
    /// Cursor ray.
    /// </summary>
    public class CursorRay : Cursor
    {
        // Smoothing factor for ray stabilization.
        private const float StabilizedRayHalfLife = 0.01f;

        private readonly StabilizedRay stabilizedRay = new StabilizedRay(StabilizedRayHalfLife);

        private readonly float CursorBeamBackwardTolerance = 0.5f;
        private readonly float CursorBeamUpTolerance = 0.8f;

        /// <summary>
        /// Gets or sets the maximum length of the cursor representation.
        /// </summary>
        [RenderProperty(Tooltip = "The maximum length of the cursor representation")]
        public float MaxLength { get; set; } = 0.5f;

        /// <summary>
        /// The <see cref="LineMeshBase"/> of the ray.
        /// </summary>
        [BindComponent(isExactType: false, source: BindComponentSource.Children)]
        protected LineMeshBase rayLineMesh;

        private CursorTouch touchCursor;
        private TrackXRController xrController;
        private TrackXRJoint xrJoint;
        private Transform3D lineMeshTransform;
        private Transform3D touchCursorTransform;

        private CollisionCategory3D cursorCollisionCategoryMask;

        private float pinchDist;
        private Vector3 pinchPosRef;
        private Camera3D cam;
        private Texture handrayTexture;

        private Entity farPointerInteractedEntity;

        /// <summary>
        /// Gets or sets the reference touch cursor.
        /// </summary>
        public Entity TouchCursorEntity { get; set; }

        /// <summary>
        /// Gets or sets the Handedness.
        /// </summary>
        public XRHandedness Handedness { get; set; }

        /// <inheritdoc/>
        protected override bool OnAttached()
        {
            if (!base.OnAttached() ||
                this.TouchCursorEntity == null)
            {
                return false;
            }

            this.touchCursor = this.TouchCursorEntity.FindComponentInChildren<CursorTouch>(isRecursive: true);
            this.xrController = this.TouchCursorEntity.FindComponent<TrackXRController>(isExactType: false);
            if (this.xrController is TrackXRJoint trackXRJoint)
            {
                this.xrJoint = trackXRJoint;
            }

            this.cursorCollisionCategoryMask = this.TouchCursorEntity.FindComponentInChildren<StaticBody3D>(isRecursive: true).MaskBits;

            this.UpdateOrder = this.touchCursor.UpdateOrder + 0.1f; // Ensure this is executed always after the main Cursor
            this.cam = this.Managers.RenderManager.ActiveCamera3D;

            this.handrayTexture = this.rayLineMesh.DiffuseTexture;

            return true;
        }

        /// <inheritdoc/>
        protected override void OnActivated()
        {
            base.OnActivated();
            this.lineMeshTransform = this.rayLineMesh.Owner.FindComponent<Transform3D>();
            this.touchCursorTransform = this.touchCursor.Owner.FindComponent<Transform3D>();
        }

        /// <summary>
        /// Check whether hand palm is angled in a way that hand rays should be used.
        /// </summary>
        /// <returns>Return true is the palm is not up.</returns>
        private bool ShouldShowRay(Vector3 headForward, Vector3 palmNormal)
        {
            if (headForward.Length() < MathHelper.Epsilon)
            {
                return false;
            }

            bool valid = true;
            if (this.CursorBeamBackwardTolerance >= 0)
            {
                Vector3 cameraBackward = -headForward;
                if (Vector3.Dot(Vector3.Normalize(palmNormal), cameraBackward) > this.CursorBeamBackwardTolerance)
                {
                    valid = false;
                }
            }

            if (valid && this.CursorBeamUpTolerance >= 0)
            {
                if (Vector3.Dot(palmNormal, Vector3.Up) > this.CursorBeamUpTolerance)
                {
                    valid = false;
                }
            }

            return valid;
        }

        /// <inheritdoc/>
        protected override void Update(TimeSpan gameTime)
        {
            Ray? ray = null;
            var disableByPalmUp = false;
            if (this.xrController != null)
            {
                var rayPosition = Vector3.Zero;
                if (this.xrJoint != null && this.xrJoint.PoseIsValid)
                {
                    if (this.xrJoint.TrackedDevice.TryGetArticulatedHandJoint(XRHandJointKind.IndexProximal, out var handJoint))
                    {
                        rayPosition = handJoint.Pose.Position;
                    }
                }
                else if (this.xrController.PoseIsValid)
                {
                    rayPosition = this.xrController.Pose.Position;
                }

                var measuredDirection = this.xrController.LocalPointer.Direction;

                this.stabilizedRay.AddSample(new Ray(rayPosition, measuredDirection));
                ray = new Ray(this.stabilizedRay.StabilizedPosition, this.stabilizedRay.StabilizedDirection);
            }
            else
            {
                var touchCursorWorldTransform = this.touchCursorTransform.WorldTransform;
                ray = new Ray(touchCursorWorldTransform.Translation, touchCursorWorldTransform.Forward);
            }

            if (this.xrJoint != null && this.xrJoint.TryGetArticulatedHandJoint(XRHandJointKind.Palm, out var palmJoint))
            {
                var jointOrientation = Matrix4x4.CreateFromQuaternion(palmJoint.Pose.Orientation);
                var jointNormal = -jointOrientation.Up;

                var headForward = this.cam.Transform.WorldTransform.Forward;
                disableByPalmUp = !this.ShouldShowRay(headForward, jointNormal);
            }

            var disableByJointInvalid = true;
            var rayVisible = false;
            if (ray != null && ray.Value.Position != Vector3.Zero)
            {
                var r = ray.Value;

                var disableByProximity = false;
                if (this.Pinch)
                {
                    float dFactor = (Vector3.Transform(r.Position, this.cam.Transform.WorldInverseTransform) - this.pinchPosRef).Z;
                    dFactor = (float)Math.Pow(1 - dFactor, 4);

                    this.transform.Position = r.GetPoint(this.pinchDist * dFactor);
                }
                else
                {
                    if (this.touchCursor.IsExternalTouching || (this.touchCursor.Pinch && !this.rayLineMesh.Owner.IsEnabled))
                    {
                        disableByProximity = true;
                    }
                    else
                    {
                        var result = this.Managers.PhysicManager3D.RayCast(ref r, 10, this.cursorCollisionCategoryMask);

                        var collPoint = result.Succeeded ? result.Point : r.GetPoint(10);
                        var normal = result.Succeeded ? result.Normal : Vector3.Forward;

                        var interactedEntity = result.PhysicBody?.BodyComponent?.Owner;
                        if (interactedEntity != this.farPointerInteractedEntity)
                        {
                            if (this.farPointerInteractedEntity != null)
                            {
                                this.RemovePointerInteraction(this.farPointerInteractedEntity);
                                this.RemoveFocusableInteraction(this.farPointerInteractedEntity);
                            }

                            this.farPointerInteractedEntity = interactedEntity;

                            if (this.farPointerInteractedEntity != null)
                            {
                                this.AddPointerInteraction(this.farPointerInteractedEntity);
                                this.AddFocusableInteraction(this.farPointerInteractedEntity);
                            }
                        }

                        this.transform.Position = collPoint;
                        this.transform.LookAt(collPoint + normal, Vector3.Up);

                        if (this.touchCursor.Pinch)
                        {
                            // Pinch is about to happen
                            var dist = (r.Position - collPoint).Length();
                            this.pinchDist = dist;
                            this.pinchPosRef = Vector3.Transform(r.Position, this.cam.Transform.WorldInverseTransform);
                        }
                    }
                }

                // Update line
                disableByJointInvalid = this.xrController != null && !Tools.IsJointValid(this.xrController);

                rayVisible = !disableByJointInvalid && !disableByProximity && !disableByPalmUp;
                if (rayVisible)
                {
                    var distance = (this.transform.Position - r.Position).Length();

                    var distanceClamped = Math.Min(distance, this.MaxLength);

                    this.lineMeshTransform.Position = r.Position;
                    this.lineMeshTransform.Scale = new Vector3(1, 1, distanceClamped);
                    this.lineMeshTransform.LookAt(this.transform.Position, Vector3.Up);

                    /*
                     * Some "magic numbers" comments here:
                     * - line_dots texture counts with 8 pixels, this is that first magic number value.
                     * - Second one (2.0) is a simmetry value, which origin could be transparent pixels
                     * acting as margin in texture file.
                     */
                    this.rayLineMesh.TextureTiling = new Vector2(distanceClamped * 8.0f * 2.0f, 1.0f);
                }
                else
                {
                    this.RemoveAllFocusableInteractions();
                }
            }

            this.rayLineMesh.Owner.IsEnabled = rayVisible;
            this.IsVisible = rayVisible;
            this.touchCursor.IsVisible = !disableByJointInvalid && !disableByPalmUp && !rayVisible;
            this.touchCursor.IsEnabled = !this.Pinch && !disableByPalmUp;

            this.Pinch = rayVisible && this.touchCursor.Pinch;
            if (this.Pinch != this.PreviousPinch)
            {
                this.rayLineMesh.DiffuseTexture = this.Pinch ? null : this.handrayTexture;
            }

            base.Update(gameTime);
        }
    }
}
