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

            return base.OnAttached();
        }

        protected override void Update(TimeSpan gameTime)
        {
            holoHandsDecorator.Matrices_TPosY = transform.Position.Y;
        }
    }
}
