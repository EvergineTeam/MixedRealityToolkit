// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using WaveEngine.Framework;
using WaveEngine.MRTK.Toolkit.GUI;

namespace WaveEngine.MRTK.SDK.Features.UX.Components.Sliders
{
    /// <summary>
    /// A component to show a parent <see cref="PinchSlider"/> value.
    /// </summary>
    public class ShowSliderValue : Component
    {
        [BindComponent]
        private Text3D text3D = null;

        [BindComponent(source: BindComponentSource.Parents)]
        private PinchSlider pinchSlider = null;

        /// <inheritdoc/>
        protected override void OnActivated()
        {
            base.OnActivated();
            this.pinchSlider.ValueUpdated += this.PinchSlider_ValueUpdated;
        }

        /// <inheritdoc/>
        protected override void OnDeactivated()
        {
            base.OnDeactivated();
            this.pinchSlider.ValueUpdated -= this.PinchSlider_ValueUpdated;
        }

        private void PinchSlider_ValueUpdated(object sender, SliderEventData e)
        {
            this.text3D.Text = $"{e.NewValue:F2}";
        }
    }
}
