// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using System.Linq;
using WaveEngine.Common.Attributes;
using WaveEngine.Framework;
using WaveEngine.MRTK.SDK.Features.Input.Handlers;

namespace WaveEngine.MRTK.SDK.Features.UX.Components.PressableButtons
{
    /// <summary>
    /// Default Speech and Focus handler for <see cref="PressableButton"/>.
    /// </summary>
    public class PressableButtonSpeechHandler : SpeechHandler
    {
        /// <summary>
        /// The pressable button dependency.
        /// </summary>
        [BindComponent(source: BindComponentSource.Children)]
        protected PressableButton pressableButton;

        /// <summary>
        /// The "See It Say It" label entity.
        /// </summary>
        protected Entity seeItSayItLabel;

        /// <summary>
        /// Gets or sets the tag value used by the "See It Say It" label entity. By default: 'SeeItSayItLabel'.
        /// </summary>
        [RenderProperty(Tooltip = "The tag value used by the 'See It Say It' label entity. By default: 'SeeItSayItLabel'.")]
        public string SeeItSayItLabelTag { get; set; } = "SeeItSayItLabel";

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
        protected override void InternalOnSpeechKeywordRecognized(string keyword)
        {
            this.pressableButton.ForceFireEvents();
        }

        /// <inheritdoc/>
        protected override void InternalOnFocusEnter()
        {
            if (this.seeItSayItLabel != null)
            {
                this.seeItSayItLabel.IsEnabled = true;
            }
        }

        /// <inheritdoc/>
        protected override void InternalOnFocusLeave()
        {
            if (this.seeItSayItLabel != null)
            {
                this.seeItSayItLabel.IsEnabled = false;
            }
        }
    }
}
