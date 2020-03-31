// Copyright © 2019 Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using WaveEngine.Framework;
using WaveEngine.Components.Sound;
using WaveEngine.Common.Audio;
using WaveEngine.Common.Media;
using WaveEngine.Common.Attributes;

namespace WaveEngine.MRTK.SDK.Features.UX.Components.PressableButtons
{
    /// <summary>
    /// Sound feedback component of a pressable button.
    /// </summary>
    public class PressableButtonSoundFeedbackComponent : Component
    {
        /// <summary>
        /// The pressable button.
        /// </summary>
        [BindComponent]
        protected PressableButton pressableButton;

        /// <summary>
        /// A sound emitter.
        /// </summary>
        [BindComponent(isRequired: false)]
        protected SoundEmitter3D soundEmitter;

        /// <summary>
        /// Gets or sets the sound to be played when the button is pressed.
        /// </summary>
        [RenderProperty(Tooltip = "The sound to be played when the button is pressed")]
        public AudioBuffer PressedSound { get; set; }

        /// <summary>
        /// Gets or sets the sound to be played when the button is released.
        /// </summary>
        [RenderProperty(Tooltip = "The sound to be played when the button is released")]
        public AudioBuffer ReleasedSound { get; set; }

        /// <inheritdoc/>
        protected override bool OnAttached()
        {
            var attached = base.OnAttached();

            if (attached)
            {
                this.pressableButton.ButtonPressed += this.PressableButton_ButtonPressed;
                this.pressableButton.ButtonReleased += this.PressableButton_ButtonReleased;
            }

            if (!Application.Current.IsEditor && this.soundEmitter == null)
            {
                this.soundEmitter = new SoundEmitter3D();
                this.Owner.AddComponent(this.soundEmitter);
            }

            return attached;
        }

        /// <inheritdoc/>
        protected override void OnDetach()
        {
            base.OnDetach();

            this.pressableButton.ButtonPressed -= this.PressableButton_ButtonPressed;
            this.pressableButton.ButtonReleased -= this.PressableButton_ButtonReleased;
        }

        private void PressableButton_ButtonPressed(object sender, EventArgs args)
        {
            this.PlaySound(this.PressedSound);
        }

        private void PressableButton_ButtonReleased(object sender, EventArgs args)
        {
            this.PlaySound(this.ReleasedSound);
        }

        private void PlaySound(AudioBuffer sound)
        {
            if (this.soundEmitter != null && sound != null)
            {
                if (this.soundEmitter.PlayState == PlayState.Playing)
                {
                    this.soundEmitter.Stop();
                }

                this.soundEmitter.Audio = sound;

                this.soundEmitter.Play();
            }
        }
    }
}
