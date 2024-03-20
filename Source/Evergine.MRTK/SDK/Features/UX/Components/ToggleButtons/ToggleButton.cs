// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using System;
using Evergine.Framework;
using Evergine.MRTK.SDK.Features.UX.Components.States;

namespace Evergine.MRTK.SDK.Features.UX.Components.ToggleButtons
{
    /// <summary>
    /// Toggle button component.
    /// </summary>
    public class ToggleButton : Component
    {
        private ToggleStateManager stateManager;

        /// <summary>
        /// Raised when toggle status changes.
        /// </summary>
        public event EventHandler Toggled;

        /// <summary>
        /// Gets a value indicating whether button is on or not.
        /// </summary>
        public bool IsOn
        {
            get => this.IsOnState();
            set => this.SetIsOnState(value);
        }

        /// <inheritdoc />
        protected override bool OnAttached()
        {
            bool attached = base.OnAttached();
            if (attached)
            {
                this.AddComponents();
                this.SubscribeEvents();
            }

            return attached;
        }

        /// <inheritdoc />
        protected override void OnDetach()
        {
            base.OnDetach();
            this.UnsubscribeEvents();
        }

        private void AddComponents()
        {
            this.stateManager = this.Owner.FindComponent<ToggleStateManager>(isExactType: false);
            if (this.stateManager == null)
            {
                this.stateManager = new ToggleStateManager();
                this.Owner.AddComponent(this.stateManager);
            }
        }

        private bool IsOnState()
        {
            var toggleState = this.GetToggleState();
            return toggleState?.Value == ToggleState.On;
        }

        private void SetIsOnState(bool isOn)
        {
            if (this.stateManager != null)
            {
                this.stateManager.ChangeState(isOn ? ToggleStateManager.State_On : ToggleStateManager.State_Off);
            }
        }

        private void SubscribeEvents()
        {
            if (this.stateManager != null)
            {
                this.stateManager.StateChanged += this.ToggleStateComponent_StateChanged;
            }
        }

        private void UnsubscribeEvents()
        {
            if (this.stateManager != null)
            {
                this.stateManager.StateChanged -= this.ToggleStateComponent_StateChanged;
            }
        }

        private void ToggleStateComponent_StateChanged(object sender, StateChangedEventArgs<ToggleState> e) =>
            this.Toggled?.Invoke(this, EventArgs.Empty);

        private State<ToggleState> GetToggleState() => this.stateManager?.CurrentState;
    }
}
