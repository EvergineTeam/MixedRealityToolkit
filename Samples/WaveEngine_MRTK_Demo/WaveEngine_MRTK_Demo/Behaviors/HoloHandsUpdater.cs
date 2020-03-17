using System;
using System.Collections.Generic;
using System.Text;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine_MRTK_Demo.Effects;

namespace WaveEngine_MRTK_Demo.Behaviors
{
    public class HoloHandsUpdater : Behavior
    {
        [BindComponent]
        protected Transform3D transform = null;

        public Material mat;

        HoloHands holoHandsDecorator;

        protected override bool OnAttached()
        {
            holoHandsDecorator = new HoloHands(mat);
            mat.ActiveDirectivesNames = new string[] { "MULTIVIEW", "PULSE" };

            return base.OnAttached();
        }

        float time = 0;
        protected override void Update(TimeSpan gameTime)
        {
            time += (float)gameTime.TotalSeconds;
            holoHandsDecorator.Matrices_TPosY = transform.Position.Y - ((float)Math.Sin(time) + 1.0f) * 0.1f;
        }
    }
}
