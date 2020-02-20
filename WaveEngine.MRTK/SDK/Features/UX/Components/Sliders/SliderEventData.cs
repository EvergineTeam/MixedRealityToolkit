// Copyright © 2019 Wave Engine S.L. All rights reserved. Use is subject to license terms.

namespace WaveEngine.MRTK.SDK.Features.UX.Components.Sliders
{
    /// <summary>
    /// The slider event data.
    /// </summary>
    public class SliderEventData
    {
        /// <summary>
        /// Gets the previous value of the slider.
        /// </summary>
        public float OldValue { get; private set; }

        /// <summary>
        /// Gets the current value of the slider.
        /// </summary>
        public float NewValue { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SliderEventData"/> class.
        /// </summary>
        /// <param name="oldValue">The old slider value.</param>
        /// <param name="newValue">The new slider value.</param>
        public SliderEventData(float oldValue, float newValue)
        {
            this.OldValue = oldValue;
            this.NewValue = newValue;
        }
    }
}
