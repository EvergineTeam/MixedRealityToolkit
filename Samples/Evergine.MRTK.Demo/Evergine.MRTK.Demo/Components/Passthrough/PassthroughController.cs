// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using Evergine.Common;
using Evergine.Components.XR;
using Evergine.Framework;
using Evergine.MRTK.SDK.Features;
using Evergine.MRTK.SDK.Features.UX.Components.ToggleButtons;
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

            if (!Application.Current.IsEditor)
            {
                this.Owner.IsEnabled = this.IsPassthroughSupported();
            }

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
            bool hasPasstroughSupport = (Application.Current as MyApplication).HasPassthroughSupport;
            return Tools.IsXRPlatformInputTrackingAvailable() && hasPasstroughSupport;
        }

        private void UpdatePassthroughState()
        {
            if (!Application.Current.IsEditor)
            {
                this.xrPassthroughLayerComponent.IsEnabled = this.buttonPassthroughToggleButton.IsOn;
            }
        }
    }
}
