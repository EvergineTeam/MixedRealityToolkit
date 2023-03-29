// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.Mathematics;

namespace Evergine.MRTK.InputSystem.Controllers
{
    /// <summary>
    /// MRTK controller for a physical controller.
    /// </summary>
    public class XRPhysicalController : BaseXRController
    {
        private const string NearPointerAnchorTag = "NearPointerAnchor";

        [BindComponent(source: BindComponentSource.Children, isRequired: false, tag: NearPointerAnchorTag)]
        private Transform3D nearPointerAnchorTransform = null;

        /// <inheritdoc/>
        public override ControllerType ControllerType => ControllerType.XRPhysicalController;

        /// <inheritdoc/>
        public override bool TryGetNearInteractionTransform(out Matrix4x4 transform)
        {
            if (this.nearPointerAnchorTransform != null)
            {
                transform = this.nearPointerAnchorTransform.WorldTransform;
                return true;
            }

            transform = Matrix4x4.Identity;
            return false;
        }
    }
}
