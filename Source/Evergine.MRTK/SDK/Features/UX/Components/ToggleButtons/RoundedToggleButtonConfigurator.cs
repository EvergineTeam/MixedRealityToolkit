// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using Evergine.Framework;
using Evergine.MRTK.SDK.Features.UX.Components.Configurators;
using Evergine.MRTK.SDK.Features.UX.Components.States;

namespace Evergine.MRTK.SDK.Features.UX.Components.ToggleButtons
{
    /// <summary>
    /// Rounded button configuration for toggle states.
    /// </summary>
    [AllowMultipleInstances]
    public class RoundedToggleButtonConfigurator : RoundedButtonConfigurator, IStateAware<ToggleState>
    {
        /// <inheritdoc />
        public ToggleState TargetState { get; set; }
    }
}
