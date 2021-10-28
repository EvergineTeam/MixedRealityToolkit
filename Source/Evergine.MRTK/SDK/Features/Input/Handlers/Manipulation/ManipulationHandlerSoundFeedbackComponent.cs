// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using System;
using Evergine.Common.Attributes;
using Evergine.Common.Audio;
using Evergine.Components.Sound;
using Evergine.Framework;

namespace Evergine.MRTK.SDK.Features.Input.Handlers.Manipulation
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
        /// Gets or sets the sound to be played when the manipulation is started.
        /// </summary>
        [RenderProperty(Tooltip = "The sound to be played when the manipulation is started")]
        public AudioBuffer ManipulationStartedSound { get; set; }

        /// <summary>
        /// Gets or sets the sound to be played when the manipulation is ended.
        /// </summary>
        [RenderProperty(Tooltip = "The sound to be played when the manipulation is ended")]
        public AudioBuffer ManipulationEndedSound { get; set; }

        private SoundEmitter3D soundEmitter;

        /// <inheritdoc/>
        protected override bool OnAttached()
        {
            var attached = base.OnAttached();

            if (attached)
            {
                this.manipulationHandler.ManipulationStarted += this.ManipulationHandler_ManipulationStarted;
                this.manipulationHandler.ManipulationEnded += this.ManipulationHandler_ManipulationEnded;

                if (!Application.Current.IsEditor)
                {
                    this.soundEmitter = this.Owner.GetOrAddComponent<SoundEmitter3D>();
                }
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
            Tools.PlaySound(this.soundEmitter, this.ManipulationStartedSound);
        }

        private void ManipulationHandler_ManipulationEnded(object sender, EventArgs e)
        {
            Tools.PlaySound(this.soundEmitter, this.ManipulationEndedSound);
        }
    }
}
