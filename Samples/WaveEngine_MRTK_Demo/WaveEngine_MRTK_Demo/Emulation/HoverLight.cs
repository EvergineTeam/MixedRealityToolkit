using System.Collections.Generic;
using WaveEngine.Common.Graphics;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;

namespace WaveEngine_MRTK_Demo.Emulation
{
    public class HoverLight : Component
    {
        public const int MaxLights = 3;
        public static List<HoverLight> activeHoverLights = new List<HoverLight>(MaxLights);

        [BindComponent]
        public Transform3D transform = null;

        public float Radius { get; set; } = 0.15f;
        public Color Color { get; set; } = new Color(63, 63, 63, 255);

        protected override void OnActivated()
        {
            activeHoverLights.Add(this);
        }

        protected override void OnDeactivated()
        {
            activeHoverLights.Remove(this);
        }
    }
}
