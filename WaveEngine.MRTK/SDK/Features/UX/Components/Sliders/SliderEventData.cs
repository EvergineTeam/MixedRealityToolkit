namespace WaveEngine.MRTK.SDK.Features.UX.Components.Sliders
{
    public class SliderEventData
    {
        /// <summary>
        /// The previous value of the slider
        /// </summary>
        public float OldValue { get; private set; }

        /// <summary>
        /// The current value of the slider
        /// </summary>
        public float NewValue { get; private set; }

        public SliderEventData(float oldValue, float newValue)
        {
            this.OldValue = oldValue;
            this.NewValue = newValue;
        }
    }
}
