// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using System.Linq;
using WaveEngine.Common.Attributes;
using WaveEngine.Framework;
using WaveEngine.MRTK.Base.EventDatum.Input;
using WaveEngine.MRTK.Base.Interfaces.InputSystem.Handlers;

namespace WaveEngine.MRTK.SDK.Features.Input.Handlers
{
    /// <summary>
    /// A generic speech handler.
    /// </summary>
    public class SpeechHandler : Component, IMixedRealitySpeechHandler, IMixedRealityFocusHandler
    {
        /// <summary>
        ///  Gets or sets the words that will make this speech handler to trigger.
        /// </summary>
        [RenderProperty(Tooltip = "The words that will make this speech handler to trigger.")]
        public string[] SpeechKeywords { get; set; }

        /// <summary>
        /// Gets or sets the condition for this handler to fire its events.
        /// </summary>
        [RenderProperty(Tooltip = "The condition for this handler to fire its events.")]
        public SpeechHandlerFireCondition SpeechHandlerFireCondition { get; set; }

        /// <summary>
        /// Occurs whenever the <see cref="SpeechKeywords"/> are recognized.
        /// </summary>
        public event EventHandler SpeechKeywordRecognized;

        private bool hasFocus;

        /// <inheritdoc/>
        public void OnSpeechKeywordRecognized(string word)
        {
            var shouldFireEnabled = this.SpeechHandlerFireCondition == SpeechHandlerFireCondition.Global || this.IsEnabled;
            var shouldFireFocus = this.SpeechHandlerFireCondition != SpeechHandlerFireCondition.EnabledAndFocus || this.hasFocus;

            if (shouldFireEnabled && shouldFireFocus && (this.SpeechKeywords?.Contains(word) ?? false))
            {
                this.InternalOnSpeechKeywordRecognized(word);
                this.SpeechKeywordRecognized?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <inheritdoc/>
        public void OnFocusEnter(MixedRealityFocusEventData eventData)
        {
            this.hasFocus = true;
            this.InternalOnFocusEnter();
        }

        /// <inheritdoc/>
        public void OnFocusExit(MixedRealityFocusEventData eventData)
        {
            this.hasFocus = false;
            this.InternalOnFocusLeave();
        }

        /// <summary>
        /// Called when any of this <see cref="SpeechHandler"/>'s <see cref="SpeechKeywords"/> are recognized.
        /// </summary>
        /// <param name="keyword">The recognized keyword.</param>
        protected virtual void InternalOnSpeechKeywordRecognized(string keyword)
        {
        }

        /// <summary>
        /// Called when the element gets focus.
        /// </summary>
        protected virtual void InternalOnFocusEnter()
        {
        }

        /// <summary>
        /// Called when the element loses focus.
        /// </summary>
        protected virtual void InternalOnFocusLeave()
        {
        }
    }
}
