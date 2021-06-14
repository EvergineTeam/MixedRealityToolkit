// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using System.Collections.Generic;
using System.Linq;
using WaveEngine.Framework;
using WaveEngine.MRTK.SDK.Features.UX.Components.PressableButtons;

namespace WaveEngine.MRTK.SDK.Features.UX.Components.States
{
    /// <summary>
    /// Handles states on a component.
    /// </summary>
    /// <typeparam name="TState">State type.</typeparam>
    public abstract class BaseStateManager<TState> : Component
    {
        [BindComponent(source: BindComponentSource.ChildrenSkipOwner, isRequired: true)]
        private PressableButton button = null;

        private State<TState> currentState;
        private List<State<TState>> allStates;

        /// <summary>
        /// Gets current state.
        /// </summary>
        public State<TState> CurrentState { get => this.currentState; }

        /// <summary>
        /// Raised when state changes.
        /// </summary>
        public event EventHandler<StateChangedEventArgs<TState>> StateChanged;

        /// <inheritdoc />
        protected override bool OnAttached()
        {
            bool attached = base.OnAttached();
            if (attached)
            {
                this.button.ButtonPressed += this.Button_ButtonPressed;
                this.allStates = this.GetStateList();
            }

            return attached;
        }

        /// <inheritdoc />
        protected override void Start()
        {
            base.Start();
            this.ChangeState(this.allStates.FirstOrDefault());
        }

        /// <inheritdoc />
        protected override void OnDetach()
        {
            base.OnDetach();
            this.button.ButtonPressed -= this.Button_ButtonPressed;
        }

        /// <summary>
        /// Child classes should load here the list of states.
        /// </summary>
        /// <returns>List of states to handle.</returns>
        protected abstract List<State<TState>> GetStateList();

        /// <summary>
        /// Retrieves next state once user presses the button.
        /// </summary>
        /// <returns>Next state.</returns>
        protected virtual State<TState> GetNextState()
        {
            return this.currentState == null
                ? this.allStates.FirstOrDefault()
                : this.allStates[(this.allStates.IndexOf(this.currentState) + 1) % this.allStates.Count];
        }

        private void ChangeState(State<TState> newState)
        {
            if (newState != this.currentState)
            {
                var oldState = this.currentState;
                this.currentState = newState;
                this.NotifyStateAware();
                this.StateChanged?.Invoke(this, new StateChangedEventArgs<TState>(oldState, newState));
            }
        }

        private void NotifyStateAware()
        {
            var allConfigurators = this.Owner
                .FindComponents(typeof(IStateAware<TState>), isExactType: false)
                .Cast<IStateAware<TState>>();
            for (int i = 0; i < allConfigurators.Count(); i++)
            {
                var current = allConfigurators.ElementAt(i);
                current.IsEnabled = current.TargetState?.Equals(this.currentState.Value) ?? false;
            }
        }

        private void Button_ButtonPressed(object sender, EventArgs e)
        {
            var newState = this.GetNextState();
            this.ChangeState(newState);
        }
    }
}
