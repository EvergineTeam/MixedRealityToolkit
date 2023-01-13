// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using System;
using Evergine.Components.XR;
using Evergine.Framework;
using Evergine.Framework.XR;
using Evergine.MRTK.Emulation;

namespace Evergine.MRTK.Behaviors
{
    /// <summary>
    /// XR controller cursor pinch.
    /// </summary>
    public class PhysicalControllerControlBehavior : Behavior
    {
        /// <summary>
        /// The controller.
        /// </summary>
        [BindComponent]
        protected TrackXRController trackXRController;

        /// <summary>
        /// The cursor.
        /// </summary>
        [BindComponent(isExactType: false, source: BindComponentSource.Children)]
        protected Cursor cursor;

        /// <inheritdoc/>
        protected override void Update(TimeSpan gameTime)
        {
            if (this.trackXRController.PoseIsValid)
            {
                var controllerState = this.trackXRController.ControllerState;

                var isPinching = this.cursor.Pinch;
                if (isPinching && !controllerState.IsButtonPressed(XRButtons.Trigger))
                {
                    this.cursor.Pinch = false;
                }
                else if (!isPinching && controllerState.IsButtonPressed(XRButtons.Trigger))
                {
                    this.cursor.Pinch = true;
                }
            }
            else
            {
                this.cursor.Pinch = false;
            }
        }
    }
}
