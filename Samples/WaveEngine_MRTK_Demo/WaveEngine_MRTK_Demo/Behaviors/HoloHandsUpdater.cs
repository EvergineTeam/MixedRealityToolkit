using System;
using WaveEngine.Components.Graphics3D;
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
        

        public XRHandedness Handedness { get; set; }

        private HoloHandsLocal holoHandsDecorator;
        private Camera3D camera;
        private Material material;
        private TrackXRJoint trackXRJoint;
        private Transform3D transform;

        private float time = 0;
        private string[] directivesAnimating = { "PULSE" };
        private string[] directivesNotAnimating = { "BASE" };
        private bool isAnimating = true;

        protected override void Start()
        {
            if (!Application.Current.IsEditor)
            {
                MaterialComponent materialComponent = this.Owner.FindComponent<MaterialComponent>();
                materialComponent.Material = materialComponent.Material.Clone();
                this.material = materialComponent.Material;
                this.holoHandsDecorator = new HoloHandsLocal(this.material);
                this.material.ActiveDirectivesNames = directivesAnimating;

                this.camera = this.Managers.RenderManager.ActiveCamera3D;

                CursorManager cursorManager = Owner.Scene.Managers.FindManager<CursorManager>();
                foreach (Cursor c in cursorManager.cursors)
                {
                    TrackXRJoint joint = c.Owner.FindComponent<TrackXRJoint>();
                    if (joint != null && joint.Handedness == this.Handedness)
                    {
                        this.trackXRJoint = joint;
                        this.transform = c.Owner.FindComponent<Transform3D>();
                        break;
                    }
                }
            }
        }

        protected override void Update(TimeSpan gameTime)
        {
            if (this.trackXRJoint != null)
            {
                if (this.trackXRJoint.TrackedDevice == null || !this.trackXRJoint.TrackedDevice.IsConnected || !this.trackXRJoint.TrackedDevice.PoseIsValid)
                {
                    this.time = MathHelper.Clamp(this.time - (float)gameTime.TotalSeconds * 0.3f, 0, 1);
                }
                else
                {
                    this.time = MathHelper.Clamp(this.time + (float)gameTime.TotalSeconds * 0.3f, 0, 1);
                }
            }

            if (this.isAnimating)
            {
                this.holoHandsDecorator.Matrices_T = 1 - this.time;
            }

            bool isAnimating = this.time != 0 && this.time != 1;
            if (isAnimating != this.isAnimating)
            {
                this.material.ActiveDirectivesNames = isAnimating ? directivesAnimating : directivesNotAnimating;
                this.isAnimating = isAnimating;
            }
        }
    }
}
