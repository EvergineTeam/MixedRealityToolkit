// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Mathematics;

namespace WaveEngine.MRTK.Behaviors
{
    /// <summary>
    /// Component that keep entity <see cref="Transform3D"/> scale independent to distance.
    /// </summary>
    public class CameraDistanceScale : Behavior
    {
        /// <summary>
        /// The <see cref="Transform3D"/> component dependency.
        /// </summary>
        [BindComponent]
        protected Transform3D transform;

        /// <summary>
        /// Gets or sets the scale factor to apply. Default: <c>1.0f</c>.
        /// </summary>
        public float ScaleDistanceFactor { get; set; } = 1f;

        /// <summary>
        /// Gets or sets the minimum scale to apply. Default: <c>1f</c>.
        /// </summary>
        public float MinScale { get; set; } = 1f;

        /// <summary>
        /// Gets or sets the maximum scale to apply. Default: <c>1000f</c>.
        /// </summary>
        public float MaxScale { get; set; } = 1000f;

        /// <inheritdoc/>
        protected override void Update(TimeSpan gameTime)
        {
            var camera3D = this.Managers.RenderManager.ActiveCamera3D;
            var distanceToCamera = Vector3.Distance(this.transform.Position, camera3D.Transform.Position);
            var scale = MathHelper.Clamp(distanceToCamera * this.ScaleDistanceFactor, this.MinScale, this.MaxScale);
            this.transform.LocalScale = new Vector3(scale);
        }
    }
}
