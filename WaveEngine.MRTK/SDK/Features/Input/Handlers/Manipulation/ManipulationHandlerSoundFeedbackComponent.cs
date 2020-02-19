using System;
using WaveEngine.Common.Attributes;
using WaveEngine.Common.Audio;
using WaveEngine.Common.Media;
using WaveEngine.Components.Sound;
using WaveEngine.Framework;

namespace WaveEngine.MRTK.SDK.Features.Input.Handlers.Manipulation
{
    public class ManipulationHandlerSoundFeedbackComponent : Component
    {
        [BindComponent]
        protected SimpleManipulationHandler manipulationHandler;

        [BindComponent]
        protected SoundEmitter3D soundEmitter;

        [RenderProperty(Tooltip = "The sound to be played when the manipulation is started")]
        public AudioBuffer ManipulationStartedSound { get; set; }

        [RenderProperty(Tooltip = "The sound to be played when the manipulation is ended")]
        public AudioBuffer ManipulationEndedSound { get; set; }

        protected override bool OnAttached()
        {
            var attached = base.OnAttached();

            if (attached)
            {
                this.manipulationHandler.ManipulationStarted += ManipulationHandler_ManipulationStarted;
                this.manipulationHandler.ManipulationEnded += ManipulationHandler_ManipulationEnded;
            }

            return attached;
        }

        protected override void OnDetach()
        {
            base.OnDetach();

            this.manipulationHandler.ManipulationStarted -= ManipulationHandler_ManipulationStarted;
            this.manipulationHandler.ManipulationEnded -= ManipulationHandler_ManipulationEnded;
        }

        private void ManipulationHandler_ManipulationStarted(object sender, EventArgs e)
        {
            this.PlaySound(this.ManipulationStartedSound);
        }

        private void ManipulationHandler_ManipulationEnded(object sender, EventArgs e)
        {
            this.PlaySound(this.ManipulationEndedSound);
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
