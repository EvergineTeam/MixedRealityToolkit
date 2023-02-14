// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using Evergine.Common;
using Evergine.Components.XR;
using Evergine.Framework;
using Evergine.MRTK.SDK.Features;
using Evergine.MRTK.SDK.Features.UX.Components.ToggleButtons;
using Evergine.Platform;
using System;

namespace Evergine.MRTK.Demo.Components.Passthrough
{
    public class PassthroughController : Component
    {
        [BindComponent]
        private XRPassthroughLayerComponent xrPassthroughLayerComponent = null;

        [BindComponent(source: BindComponentSource.Children, tag: "Button_Passthrough")]
        private ToggleButton buttonPassthroughToggleButton = null;

        protected override bool OnAttached()
        {
            if (!base.OnAttached())
            {
                return false;
            }

            this.Owner.IsEnabled = this.IsPassthroughSupported();

            this.UpdatePassthroughState();

            return true;
        }

        protected override void OnActivated()
        {
            base.OnActivated();

            this.buttonPassthroughToggleButton.Toggled += this.ButtonPassthroughToggleButton_Toggled;
        }

        protected override void OnDetach()
        {
            base.OnDetach();

            this.buttonPassthroughToggleButton.Toggled -= this.ButtonPassthroughToggleButton_Toggled;
        }

        private void ButtonPassthroughToggleButton_Toggled(object sender, EventArgs e)
        {
            this.UpdatePassthroughState();
        }

        private bool IsPassthroughSupported()
        {
            return Tools.IsXRPlatformInputTrackingAvailable() && DeviceInfo.PlatformType == PlatformType.Android;
        }

        private void UpdatePassthroughState()
        {
            this.xrPassthroughLayerComponent.IsEnabled = this.buttonPassthroughToggleButton.IsOn;
        }
    }
}
