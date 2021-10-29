using System;
using Evergine.Framework;
using Evergine.MRTK.SDK.Features.UX.Components.ToggleButtons;
using Evergine.MRTK.Toolkit.GUI;

namespace Evergine.MRTK.Demo.Components.ToggleButtons
{
    public class ShowToggleState : Component
    {
        [BindComponent(source: BindComponentSource.Owner, isRequired: true)]
        protected Text3D textComponent;

        [BindComponent(source: BindComponentSource.ChildrenSkipOwner, isRequired: true)]
        protected ToggleButton button;

        protected override void OnActivated()
        {
            base.OnActivated();

            this.button.Toggled += this.Button_Toggled;
        }

        private void Button_Toggled(object sender, EventArgs e)
        {
            this.textComponent.Text = this.button.IsOn ? "On" : "Off";
        }
    }
}
