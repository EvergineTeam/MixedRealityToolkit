// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System.Collections.Generic;
using WaveEngine.Common.Graphics;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;

namespace WaveEngine_MRTK_Demo.Emulation
{
    /// <summary>
    /// Hover Light for Holographic shader.
    /// </summary>
    public class HoverLight : Component
    {
        /// <summary>
        /// Maximum number of Hover Lights allowed.
        /// </summary>
        public const int MaxLights = 3;

        /// <summary>
        /// Active Hover lights.
        /// </summary>
        public static List<HoverLight> activeHoverLights = new List<HoverLight>(MaxLights);

        /// <summary>
        /// The transform.
        /// </summary>
        [BindComponent]
        public Transform3D transform = null;

        /// <summary>
        /// Gets or sets the radisu.
        /// </summary>
        public float Radius { get; set; } = 0.15f;

        /// <summary>
        /// Gets or sets the color.
        /// </summary>
        public Color Color { get; set; } = new Color(63, 63, 63, 255);

        /// <inheritdoc/>
        protected override void OnActivated()
        {
            activeHoverLights.Add(this);
        }

        /// <inheritdoc/>
        protected override void OnDeactivated()
        {
            activeHoverLights.Remove(this);
        }
    }
}
