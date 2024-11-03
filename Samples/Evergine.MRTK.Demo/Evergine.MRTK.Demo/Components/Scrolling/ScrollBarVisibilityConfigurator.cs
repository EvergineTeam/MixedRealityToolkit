using Evergine.Framework;
using Evergine.MRTK.SDK.Features.UX.Components.Configurators;
using Evergine.MRTK.SDK.Features.UX.Components.Scrolling;
using Evergine.MRTK.SDK.Features.UX.Components.States;

namespace Evergine.MRTK.Demo.Components.Scrolling
{
    [AllowMultipleInstances]
    internal class ScrollBarVisibilityConfiguratorr : StandardButtonConfigurator, IStateAware<ScrollBarVisibility>
    {
        public ScrollBarVisibility TargetState { get; set; }
    }
}
