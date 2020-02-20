// Copyright © 2019 Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using WaveEngine.Common.Attributes;
using WaveEngine.Common.Audio;
using WaveEngine.Common.Media;
using WaveEngine.Components.Sound;
using WaveEngine.Framework;

namespace WaveEngine.MRTK.SDK.Features.Input.Handlers.Manipulation
{
    /// <summary>
    /// The manipulation handler sound feedback component.
    /// </summary>
    public class ManipulationHandlerSoundFeedbackComponent : Component
    {
        /// <summary>
        /// The manilupation handler.
        /// </summary>
        [BindComponent]
        protected SimpleManipulationHandler manipulationHandler;

        /// <summary>
        /// The sound emitter.
        /// </summary>
        [BindComponent]
        protected SoundEmitter3D soundEmitter;

        /// <summary>
        /// Gets or sets the sound to be played when the manipulation is started.
        /// </summary>
        [RenderProperty(Tooltip = "The sound to be played when the manipulation is started")]
        public AudioBuffer ManipulationStartedSound { get; set; }

        /// <summary>
        /// Gets or sets the sound to be played when the manipulation is ended.
        /// </summary>
        [RenderProperty(Tooltip = "The sound to be played when the manipulation is ended")]
        public AudioBuffer ManipulationEndedSound { get; set; }

        /// <inheritdoc/>
        protected override bool OnAttached()
        {
            var attached = base.OnAttached();

            if (attached)
            {
                this.manipulationHandler.ManipulationStarted += this.ManipulationHandler_ManipulationStarted;
                this.manipulationHandler.ManipulationEnded += this.ManipulationHandler_ManipulationEnded;
            }

            return attached;
        }

        /// <inheritdoc/>
        protected override void OnDetach()
        {
            base.OnDetach();

            this.manipulationHandler.ManipulationStarted -= this.ManipulationHandler_ManipulationStarted;
            this.manipulationHandler.ManipulationEnded -= this.ManipulationHandler_ManipulationEnded;
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
