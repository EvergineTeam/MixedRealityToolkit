using System;
using WaveEngine.Framework;
using WaveEngine.Components.Sound;
using WaveEngine.Common.Audio;
using WaveEngine.Common.Media;
using WaveEngine.Common.Attributes;

namespace WaveEngine.MRTK.SDK.Features.UX.Components.PressableButtons
{
    public class PressableButtonSoundFeedbackComponent : Component
    {
        [BindComponent]
        protected PressableButton pressableButton;

        [BindComponent]
        protected SoundEmitter3D soundEmitter;

        [RenderProperty(Tooltip = "The sound to be played when the button is pressed")]
        public AudioBuffer PressedSound { get; set; }

        [RenderProperty(Tooltip = "The sound to be played when the button is released")]
        public AudioBuffer ReleasedSound { get; set; }

        protected override bool OnAttached()
        {
            var attached = base.OnAttached();

            if (attached)
            {
                this.pressableButton.ButtonPressed += this.PressableButton_ButtonPressed;
                this.pressableButton.ButtonReleased += this.PressableButton_ButtonReleased;
            }

            return attached;
        }

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
            if (this.soundEmitter != null)
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
