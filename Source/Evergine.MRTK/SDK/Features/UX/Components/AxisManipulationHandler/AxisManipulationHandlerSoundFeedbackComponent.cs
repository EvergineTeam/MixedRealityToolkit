// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using System;
using Evergine.Common.Attributes;
using Evergine.Common.Audio;
using Evergine.Components.Sound;
using Evergine.Framework;

namespace Evergine.MRTK.SDK.Features.UX.Components.AxisManipulationHandler
{
    /// <summary>
    /// Component that adds sound feedback to the <see cref="axisManipulationHandler"/>.
    /// </summary>
    public class AxisManipulationHandlerSoundFeedbackComponent : Component
    {
        [BindComponent]
        private AxisManipulationHandler axisManipulationHandler = null;

        /// <summary>
        /// Gets or sets the sound to be played when a center manipulation starts.
        /// </summary>
        [RenderProperty(Tooltip = "The sound to be played when a center manipulation starts")]
        public AudioBuffer CenterManipulationStartedSound { get; set; }

        /// <summary>
        /// Gets or sets the sound to be played when a center manipulation starts.
        /// </summary>
        [RenderProperty(Tooltip = "The sound to be played when a center manipulation ends")]
        public AudioBuffer CenterManipulationEndedSound { get; set; }

        /// <summary>
        /// Gets or sets the sound to be played when an axis manipulation starts.
        /// </summary>
        [RenderProperty(Tooltip = "The sound to be played when an axis manipulation starts")]
        public AudioBuffer AxisManipulationStartedSound { get; set; }

        /// <summary>
        /// Gets or sets the sound to be played when an axis manipulation starts.
        /// </summary>
        [RenderProperty(Tooltip = "The sound to be played when an axis manipulation ends")]
        public AudioBuffer AxisManipulationEndedSound { get; set; }

        /// <summary>
        /// Gets or sets the sound to be played when a plane manipulation starts.
        /// </summary>
        [RenderProperty(Tooltip = "The sound to be played when a plane manipulation starts")]
        public AudioBuffer PlaneManipulationStartedSound { get; set; }

        /// <summary>
        /// Gets or sets the sound to be played when a plane manipulation starts.
        /// </summary>
        [RenderProperty(Tooltip = "The sound to be played when a plane manipulation ends")]
        public AudioBuffer PlaneManipulationEndedSound { get; set; }

        private SoundEmitter3D soundEmitter;

        /// <inheritdoc/>
        protected override bool OnAttached()
        {
            if (!base.OnAttached())
            {
                return false;
            }

            if (!Application.Current.IsEditor)
            {
                this.soundEmitter = this.Owner.GetOrAddComponent<SoundEmitter3D>();
            }

            return true;
        }

        /// <inheritdoc/>
        protected override void OnActivated()
        {
            base.OnActivated();

            this.axisManipulationHandler.CenterManipulationStarted += this.AxisManipulationHandler_CenterManipulationStarted;
            this.axisManipulationHandler.CenterManipulationEnded += this.AxisManipulationHandler_CenterManipulationEnded;
            this.axisManipulationHandler.AxisManipulationStarted += this.AxisManipulationHandler_AxisManipulationStarted;
            this.axisManipulationHandler.AxisManipulationEnded += this.AxisManipulationHandler_AxisManipulationEnded;
            this.axisManipulationHandler.PlaneManipulationStarted += this.AxisManipulationHandler_PlaneManipulationStarted;
            this.axisManipulationHandler.PlaneManipulationEnded += this.AxisManipulationHandler_PlaneManipulationEnded;
        }

        /// <inheritdoc/>
        protected override void OnDeactivated()
        {
            base.OnDeactivated();

            this.axisManipulationHandler.CenterManipulationStarted -= this.AxisManipulationHandler_CenterManipulationStarted;
            this.axisManipulationHandler.CenterManipulationEnded -= this.AxisManipulationHandler_CenterManipulationEnded;
            this.axisManipulationHandler.AxisManipulationStarted -= this.AxisManipulationHandler_AxisManipulationStarted;
            this.axisManipulationHandler.AxisManipulationEnded -= this.AxisManipulationHandler_AxisManipulationEnded;
            this.axisManipulationHandler.PlaneManipulationStarted -= this.AxisManipulationHandler_PlaneManipulationStarted;
            this.axisManipulationHandler.PlaneManipulationEnded -= this.AxisManipulationHandler_PlaneManipulationEnded;
        }

        private void AxisManipulationHandler_CenterManipulationStarted(object sender, EventArgs e)
        {
            Tools.PlaySound(this.soundEmitter, this.CenterManipulationStartedSound);
        }

        private void AxisManipulationHandler_CenterManipulationEnded(object sender, EventArgs e)
        {
            Tools.PlaySound(this.soundEmitter, this.CenterManipulationEndedSound);
        }

        private void AxisManipulationHandler_AxisManipulationStarted(object sender, EventArgs e)
        {
            Tools.PlaySound(this.soundEmitter, this.AxisManipulationStartedSound);
        }

        private void AxisManipulationHandler_AxisManipulationEnded(object sender, EventArgs e)
        {
            Tools.PlaySound(this.soundEmitter, this.AxisManipulationEndedSound);
        }

        private void AxisManipulationHandler_PlaneManipulationStarted(object sender, EventArgs e)
        {
            Tools.PlaySound(this.soundEmitter, this.PlaneManipulationStartedSound);
        }

        private void AxisManipulationHandler_PlaneManipulationEnded(object sender, EventArgs e)
        {
            Tools.PlaySound(this.soundEmitter, this.PlaneManipulationEndedSound);
        }
    }
}
