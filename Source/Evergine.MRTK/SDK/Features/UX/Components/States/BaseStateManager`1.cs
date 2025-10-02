// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using System;
using System.Collections.Generic;
using System.Linq;
using Evergine.Framework;
using Evergine.MRTK.SDK.Features.UX.Components.PressableButtons;

namespace Evergine.MRTK.SDK.Features.UX.Components.States
{
    /// <summary>
    /// Handles states on a component.
    /// </summary>
    /// <typeparam name="TState">State type.</typeparam>
    public abstract class BaseStateManager<TState> : Component
    {
        [BindComponent(source: BindComponentSource.Children, isRequired: true)]
        private PressableButton button = null;

        private State<TState> currentState;
        private List<State<TState>> allStates;
        private TState initialState;

        /// <summary>
        /// Gets or sets the initial state.
        /// </summary>
        public TState InitialState
        {
            get => this.initialState;
            set
            {
                if (this.initialState?.Equals(value) != true)
                {
                    this.initialState = value;
                    this.SetInitialState();
                }
            }
        }

        /// <summary>
        /// Gets current state.
        /// </summary>
        public State<TState> CurrentState { get => this.currentState; }

        /// <summary>
        /// Gets manager states.
        /// </summary>
        public IReadOnlyCollection<State<TState>> States => this.allStates?.AsReadOnly();

        /// <summary>
        /// Raised when state changes.
        /// </summary>
        public event EventHandler<StateChangedEventArgs<TState>> StateChanged;

        /// <summary>
        /// Changes manager current state to other state.
        /// </summary>
        /// <param name="newState">New state.</param>
        public virtual void ChangeState(State<TState> newState)
        {
            if (newState != this.currentState)
            {
                var oldState = this.currentState;
                this.currentState = newState;
                this.NotifyStateAware();
                this.StateChanged?.Invoke(this, new StateChangedEventArgs<TState>(oldState, newState));
            }
        }

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

            this.SetInitialState();
        }

        /// <inheritdoc />
        protected override void OnDetached()
        {
            base.OnDetached();
            this.button.ButtonPressed -= this.Button_ButtonPressed;
        }

        /// <summary>
        /// Child classes should load here the list of states.
        /// </summary>
        /// <returns>List of states to handle.</returns>
        protected abstract List<State<TState>> GetStateList();

        /// <summary>
        /// Controls if button state could be changed through user interaction.
        /// </summary>
        /// <returns>True if state change is allowed; false otherwise.</returns>
        protected virtual bool CanChangeState() => true;

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
            if (this.CanChangeState())
            {
                var newState = this.GetNextState();
                this.ChangeState(newState);
            }
        }

        private void SetInitialState()
        {
            if (this.allStates != null)
            {
                var initialState = this.allStates.FirstOrDefault(s => s.Value.Equals(this.InitialState));

                this.ChangeState(initialState);
            }
        }
    }
}
