using System;
using WaveEngine.Common.Attributes;
using WaveEngine.Common.Graphics;
using WaveEngine.Common.Input.Keyboard;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Graphics.Materials;
using WaveEngine.Framework.Physics3D;
using WaveEngine.Framework.Services;
using WaveEngine_MRTK_Demo.Behaviors;

namespace WaveEngine_MRTK_Demo.Emulation
{
    public class Cursor : Behavior
    {
        [BindComponent]
        protected Transform3D transform = null;

        [BindComponent(isExactType: false)]
        public Collider3D Collider3D;

        [BindComponent]
        public StaticBody3D StaticBody3D;

        [BindComponent(isRequired: false)]
        protected MaterialComponent materialComponent;

        [BindComponent(isRequired: false)]
        protected KeyboardControlBehavior keyboardControlBehavior;

        [BindComponent(isRequired: false)]
        protected TrackXRJoint trackXRJoint;

        public Color PressedColor { get; set; }

        public Color ReleasedColor { get; set; }

        public bool UseShift { get; set; }

        [WaveIgnore]
        [DontRenderProperty]
        public bool Pinch { get; private set; }

        [WaveIgnore]
        [DontRenderProperty]
        public bool PreviousPinch { get; private set; }

        private StandardMaterial material;

        protected override bool OnAttached()
        {
            var attached = base.OnAttached();

            if (attached)
            {
                if (this.keyboardControlBehavior != null)
                {
                    this.UseShift = keyboardControlBehavior.UseShift;
                }

                if (this.materialComponent != null)
                {
                    this.material = new StandardMaterial(this.materialComponent.Material);
                }
            }

            return attached;
        }

        protected override void Update(TimeSpan gameTime)
        {
            this.PreviousPinch = this.Pinch;

            var xrPlatform = Application.Current.Container.Resolve<XRPlatform>();

            if (xrPlatform != null)
            {
                // HoloLens 2
                if (this.trackXRJoint != null
                    && this.trackXRJoint.TrackedDevice != null
                    && this.trackXRJoint.TrackedDevice.TryGetArticulatedHandJoint(WaveEngine.Framework.XR.XRHandJointKind.ThumbTip, out var joint))
                {
                    var distance = this.transform.Position - joint.Pose.Position;
                    this.Pinch = distance.Length() < 0.03f;
                }
                else
                {
                    this.Pinch = false;
                }
            }
            else
            {
                // Windows
                var graphicsPresenter = Application.Current.Container.Resolve<GraphicsPresenter>();
                var keyboardDispatcher = graphicsPresenter.FocusedDisplay.KeyboardDispatcher;

                if (keyboardDispatcher.IsKeyDown(Keys.RightShift) != this.UseShift)
                {
                    return;
                }

                if (keyboardDispatcher.ReadKeyState(Keys.P) == WaveEngine.Common.Input.ButtonState.Pressing)
                {
                    this.Pinch = !this.Pinch;
                }
            }

            if (this.Pinch != this.PreviousPinch)
            {
                this.SetColor(this.Pinch ? this.PressedColor : this.ReleasedColor);
            }
        }

        private void SetColor(Color color)
        {
            if (this.material != null)
            {
                this.material.BaseColor = color;
            }
        }
    }
}
