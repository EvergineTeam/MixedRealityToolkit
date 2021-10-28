// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using System;
using Evergine.Framework;
using Evergine.MRTK.Base.EventDatum.Input;
using Evergine.MRTK.Base.Interfaces.InputSystem.Handlers;

namespace Evergine.MRTK.SDK.Features.Input.Handlers
{
    /// <summary>
    /// Component used to raise events in response to pointer events.
    /// </summary>
    public class PointerHandler : Component, IMixedRealityPointerHandler
    {
        /// <summary>
        /// Event raised on pointer clicked.
        /// </summary>
        public event EventHandler<MixedRealityPointerEventData> OnPointerClicked;

        /// <summary>
        /// Event raised on pointer down.
        /// </summary>
        public event EventHandler<MixedRealityPointerEventData> OnPointerDown;

        /// <summary>
        /// Event raised every frame the pointer is down.
        /// </summary>
        public event EventHandler<MixedRealityPointerEventData> OnPointerDragged;

        /// <summary>
        /// Event raised on pointer up.
        /// </summary>
        public event EventHandler<MixedRealityPointerEventData> OnPointerUp;

        /// <inheritdoc/>
        void IMixedRealityPointerHandler.OnPointerClicked(MixedRealityPointerEventData eventData)
        {
            this.OnPointerClicked?.Invoke(this, eventData);
        }

        /// <inheritdoc/>
        void IMixedRealityPointerHandler.OnPointerDown(MixedRealityPointerEventData eventData)
        {
            this.OnPointerDown?.Invoke(this, eventData);
        }

        /// <inheritdoc/>
        void IMixedRealityPointerHandler.OnPointerDragged(MixedRealityPointerEventData eventData)
        {
            this.OnPointerDragged?.Invoke(this, eventData);
        }

        /// <inheritdoc/>
        void IMixedRealityPointerHandler.OnPointerUp(MixedRealityPointerEventData eventData)
        {
            this.OnPointerUp?.Invoke(this, eventData);
        }
    }
}
