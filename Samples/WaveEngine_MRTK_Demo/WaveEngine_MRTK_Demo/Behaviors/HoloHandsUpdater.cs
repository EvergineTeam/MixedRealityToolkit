using System;
using System.Collections.Generic;
using System.Text;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Components.XR;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.XR;
using WaveEngine.Mathematics;
using WaveEngine_MRTK_Demo.Effects;
using WaveEngine_MRTK_Demo.Emulation;

namespace WaveEngine_MRTK_Demo.Behaviors
{
    public class HoloHandsUpdater : Behavior
    {
        public XRHandedness handedness;

        private HoloHands holoHandsDecorator;
        private Camera3D camera;
        private Material material;
        private TrackXRJoint trackXRJoint;
        private Transform3D transform;

        protected override void Start()
        {
            if (!Application.Current.IsEditor)
            {
                MaterialComponent materialComponent = this.Owner.FindComponent<MaterialComponent>(); ;
                materialComponent.Material = materialComponent.Material.Clone();
                material = materialComponent.Material;
                holoHandsDecorator = new HoloHands(material);
                material.ActiveDirectivesNames = new string[] { "MULTIVIEW", "PULSE" };

                camera = this.Managers.RenderManager.ActiveCamera3D;

                CursorManager cursorManager = Owner.Scene.Managers.FindManager<CursorManager>();
                foreach (Cursor c in cursorManager.cursors)
                {
                    TrackXRJoint joint = c.Owner.FindComponent<TrackXRJoint>();
                    if (joint != null && joint.Handedness == handedness)
                    {
                        trackXRJoint = joint;
                        transform = c.Owner.FindComponent<Transform3D>();
                        break;
                    }
                }
            }
        }

        float time = 0;
        protected override void Update(TimeSpan gameTime)
        {
            if (trackXRJoint != null)
            {
                if (!trackXRJoint.Owner.IsEnabled)
                {
                    if (time != 0)
                    {
                        time = 0;
                    }
                }
                else
                {
                    time += (float)gameTime.TotalSeconds;
                    holoHandsDecorator.Matrices_TPosY = transform.Position.Y + 0.1f - time * 0.2f;
                }
            }
        }
    }
}
