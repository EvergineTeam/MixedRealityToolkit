// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using System.Linq;
using WaveEngine.Common.Attributes;
using WaveEngine.Framework;

namespace WaveEngine.MRTK.SDK.Features.UX.Components.PressableButtons
{
    /// <summary>
    /// Default Speech and Focus handler for <see cref="PressableButton"/>.
    /// </summary>
    public class PressableButtonSpeechHandler : Component, ISpeechHandler, IFocusable
    {
        /// <summary>
        /// The pressable button dependency.
        /// </summary>
        [BindComponent]
        protected PressableButton pressableButton;

        /// <summary>
        /// The "See It Say It" label entity.
        /// </summary>
        protected Entity seeItSayItLabel;

        /// <summary>
        ///  Gets or sets the word that will make this button be pressed.
        /// </summary>
        public string SpeechKeyword { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the tag value used by the "See It Say It" label entity. By default: 'SeeItSayItLabel'.
        /// </summary>
        [RenderProperty(Tooltip = "the tag value used by the 'See It Say It' label entity. By default: 'SeeItSayItLabel'.")]
        public string SeeItSayItLabelTag { get; set; } = "SeeItSayItLabel";

        /// <summary>
        /// Occurs whenever the <see cref="SpeechKeyword"/> is recognized;
        /// </summary>
        public event EventHandler SpeechKeywordRecognized;

        /// <inheritdoc/>
        protected override bool OnAttached()
        {
            if (!base.OnAttached())
            {
                return false;
            }

            this.seeItSayItLabel = this.Owner.FindChildrenByTag(this.SeeItSayItLabelTag, isRecursive: true).FirstOrDefault();

            if (this.seeItSayItLabel != null)
            {
                this.seeItSayItLabel.IsEnabled = false;
            }

            return true;
        }

        /// <inheritdoc/>
        public void OnSpeechKeywordRecognized(string word)
        {
            if (this.SpeechKeyword == word)
            {
                this.pressableButton.SimulatePress();
                this.SpeechKeywordRecognized?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <inheritdoc/>
        public void OnFocusEnter()
        {
            if (this.seeItSayItLabel != null)
            {
                this.seeItSayItLabel.IsEnabled = true;
            }
        }

        /// <inheritdoc/>
        public void OnFocusExit()
        {
            if (this.seeItSayItLabel != null)
            {
                this.seeItSayItLabel.IsEnabled = false;
            }
        }
    }
}
