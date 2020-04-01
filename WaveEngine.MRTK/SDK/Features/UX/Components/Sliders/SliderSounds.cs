// Copyright © 2019 Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using WaveEngine.Common.Attributes;
using WaveEngine.Common.Audio;
using WaveEngine.Common.Media;
using WaveEngine.Components.Sound;
using WaveEngine.Framework;
using WaveEngine.Mathematics;

namespace WaveEngine.MRTK.SDK.Features.UX.Components.Sliders
{
    /// <summary>
    /// The slider sound behavior.
    /// </summary>
    public class SliderSounds : Behavior
    {
        /// <summary>
        /// The pinch slider.
        /// </summary>
        [BindComponent]
        protected PinchSlider pinchSlider;

        /// <summary>
        /// Gets or sets the sound to be played when interaction with the slider starts.
        /// </summary>
        [RenderProperty(Tooltip = "The sound to be played when interaction with the slider starts")]
        public AudioBuffer InteractionStartSound { get; set; }

        /// <summary>
        /// Gets or sets the sound to be played when interaction with the slider ends.
        /// </summary>
        [RenderProperty(Tooltip = "The sound to be played when interaction with the slider ends")]
        public AudioBuffer InteractionEndSound { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to play 'tick tick' sounds as the slider passes notches.
        /// </summary>
        [RenderProperty(Tooltip = "Whether to play 'tick tick' sounds as the slider passes notches")]
        public bool PlayTickSounds { get; set; } = true;

        /// <summary>
        /// Gets or sets the sound to play when slider passes a notch.
        /// </summary>
        [RenderProperty(Tooltip = "Sound to play when slider passes a notch")]
        public AudioBuffer PassNotchSound { get; set; }

        /// <summary>
        /// Gets or sets the amount the slider value has to change to play the tick sound.
        /// </summary>
        [RenderPropertyAsFInput(Tooltip = "The amount the slider value has to change to play the tick sound", MinLimit = 0, MaxLimit = 1)]
        public float TickEvery { get; set; } = 0.1f;

        /// <summary>
        /// Gets or sets the pitch the tick sound will have at the lowest slider value.
        /// </summary>
        [RenderProperty(Tooltip = "The pitch the tick sound will have at the lowest slider value")]
        public float StartPitch { get; set; } = 0.75f;

        /// <summary>
        /// Gets or sets the pitch the tick sound will have at the highest slider value.
        /// </summary>
        [RenderProperty(Tooltip = "The pitch the tick sound will have at the highest slider value")]
        public float EndPitch { get; set; } = 1.25f;

        /// <summary>
        /// Gets or sets the minimum time in seconds to wait between every tick sound.
        /// </summary>
        [RenderProperty(Tooltip = "The minimum time in seconds to wait between every tick sound")]
        public float MinSecondsBetweenTicks { get; set; } = 0.1f;

        private float accumulatedDeltaSliderValue;
        private float lastSoundPlayTime;
        private float timeSinceStart;
        private SoundEmitter3D soundEmitter;

        /// <inheritdoc/>
        protected override bool OnAttached()
        {
            var attached = base.OnAttached();

            if (attached)
            {
                this.pinchSlider.InteractionStarted += this.PinchSlider_InteractionStarted;
                this.pinchSlider.InteractionEnded += this.PinchSlider_InteractionEnded;
                this.pinchSlider.ValueUpdated += this.PinchSlider_ValueUpdated;

                if (!Application.Current.IsEditor)
                {
                    this.soundEmitter = this.Owner.GetOrAddComponent<SoundEmitter3D>();
                }
            }

            return attached;
        }

        /// <inheritdoc/>
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
                Tools.PlaySound(this.soundEmitter, this.InteractionStartSound);
            }
        }

        private void PinchSlider_InteractionEnded(object sender, EventArgs e)
        {
            if (this.InteractionEndSound != null)
            {
                Tools.PlaySound(this.soundEmitter, this.InteractionEndSound);
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
                    Tools.PlaySound(this.soundEmitter, this.PassNotchSound, pitch);

                    this.accumulatedDeltaSliderValue = 0;
                    this.lastSoundPlayTime = this.timeSinceStart;
                }
            }
        }

        /// <inheritdoc/>
        protected override void Update(TimeSpan gameTime)
        {
            this.timeSinceStart += (float)gameTime.TotalSeconds;
        }
    }
}
