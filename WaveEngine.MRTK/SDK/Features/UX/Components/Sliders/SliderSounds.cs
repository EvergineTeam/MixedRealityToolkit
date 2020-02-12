using System;
using WaveEngine.Common.Attributes;
using WaveEngine.Common.Audio;
using WaveEngine.Common.Media;
using WaveEngine.Components.Sound;
using WaveEngine.Framework;
using WaveEngine.Mathematics;

namespace WaveEngine.MRTK.SDK.Features.UX.Components.Sliders
{
    public class SliderSounds : Behavior
    {
        [BindComponent]
        protected PinchSlider pinchSlider;

        [BindComponent]
        protected SoundEmitter3D soundEmitter;

        [RenderProperty(Tooltip = "The sound to be played when interaction with the slider starts")]
        public AudioBuffer InteractionStartSound { get; set; }

        [RenderProperty(Tooltip = "The sound to be played when interaction with the slider ends")]
        public AudioBuffer InteractionEndSound { get; set; }

        [RenderProperty(Tooltip = "Whether to play 'tick tick' sounds as the slider passes notches")]
        public bool PlayTickSounds { get; set; } = true;

        [RenderProperty(Tooltip = "Sound to play when slider passes a notch")]
        public AudioBuffer PassNotchSound { get; set; }

        [RenderPropertyAsFInput(Tooltip = "The amount the slider value has to change to play the tick sound", MinLimit = 0, MaxLimit = 1)]
        public float TickEvery { get; set; } = 0.1f;

        [RenderProperty(Tooltip = "The pitch the tick sound will have at the lowest slider value")]
        public float StartPitch { get; set; } = 0.75f;

        [RenderProperty(Tooltip = "The pitch the tick sound will have at the highest slider value")]
        public float EndPitch { get; set; } = 1.25f;

        [RenderProperty(Tooltip = "The minimum time in seconds to wait between every tick sound")]
        public float MinSecondsBetweenTicks { get; set; } = 0.1f;

        private float accumulatedDeltaSliderValue;
        private float lastSoundPlayTime;
        private float timeSinceStart;

        protected override bool OnAttached()
        {
            var attached = base.OnAttached();

            if (attached)
            {
                this.pinchSlider.InteractionStarted += this.PinchSlider_InteractionStarted;
                this.pinchSlider.InteractionEnded += this.PinchSlider_InteractionEnded;
                this.pinchSlider.ValueUpdated += this.PinchSlider_ValueUpdated;
            }

            return attached;
        }

        protected override void OnActivated()
        {
            base.OnActivated();

            this.accumulatedDeltaSliderValue = 0;
            this.lastSoundPlayTime = 0;
            this.timeSinceStart = 0;
        }

        private void PinchSlider_InteractionStarted(object sender, EventArgs e)
        {
            if (this.InteractionStartSound != null)
            {
                this.PlaySound(this.InteractionStartSound);
            }
        }

        private void PinchSlider_InteractionEnded(object sender, EventArgs e)
        {
            if (this.InteractionEndSound != null)
            {
                this.PlaySound(this.InteractionEndSound);
            }
        }

        private void PinchSlider_ValueUpdated(object sender, SliderEventData eventData)
        {
            if (this.PlayTickSounds && this.PassNotchSound != null)
            {
                float delta = eventData.NewValue - eventData.OldValue;
                this.accumulatedDeltaSliderValue += Math.Abs(delta);

                var timeSinceLastSound = this.timeSinceStart - this.lastSoundPlayTime;
                if (this.accumulatedDeltaSliderValue > this.TickEvery && timeSinceLastSound > this.MinSecondsBetweenTicks)
                {
                    var pitch = MathHelper.Lerp(this.StartPitch, this.EndPitch, eventData.NewValue);
                    this.PlaySound(this.PassNotchSound, pitch);

                    this.accumulatedDeltaSliderValue = 0;
                    this.lastSoundPlayTime = this.timeSinceStart;
                }
            }
        }

        protected override void Update(TimeSpan gameTime)
        {
            this.timeSinceStart += (float)gameTime.TotalSeconds;
        }

        private void PlaySound(AudioBuffer sound, float pitch = 1.0f)
        {
            if (this.soundEmitter != null)
            {
                if (this.soundEmitter.PlayState == PlayState.Playing)
                {
                    this.soundEmitter.Stop();
                }

                this.soundEmitter.Audio = sound;
                this.soundEmitter.Pitch = pitch;

                this.soundEmitter.Play();
            }
        }
    }
}
