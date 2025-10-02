// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using System;
using Evergine.Common.Attributes;
using Evergine.Common.Audio;
using Evergine.Components.Sound;
using Evergine.Framework;

namespace Evergine.MRTK.SDK.Features.UX.Components.PressableButtons
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
        /// Gets or sets the sound to be played when the button is pressed.
        /// </summary>
        [RenderProperty(Tooltip = "The sound to be played when the button is pressed")]
        public AudioBuffer PressedSound { get; set; }

        /// <summary>
        /// Gets or sets the sound to be played when the button is released.
        /// </summary>
        [RenderProperty(Tooltip = "The sound to be played when the button is released")]
        public AudioBuffer ReleasedSound { get; set; }

        private SoundEmitter3D soundEmitter;

        /// <inheritdoc/>
        protected override bool OnAttached()
        {
            if (!base.OnAttached())
            {
                return false;
            }

            this.pressableButton.ButtonPressed += this.PressableButton_ButtonPressed;
            this.pressableButton.ButtonReleased += this.PressableButton_ButtonReleased;

            if (!Application.Current.IsEditor)
            {
                this.soundEmitter = this.Owner.GetOrAddComponent<SoundEmitter3D>();
            }

            return true;
        }

        /// <inheritdoc/>
        protected override void OnDetached()
        {
            base.OnDetached();

            this.pressableButton.ButtonPressed -= this.PressableButton_ButtonPressed;
            this.pressableButton.ButtonReleased -= this.PressableButton_ButtonReleased;
        }

        private void PressableButton_ButtonPressed(object sender, EventArgs args)
        {
            Tools.PlaySound(this.soundEmitter, this.PressedSound);
        }

        private void PressableButton_ButtonReleased(object sender, EventArgs args)
        {
            Tools.PlaySound(this.soundEmitter, this.ReleasedSound);
        }
    }
}
