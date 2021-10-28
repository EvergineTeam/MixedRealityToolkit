// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using System.Collections.Generic;
using Evergine.Common.Attributes;
using Evergine.Common.Graphics;
using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.Mathematics;
using Evergine.MRTK.Effects;

namespace Evergine.MRTK.Emulation
{
    /// <summary>
    /// Utility component to animate and visualize a light that can be used with
    /// the <see cref="HoloGraphic"/> shader <see cref="HoloGraphic.HoverLightDirective"/> feature.
    /// </summary>
    public class HoverLight : Component
    {
        /// <summary>
        /// Maximum number of Hover Lights allowed.
        /// </summary>
        internal const int MaxLights = 3;

        /// <summary>
        /// Active Hover lights.
        /// </summary>
        internal static List<HoverLight> ActiveHoverLights = new List<HoverLight>(MaxLights);

        /// <summary>
        /// The <see cref="Transform3D"/> component dependency.
        /// </summary>
        [BindComponent]
        protected Transform3D transform;

        /// <summary>
        /// Gets the position of the <see cref="HoverLight"/>.
        /// </summary>
        public Vector3 Position => this.transform.Position;

        /// <summary>
        /// Gets or sets the radius of the <see cref="HoverLight"/> effect.
        /// </summary>
        [RenderPropertyAsFInput(minLimit: 0, maxLimit: 1, Tooltip = "Specifies the radius of the HoverLight effect.")]
        public float Radius { get; set; } = 0.15f;

        /// <summary>
        /// Gets or sets the highlight color.
        /// </summary>
        [RenderProperty(Tooltip = "Specifies the highlight color.")]
        public Color Color { get; set; } = new Color(63, 63, 63, 255);

        /// <inheritdoc/>
        protected override void OnActivated()
        {
            ActiveHoverLights.Add(this);
        }

        /// <inheritdoc/>
        protected override void OnDeactivated()
        {
            ActiveHoverLights.Remove(this);
        }
    }
}
