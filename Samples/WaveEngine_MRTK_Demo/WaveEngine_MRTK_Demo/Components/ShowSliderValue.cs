using WaveEngine.Framework;
using WaveEngine.MRTK.SDK.Features.UX.Components.Sliders;
using WaveEngine_MRTK_Demo.Toolkit.Components.GUI;

namespace WaveEngine_MRTK_Demo.Components
{
    public class ShowSliderValue : Component
    {
        [BindComponent]
        protected Text3D text3D = null;

        [BindComponent(source: BindComponentSource.Parents)]
        protected PinchSlider pinchSlider;

        protected override bool OnAttached()
        {
            var attached = base.OnAttached();

            if (attached)
            {
                this.pinchSlider.ValueUpdated += this.PinchSlider_ValueUpdated;
            }

            return attached;
        }

        protected override void OnDetach()
        {
            this.pinchSlider.ValueUpdated -= this.PinchSlider_ValueUpdated; base.OnDetach();
        }

        private void PinchSlider_ValueUpdated(object sender, SliderEventData e)
        {
            this.text3D.Text = $"{e.NewValue:F2}";
        }
    }
}
