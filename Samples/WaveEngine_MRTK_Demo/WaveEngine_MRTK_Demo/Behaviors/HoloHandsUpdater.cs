using System;
using System.Collections.Generic;
using System.Text;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Components.XR;
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

        public string XRModelName;

        private HoloHands holoHandsDecorator;
        private Camera3D camera;
        private Material material;

        protected override void Start()
        {
            if (!Application.Current.IsEditor)
            {
                MaterialComponent materialComponent = null;
                foreach (XRDeviceMeshComponent xrdm in this.Managers.EntityManager.FindComponentsOfType<XRDeviceMeshComponent>())
                {
                    if (xrdm.XRModelName == XRModelName)
                    {
                        materialComponent = xrdm.Owner.FindComponent<MaterialComponent>();
                        break;
                    }
                }

                materialComponent.Material = materialComponent.Material.Clone();
                material = materialComponent.Material;
                holoHandsDecorator = new HoloHands(material);
                material.ActiveDirectivesNames = new string[] { "MULTIVIEW", "PULSE" };

                camera = this.Managers.RenderManager.ActiveCamera3D;
            }
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
