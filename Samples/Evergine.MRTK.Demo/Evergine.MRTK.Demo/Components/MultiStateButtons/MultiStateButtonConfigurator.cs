using Evergine.Framework;
using Evergine.MRTK.SDK.Features.UX.Components.Configurators;
using Evergine.MRTK.SDK.Features.UX.Components.States;

namespace Evergine.MRTK.Demo.Components.MultiStateButtons
{
    [AllowMultipleInstances]
    public class MultiStateButtonConfigurator : StandardButtonConfigurator, IStateAware<MultiStateButtonStates>
    {
        public MultiStateButtonStates TargetState { get; set; }
    }
}
