using System;
using System.Collections.Generic;
using System.Text;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Mathematics;
using WaveEngine_MRTK_Demo.Effects;

namespace WaveEngine_MRTK_Demo.Behaviors
{
    public class HoloHandsUpdater : Behavior
    {
        [BindComponent]
        protected Transform3D transform = null;

        public Material mat;

        private HoloHands holoHandsDecorator;
        private Camera3D camera;

        protected override bool OnAttached()
        {
            holoHandsDecorator = new HoloHands(mat);
            mat.ActiveDirectivesNames = new string[] { "MULTIVIEW", "PULSE" };

            camera = this.Managers.RenderManager.ActiveCamera3D;

            return base.OnAttached();
        }

        float time = 0;
        protected override void Update(TimeSpan gameTime)
        {
            time += (float)gameTime.TotalSeconds;
            holoHandsDecorator.Matrices_TPosY = transform.Position.Y  + 0.1f - time * 0.2f;

            Vector3 camForward = camera.Transform.WorldTransform.Forward;
            Vector3 camPosV = transform.Position - camera.Transform.Position;

            if (Vector3.Angle(camForward, camPosV) > (0.3f))
            {
                time = 0;
            }
        }
    }
}
