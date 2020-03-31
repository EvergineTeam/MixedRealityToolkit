using System;
using WaveEngine.Common.Graphics;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Graphics.Materials;
using WaveEngine.Mathematics;
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

        [BindComponent(isRequired: false)]
        protected MaterialComponent materialComponent;

        [BindComponent(isRequired: false, source: BindComponentSource.Scene)]
        protected PressableButton pressableButton;

        [BindComponent(isRequired: false, source: BindComponentSource.Scene)]
        protected PinchSlider pinchSlider;

        [BindComponent(isRequired: false)]
        protected SimpleManipulationHandler simpleManipulationHandler;

        [BindComponent(isRequired: false)]
        protected BoxColliderRenderer boxColliderRenderer;

        private StandardMaterial material;

        private Color initialColor;
        private Color colorBeforePressing;
        private Vector3 scaleBeforePressing;

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

                if (this.simpleManipulationHandler != null)
                {
                    this.simpleManipulationHandler.ManipulationStarted += SimpleManipulationHandler_ManipulationStarted;
                    this.simpleManipulationHandler.ManipulationEnded += SimpleManipulationHandler_ManipulationEnded;
                }

                if (this.boxColliderRenderer != null)
                {
                    this.boxColliderRenderer.IsEnabled = false;
                }

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

            if (this.simpleManipulationHandler != null)
            {
                this.simpleManipulationHandler.ManipulationStarted -= SimpleManipulationHandler_ManipulationStarted;
                this.simpleManipulationHandler.ManipulationEnded -= SimpleManipulationHandler_ManipulationEnded;
            }
        }

        private void PressableButton_ButtonPressed(object sender, EventArgs e)
        {
            this.colorBeforePressing = this.material.BaseColor;
            this.material.BaseColor = Color.Red;

            this.scaleBeforePressing = this.transform.LocalScale;
            this.transform.LocalScale *= 0.9f;
        }

        private void PressableButton_ButtonReleased(object sender, EventArgs e)
        {
            this.material.BaseColor = this.colorBeforePressing;
            this.transform.LocalScale = this.scaleBeforePressing;
        }

        private void PinchSlider_ValueUpdated(object sender, SliderEventData eventData)
        {
            if (this.material != null)
            {
                this.material.BaseColor = this.initialColor * eventData.NewValue;
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
