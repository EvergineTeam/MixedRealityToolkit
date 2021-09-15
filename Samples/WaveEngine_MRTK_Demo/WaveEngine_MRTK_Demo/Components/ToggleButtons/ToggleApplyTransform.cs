using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Mathematics;
using WaveEngine.MRTK.SDK.Features.UX.Components.States;
using WaveEngine.MRTK.SDK.Features.UX.Components.ToggleButtons;

namespace WaveEngine_MRTK_Demo.Components.ToggleButtons
{
    [AllowMultipleInstances]
    public class ToggleApplyTransform : Component, IStateAware<ToggleState>
    {
        [BindComponent(source: BindComponentSource.Owner, isRequired: true)]
        private Transform3D targetTransform = null;

        public ToggleState TargetState { get; set; }

        public Vector3 Scale { get; set; } = new Vector3(1, 1, 1);

        protected override void OnActivated()
        {
            base.OnActivated();
            this.targetTransform.LocalScale = this.Scale;
        }
    }
}
