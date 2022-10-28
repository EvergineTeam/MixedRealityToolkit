// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using Evergine.Common.Attributes;
using Evergine.Framework;
using Evergine.Mathematics;

namespace Evergine.MRTK.SDK.Features.Input.Constraints
{
    /// <summary>
    /// A component to hold a scale constraint that other handlers should follow.
    /// </summary>
    public class MinScaleConstraint : Component
    {
        /// <summary>
        /// Gets or sets the minimum scale allowed for content.
        /// </summary>
        [RenderProperty(Tooltip = "Minimum scale allowed for content.")]
        public Vector3 MinimumScale { get; set; } = Vector3.One * 0.05f;
    }
}
