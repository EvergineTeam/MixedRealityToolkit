// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System.Collections.Generic;
using WaveEngine.Common.Graphics;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;

namespace WaveEngine.MRTK.Emulation
{
    internal class ProximityLight : WaveEngine.Framework.Component
    {
        // Two proximity lights are supported at this time.
        public const int MaxLights = 2;
        public static List<ProximityLight> activeProximityLights = new List<ProximityLight>(MaxLights);

        [BindComponent]
        public Transform3D transform = null;

        public float NearRadius { get; set; } = 0.05f;

        public float FarRadius { get; set; } = 0.2f;

        public float NearDistance { get; set; } = 0.02f;

        public float MinNearSizePercentage { get; set; } = 0.35f;

        public Color CenterColor { get; set; } = new Color(54, 142, 250, 0);

        public Color MiddleColor { get; set; } = new Color(47, 132, 255, 51);

        public Color OuterColor { get; set; } = new Color(82, 31, 191, 255);

        public float pulseFade { get; set; } = 0.0f;

        public float pulseTime { get; set; } = 0.0f;

        protected override void OnActivated()
        {
            activeProximityLights.Add(this);
        }

        protected override void OnDeactivated()
        {
            activeProximityLights.Remove(this);
        }
    }
}
