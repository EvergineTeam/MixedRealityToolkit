// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using WaveEngine.Common.Graphics;
using WaveEngine.Components.Primitives;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Physics3D;
using WaveEngine.Framework.XR;
using WaveEngine.Mathematics;
using WaveEngine.MRTK.Behaviors;
using WaveEngine.MRTK.SDK.Features;

namespace WaveEngine.MRTK.Emulation
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
        /// The <see cref="LineMeshBase"/> of the ray.
        /// </summary>
        [BindComponent(isExactType: false, source: BindComponentSource.Children)]
        protected LineMeshBase rayLineMesh;

        private CursorTouch touchCursor;
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

            this.touchCursor = this.TouchCursorEntity.FindComponentInChildren<CursorTouch>();
            this.xrJoint = this.TouchCursorEntity.FindComponent<TrackXRJoint>();

            this.cursorCollisionCategoryMask = this.TouchCursorEntity.FindComponent<StaticBody3D>().MaskBits;

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
            if (this.xrJoint != null)
            {
                if (this.xrJoint.PoseIsValid)
                {
                    this.xrJoint.TrackedDevice.TryGetArticulatedHandJoint(XRHandJointKind.IndexProximal, out var handJoint);
                    var handPosition = handJoint.Pose.Position;
                    var measuredRayPosition = handPosition;
                    var measuredDirection = this.xrJoint.Pointer.Direction;

                    this.stabilizedRay.AddSample(new Ray(measuredRayPosition, measuredDirection));
                    ray = new Ray(this.stabilizedRay.StabilizedPosition, this.stabilizedRay.StabilizedDirection);
                }
            }
            else
            {
                var touchCursorWorldTransform = this.touchCursorTransform.WorldTransform;
                ray = new Ray(touchCursorWorldTransform.Translation, touchCursorWorldTransform.Forward);
            }

            if (this.xrJoint?.PoseIsValid == true)
            {
                var jointOrientation = Matrix4x4.CreateFromQuaternion(this.xrJoint.Pose.Orientation);
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
                            }

                            this.farPointerInteractedEntity = interactedEntity;

                            if (this.farPointerInteractedEntity != null)
                            {
                                this.AddPointerInteraction(this.farPointerInteractedEntity);
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
                disableByJointInvalid = this.xrJoint != null && !Tools.IsJointValid(this.xrJoint);

                rayVisible = !disableByJointInvalid && !disableByProximity && !disableByPalmUp;
                if (rayVisible)
                {
                    var distance = (this.transform.Position - r.Position).Length();

                    this.lineMeshTransform.Position = r.Position;
                    this.lineMeshTransform.Scale = new Vector3(1, 1, distance);
                    this.lineMeshTransform.LookAt(this.transform.Position, Vector3.Up);
                    this.rayLineMesh.TextureTiling = new Vector2(distance * 0.5f * 30.0f, 1.0f);
                }
            }

            this.rayLineMesh.Owner.IsEnabled = rayVisible;
            this.IsVisible = rayVisible;
            this.touchCursor.IsVisible = !disableByJointInvalid && !disableByPalmUp && !rayVisible;
            this.touchCursor.IsEnabled = !this.Pinch;

            this.Pinch = rayVisible && this.touchCursor.Pinch;
            if (this.Pinch != this.PreviousPinch)
            {
                this.rayLineMesh.DiffuseTexture = this.Pinch ? null : this.handrayTexture;
            }

            base.Update(gameTime);
        }
    }
}
