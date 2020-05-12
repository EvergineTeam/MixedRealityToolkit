// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Components.Primitives;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Physics3D;
using WaveEngine.Framework.Services;
using WaveEngine.Framework.XR;
using WaveEngine.Framework.XR.TrackedDevices;
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
        /// Gets or sets bezier.
        /// </summary>
        public LineBezierMesh Bezier { get; set; }

        /// <summary>
        /// Gets or sets the Handedness.
        /// </summary>
        public XRHandedness Handedness { get; set; }

        private float pinchDist;
        private Vector3 pinchPosRef;
        private Camera3D cam;

        /// <summary>
        /// Gets or sets the TrackXRJoint.
        /// </summary>
        public TrackXRJoint joint { get; set; }

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

            return attached;
        }

        /// <inheritdoc/>
        protected override void Update(TimeSpan gameTime)
        {
            Ray? ray;
            if (this.joint != null)
            {
                ray = this.joint.Pointer;
            }
            else
            {
                ray = new Ray(this.MainCursor.transform.Position, this.MainCursor.transform.WorldTransform.Forward);
            }

            bool disableByProximity = false;
            if (ray != null && ray.Value.Position != Vector3.Zero)
            {
                Ray r = ray.Value;

                if (this.cursor.Pinch)
                {
                    float dFactor = (Vector3.Transform(this.MainCursor.transform.Position, this.cam.Transform.WorldInverseTransform) - this.pinchPosRef).Z;
                    dFactor = (float)Math.Pow(1 - dFactor, 4);

                    this.transform.Position = r.GetPoint(this.pinchDist * dFactor);
                }
                else
                {
                    Vector3 collPoint = r.GetPoint(1000.0f);
                    this.transform.Position = collPoint; // Move the cursor to avoid collisions
                    HitResult3D result = this.Managers.PhysicManager3D.RayCast(ref r, 1000.0f, this.collisionMask);

                    if (result.Succeeded)
                    {
                        collPoint = result.Point;
                    }

                    float dist = (r.Position - collPoint).Length();
                    if (dist > 0.1f)
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

                // Update line
                bool disableByJointInvalid = false;
                if (this.joint != null)
                {
                    disableByJointInvalid = !Tools.IsJointValid(this.joint);
                }

                this.Bezier.Owner.IsEnabled = !disableByJointInvalid && !disableByProximity;

                if (this.Bezier.Owner.IsEnabled)
                {
                    this.Bezier.LinePoints[0].Position = r.Position;
                    this.Bezier.LinePoints[2].Position = this.transform.Position;
                    this.Bezier.LinePoints[1].Position = r.Position + (r.Direction * (this.transform.Position - r.Position).Length() * 0.75f);
                    this.Bezier.RefreshItems(null);

                    this.Bezier.TextureTiling = new Vector2((this.Bezier.LinePoints[0].Position - this.Bezier.LinePoints[2].Position).Length() * 30.0f, 1.0f);
                }
            }

            this.cursor.Pinch = this.MainCursor.Pinch;
        }
    }
}
