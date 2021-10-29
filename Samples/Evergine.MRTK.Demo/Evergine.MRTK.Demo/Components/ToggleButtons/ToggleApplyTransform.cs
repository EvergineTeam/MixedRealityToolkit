using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.Mathematics;
using Evergine.MRTK.SDK.Features.UX.Components.States;
using Evergine.MRTK.SDK.Features.UX.Components.ToggleButtons;

namespace Evergine.MRTK.Demo.Components.ToggleButtons
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
