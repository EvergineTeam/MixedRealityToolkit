using WaveEngine.Framework;
using WaveEngine.MRTK.SDK.Features.UX.Components.Configurators;
using WaveEngine.MRTK.SDK.Features.UX.Components.States;

namespace WaveEngine_MRTK_Demo.Components.MultiStateButtons
{
    [AllowMultipleInstances]
    public class MultiStateButtonConfigurator : StandardButtonConfigurator, IStateAware<MultiStateButtonStates>
    {
        public MultiStateButtonStates TargetState { get; set; }
    }
}
