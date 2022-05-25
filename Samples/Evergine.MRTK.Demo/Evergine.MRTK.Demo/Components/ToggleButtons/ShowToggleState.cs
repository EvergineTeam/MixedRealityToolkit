using System;
using Evergine.Components.Fonts;
using Evergine.Framework;
using Evergine.MRTK.SDK.Features.UX.Components.ToggleButtons;

namespace Evergine.MRTK.Demo.Components.ToggleButtons
{
    public class ShowToggleState : Component
    {
        [BindComponent(source: BindComponentSource.Owner, isRequired: true)]
        protected Text3DMesh textComponent;

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
