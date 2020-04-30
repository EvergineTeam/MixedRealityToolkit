// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Physics3D;
using WaveEngine.Framework.Services;
using WaveEngine.Mathematics;

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
        /// The reference cursor to retrieve the Pinch from.
        /// </summary>
        public Cursor mainCursor;

        private float pinchDist;
        private Vector3 pinchPosRef;

        /// <inheritdoc/>
        protected override bool OnAttached()
        {
            var attached = base.OnAttached();
            this.UpdateOrder = this.mainCursor.UpdateOrder + 0.1f; // Ensure this is executed always after the main Cursor

            return attached;
        }

        /// <inheritdoc/>
        protected override void Update(TimeSpan gameTime)
        {
            Ray r = new Ray(this.mainCursor.transform.Position, this.mainCursor.transform.WorldTransform.Forward);
            if (this.cursor.Pinch)
            {
                float dFactor = (this.mainCursor.transform.Position - this.pinchPosRef).Z;
                dFactor = (float)Math.Pow(1 - dFactor, 10);
                this.transform.Position = r.GetPoint(this.pinchDist * dFactor);
            }
            else
            {
                Vector3 collPoint = r.GetPoint(1000.0f);
                this.transform.Position = collPoint; // Move the cursor to avoid collisions
                HitResult3D result = this.Managers.PhysicManager3D.RayCast(ref r, 1000.0f, CollisionCategory3D.All);

                if (result.Succeeded)
                {
                    collPoint = result.Point;
                }

                this.transform.Position = collPoint;

                if (this.mainCursor.Pinch)
                {
                    // Pinch is about to happen
                    this.pinchDist = (this.transform.Position - this.mainCursor.transform.Position).Length();
                    this.pinchPosRef = this.mainCursor.transform.Position;
                }
            }

            this.cursor.Pinch = this.mainCursor.Pinch;
        }
    }
}
