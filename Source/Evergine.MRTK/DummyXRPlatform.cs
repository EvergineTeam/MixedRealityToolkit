// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using Evergine.Framework.Graphics;
using Evergine.Framework.Services;
using Evergine.Framework.XR.TrackedDevices;

namespace Evergine.MRTK
{
    /// <summary>
    /// Dummy implementation of <see cref="XRPlatform"/>. Both <see cref="Display"/> and <see cref="InputTracking"/>.
    /// </summary>
    public class DummyXRPlatform : XRPlatform
    {
        /// <inheritdoc/>
        public override Display Display => null;

        /// <inheritdoc/>
        public override XRInputTracking InputTracking => null;

        /// <inheritdoc/>
        protected override void InternalUpdate()
        {
        }
    }
}
