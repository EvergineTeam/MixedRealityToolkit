// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;

namespace WaveEngine.MRTK.SDK.Features.UX.Components.States
{
    /// <summary>
    /// Event args for state changes in a button.
    /// </summary>
    /// <typeparam name="TState">State type.</typeparam>
    public class StateChangedEventArgs<TState> : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StateChangedEventArgs{TState}"/> class.
        /// </summary>
        public StateChangedEventArgs()
            : this(null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateChangedEventArgs{TState}"/> class.
        /// </summary>
        /// <param name="oldState">Old state instance.</param>
        /// <param name="newState">New state instance.</param>
        public StateChangedEventArgs(State<TState> oldState, State<TState> newState)
        {
            this.OldState = oldState;
            this.NewState = newState;
        }

        /// <summary>
        /// Gets or sets old state instance.
        /// </summary>
        public State<TState> OldState { get; set; }

        /// <summary>
        /// Gets or sets new state instance.
        /// </summary>
        public State<TState> NewState { get; set; }
    }
}
