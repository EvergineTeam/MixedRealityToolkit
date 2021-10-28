// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

namespace Evergine.MRTK.SDK.Features.UX.Components.States
{
    /// <summary>
    /// Implement this interface in a component that should be enabled or disabled
    /// depending on current state.
    /// </summary>
    /// <typeparam name="TState">State type.</typeparam>
    public interface IStateAware<TState>
    {
        /// <summary>
        /// Gets or sets a value indicating whether element is enabled or not.
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// Gets target state.
        /// </summary>
        TState TargetState { get; }
    }
}
