using System;
using WaveEngine.Common.Graphics;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Mathematics;
using WaveEngine.MRTK.Effects;
using WaveEngine.MRTK.SDK.Features.Input.Handlers.Manipulation;
using WaveEngine.MRTK.SDK.Features.UX.Components.PressableButtons;
using WaveEngine.MRTK.SDK.Features.UX.Components.Sliders;
using WaveEngine_MRTK_Demo.Drawables;

namespace WaveEngine_MRTK_Demo.Components
{
    public class ControlsTestComponent : Component
    {
        [BindComponent]
        protected Transform3D transform;

        [BindComponent]
        protected MaterialComponent materialComponent;

        [BindComponent(isRequired: false, source: BindComponentSource.Scene)]
        protected PressableButton pressableButton;

        [BindComponent(isRequired: false, source: BindComponentSource.Scene)]
        protected PinchSlider pinchSlider;

        [BindComponent(isRequired: false)]
        protected SimpleManipulationHandler simpleManipulationHandler;

        [BindComponent(isRequired: false)]
        protected BoxColliderRenderer boxColliderRenderer;

        private HoloGraphic material;

        private Color initialColor;
        private Color colorBeforePressing;
        private Vector3 scaleBeforePressing;

        protected override bool OnAttached()
        {
            if (!base.OnAttached())
            {
                return false;
            }

            if (!HoloGraphic.IsHolographicMaterial(this.materialComponent.Material))
            {
                throw new InvalidOperationException($"Material component material should be {nameof(HoloGraphic)}.");
            }

            this.material = new HoloGraphic(this.materialComponent.Material);
            this.initialColor = this.material.Albedo;

            if (this.pressableButton != null)
            {
                this.pressableButton.ButtonPressed += this.PressableButton_ButtonPressed;
                this.pressableButton.ButtonReleased += this.PressableButton_ButtonReleased;
            }

            if (this.pinchSlider != null)
            {
                this.pinchSlider.ValueUpdated += this.PinchSlider_ValueUpdated;
            }

            if (this.simpleManipulationHandler != null)
            {
                this.simpleManipulationHandler.ManipulationStarted += SimpleManipulationHandler_ManipulationStarted;
                this.simpleManipulationHandler.ManipulationEnded += SimpleManipulationHandler_ManipulationEnded;
            }

            if (this.boxColliderRenderer != null)
            {
                this.boxColliderRenderer.IsEnabled = false;
            }

            return true;
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

            if (this.simpleManipulationHandler != null)
            {
                this.simpleManipulationHandler.ManipulationStarted -= SimpleManipulationHandler_ManipulationStarted;
                this.simpleManipulationHandler.ManipulationEnded -= SimpleManipulationHandler_ManipulationEnded;
            }
        }

        private void PressableButton_ButtonPressed(object sender, EventArgs e)
        {
            this.colorBeforePressing = this.material.Albedo;
            this.material.Albedo = Color.Red;

            this.scaleBeforePressing = this.transform.LocalScale;
            this.transform.LocalScale *= 0.9f;
        }

        private void PressableButton_ButtonReleased(object sender, EventArgs e)
        {
            this.material.Albedo = this.colorBeforePressing;
            this.transform.LocalScale = this.scaleBeforePressing;
        }

        private void PinchSlider_ValueUpdated(object sender, SliderEventData eventData)
        {
            if (this.material != null)
            {
                this.material.Albedo = this.initialColor * eventData.NewValue;
            }
        }

        private void SimpleManipulationHandler_ManipulationStarted(object sender, EventArgs e)
        {
            if (this.boxColliderRenderer != null)
            {
                this.boxColliderRenderer.IsEnabled = true;
            }
        }

        private void SimpleManipulationHandler_ManipulationEnded(object sender, EventArgs e)
        {
            if (this.boxColliderRenderer != null)
            {
                this.boxColliderRenderer.IsEnabled = false;
            }
        }
    }
}
