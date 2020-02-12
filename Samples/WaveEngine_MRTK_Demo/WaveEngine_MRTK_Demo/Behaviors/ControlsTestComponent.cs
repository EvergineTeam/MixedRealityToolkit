using System;
using WaveEngine.Common.Graphics;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Graphics.Materials;
using WaveEngine.Mathematics;
using WaveEngine.MRTK.SDK.Features.UX.Components.PressableButtons;
using WaveEngine.MRTK.SDK.Features.UX.Components.Sliders;

namespace WaveEngine_MRTK_Demo.Behaviors
{
    public class ControlsTestComponent : Component
    {
        [BindComponent]
        protected Transform3D transform;

        [BindComponent(isRequired: false)]
        protected MaterialComponent materialComponent;

        [BindComponent(isRequired: false, source: BindComponentSource.Scene)]
        protected PressableButton pressableButton;

        [BindComponent(isRequired: false, source: BindComponentSource.Scene)]
        protected PinchSlider pinchSlider;

        private StandardMaterial material;

        private Vector3 initialScale;
        private Color initialColor;
        private Color colorBeforePressing;

        protected override bool OnAttached()
        {
            var attached = base.OnAttached();

            if (attached)
            {
                if (this.materialComponent != null)
                {
                    this.material = new StandardMaterial(this.materialComponent.Material);
                }

                if (this.pressableButton != null)
                {
                    this.pressableButton.ButtonPressed += this.PressableButton_ButtonPressed;
                    this.pressableButton.ButtonReleased += this.PressableButton_ButtonReleased;
                }

                if (this.pinchSlider != null)
                {
                    this.pinchSlider.ValueUpdated += this.PinchSlider_ValueUpdated;
                }

                this.initialScale = this.transform.LocalScale;
                this.initialColor = this.material.BaseColor;
            }

            return attached;
        }

        protected override void OnDetach()
        {
            base.OnDetach();

            if (this.pressableButton != null)
            {
                this.pressableButton.ButtonPressed -= this.PressableButton_ButtonPressed;
                this.pressableButton.ButtonReleased -= this.PressableButton_ButtonReleased;
            }

            if (this.pinchSlider != null)
            {
                this.pinchSlider.ValueUpdated -= this.PinchSlider_ValueUpdated;
            }
        }

        private void PressableButton_ButtonPressed(object sender, EventArgs e)
        {
            this.colorBeforePressing = this.material.BaseColor;
            this.material.BaseColor = Color.Red;
            this.transform.LocalScale = this.initialScale * 0.9f;
        }

        private void PressableButton_ButtonReleased(object sender, EventArgs e)
        {
            this.material.BaseColor = this.colorBeforePressing;
            this.transform.LocalScale = this.initialScale;
        }

        private void PinchSlider_ValueUpdated(object sender, SliderEventData eventData)
        {
            if (this.material != null)
            {
                this.material.BaseColor = this.initialColor * eventData.NewValue;
            }
        }
    }
}
