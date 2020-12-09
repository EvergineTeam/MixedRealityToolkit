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

            this.handrayTexture = this.LineMesh.DiffuseTexture;

            return attached;
        }

        /// <inheritdoc/>
        protected override void Update(TimeSpan gameTime)
        {
            Ray? ray;
            bool disableByJointInvalid = false;
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
                if (this.joint != null)
                {
                    disableByJointInvalid = !Tools.IsJointValid(this.joint);
                }

                this.LineMesh.Owner.IsEnabled = !disableByJointInvalid && !disableByProximity;

                if (this.LineMesh.Owner.IsEnabled)
                {
                    var distance = (this.transform.Position - r.Position).Length() * 0.5f;

                    this.LineMesh.LinePoints[0].Position = r.Position;
                    this.LineMesh.LinePoints[1].Position = this.transform.Position;
                    this.LineMesh.RefreshItems(null);

                    this.LineMesh.TextureTiling = new Vector2(distance * 30.0f, 1.0f);
                }
            }

            this.MainCursor.meshRenderer.IsEnabled = !disableByJointInvalid && !this.LineMesh.Owner.IsEnabled;
            this.cursor.meshRenderer.IsEnabled = !disableByJointInvalid && this.LineMesh.Owner.IsEnabled;

            this.cursor.Pinch = this.LineMesh.Owner.IsEnabled && this.MainCursor.Pinch;
            if (this.cursor.Pinch != this.cursor.PreviousPinch)
            {
                this.LineMesh.DiffuseTexture = this.cursor.Pinch ? null : this.handrayTexture;
            }
        }
    }
}
