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
            this.cursor.Pinch = this.mainCursor.Pinch;

            Ray r = new Ray(this.mainCursor.transform.Position, this.mainCursor.transform.WorldTransform.Forward);
            Vector3 collPoint = r.GetPoint(1000.0f);
            this.transform.Position = collPoint;

            HitResult3D result = this.Managers.PhysicManager3D.RayCast(ref r, 1000.0f, CollisionCategory3D.All);

            if (result.Succeeded)
            {
                collPoint = result.Point;
            }

            this.transform.Position = collPoint;
        }
    }
}
