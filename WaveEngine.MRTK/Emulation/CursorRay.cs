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
    public class CursorRay : Behavior
    {
        /// <summary>
        /// The transform.
        /// </summary>
        [BindComponent]
        protected Transform3D transform = null;

        /// <summary>
        /// The cursor to move.
        /// </summary>
        [BindComponent(isRequired: true, source: BindComponentSource.Owner)]
        protected Cursor cursor;

        /// <summary>
        /// Gets or sets the reference cursor to retrieve the Pinch from.
        /// </summary>
        public Cursor MainCursor { get; set; }

        /// <summary>
        /// Gets or sets line mesh component.
        /// </summary>
        public LineBezierMesh LineMesh { get; set; }

        /// <summary>
        /// Gets or sets the Handedness.
        /// </summary>
        public XRHandedness Handedness { get; set; }

        private float pinchDist;
        private Vector3 pinchPosRef;
        private Camera3D cam;
        private Texture handrayTexture;
        private Transform3D lineMeshTransform;

        // Smoothing factor for ray stabilization.
        private const float StabilizedRayHalfLife = 0.01f;

        private readonly StabilizedRay stabilizedRay = new StabilizedRay(StabilizedRayHalfLife);

        /// <summary>
        /// Gets or sets the TrackXRJoint.
        /// </summary>
        public TrackXRJoint Joint { get; set; }

        /// <summary>
        /// The mask to check collisions.
        /// </summary>
        public CollisionCategory3D collisionMask = CollisionCategory3D.All;

        /// <inheritdoc/>
        protected override bool OnAttached()
        {
            var attached = base.OnAttached();

            this.UpdateOrder = this.MainCursor.UpdateOrder + 0.1f; // Ensure this is executed always after the main Cursor
            this.cam = this.Managers.RenderManager.ActiveCamera3D;

            this.handrayTexture = this.LineMesh.DiffuseTexture;

            this.lineMeshTransform = this.LineMesh.Owner.FindComponent<Transform3D>();

            return attached;
        }

        private readonly float CursorBeamBackwardTolerance = 0.5f;
        private readonly float CursorBeamUpTolerance = 0.8f;

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
            if (this.Joint != null)
            {
                if (this.Joint.PoseIsValid)
                {
                    this.Joint.TrackedDevice.TryGetArticulatedHandJoint(XRHandJointKind.IndexProximal, out var handJoint);
                    var handPosition = handJoint.Pose.Position;
                    var measuredRayPosition = handPosition;
                    var measuredDirection = this.Joint.Pointer.Direction;

                    this.stabilizedRay.AddSample(new Ray(measuredRayPosition, measuredDirection));
                    ray = new Ray(this.stabilizedRay.StabilizedPosition, this.stabilizedRay.StabilizedDirection);
                }
            }
            else
            {
                ray = new Ray(this.MainCursor.transform.Position, this.MainCursor.transform.WorldTransform.Forward);
            }

            if (this.Joint?.PoseIsValid == true)
            {
                var jointOrientation = Matrix4x4.CreateFromQuaternion(this.Joint.Pose.Orientation);
                var jointNormal = -jointOrientation.Up;

                var headForward = this.cam.Transform.WorldTransform.Forward;
                disableByPalmUp = !this.ShouldShowRay(headForward, jointNormal);
            }

            var disableByJointInvalid = true;
            var lineMeshVisible = false;
            if (ray != null && ray.Value.Position != Vector3.Zero)
            {
                var r = ray.Value;

                var disableByProximity = false;
                if (this.cursor.Pinch)
                {
                    float dFactor = (Vector3.Transform(this.MainCursor.transform.Position, this.cam.Transform.WorldInverseTransform) - this.pinchPosRef).Z;
                    dFactor = (float)Math.Pow(1 - dFactor, 4);

                    this.transform.Position = r.GetPoint(this.pinchDist * dFactor);
                }
                else
                {
                    if (this.MainCursor.Pinch && !this.LineMesh.Owner.IsEnabled)
                    {
                        disableByProximity = true;
                    }
                    else
                    {
                        Vector3 collPoint = r.GetPoint(1000.0f);
                        this.transform.Position = collPoint; // Move the cursor to avoid collisions
                        Vector3 from = r.GetPoint(-0.1f);
                        Vector3 to = r.GetPoint(1000.0f);
                        HitResult3D result = this.Managers.PhysicManager3D.RayCast(ref from, ref to, this.collisionMask);

                        if (result.Succeeded)
                        {
                            Vector3 dir = r.Direction;
                            dir.Normalize();
                            collPoint = result.Point + (dir * 0.0025f);
                        }

                        float dist = (r.Position - collPoint).Length();
                        if (dist > 0.3f)
                        {
                            this.transform.Position = collPoint;

                            if (this.MainCursor.Pinch)
                            {
                                // Pinch is about to happen
                                this.pinchDist = dist;
                                this.pinchPosRef = Vector3.Transform(this.MainCursor.transform.Position, this.cam.Transform.WorldInverseTransform);
                            }
                        }
                        else
                        {
                            disableByProximity = true;
                        }
                    }
                }

                // Update line
                disableByJointInvalid = this.Joint != null && !Tools.IsJointValid(this.Joint);

                lineMeshVisible = !disableByJointInvalid && !disableByProximity && !disableByPalmUp;
                if (lineMeshVisible)
                {
                    var distance = (this.transform.Position - r.Position).Length();

                    this.lineMeshTransform.Position = r.Position;
                    this.lineMeshTransform.Scale = new Vector3(1, 1, distance);
                    this.lineMeshTransform.LocalLookAt(this.transform.Position, Vector3.Up);

                    this.LineMesh.TextureTiling = new Vector2(distance * 0.5f * 30.0f, 1.0f);
                }
            }

            this.LineMesh.Owner.IsEnabled = lineMeshVisible;
            this.cursor.meshRenderer.IsEnabled = lineMeshVisible;
            this.MainCursor.meshRenderer.IsEnabled = !disableByJointInvalid && !disableByPalmUp && !lineMeshVisible;

            this.cursor.Pinch = lineMeshVisible && this.MainCursor.Pinch;
            if (this.cursor.Pinch != this.cursor.PreviousPinch)
            {
                this.LineMesh.DiffuseTexture = this.cursor.Pinch ? null : this.handrayTexture;
            }
        }
    }
}
