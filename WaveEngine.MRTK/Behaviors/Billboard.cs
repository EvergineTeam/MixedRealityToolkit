// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Mathematics;

namespace WaveEngine.MRTK.Behaviors
{
    /// <summary>
    /// Quick billboard class.
    /// </summary>
    public class Billboard : Behavior
    {
        /// <summary>
        /// The Transform.
        /// </summary>
        [BindComponent]
        public Transform3D transform;

        /// <summary>
        /// Whether to scale or not.
        /// </summary>
        public bool scale = true;

        /// <inheritdoc/>
        protected override void Update(TimeSpan gameTime)
        {
            Camera3D cam = this.Managers.RenderManager.ActiveCamera3D;
            this.transform.Orientation = cam.Transform.Orientation;

            if (this.scale)
            {
                Vector3 direction = this.transform.Position - cam.Transform.Position;
                float t = Math.Max(1, direction.Length());
                this.transform.Scale = Vector3.One * (t / 1f);
            }
        }
    }
}
