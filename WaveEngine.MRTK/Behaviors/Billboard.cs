// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using System.Collections.Generic;
using System.Text;
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

        /// <inheritdoc/>
        protected override void Update(TimeSpan gameTime)
        {
            Camera3D cam = this.Managers.RenderManager.ActiveCamera3D;
            this.transform.Orientation = cam.Transform.Orientation;
            float t = Math.Abs(Vector3.TransformCoordinate(this.transform.Position, cam.Transform.WorldInverseTransform).Z);
            this.transform.Scale = Vector3.One * t * 1.0f;
        }
    }
}
