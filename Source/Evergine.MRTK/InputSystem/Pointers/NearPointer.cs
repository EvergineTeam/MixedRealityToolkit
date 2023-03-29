// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using Evergine.Framework.Physics3D;
using Evergine.Framework;
using Evergine.Framework.Graphics;

namespace Evergine.MRTK.InputSystem.Pointers
{
    /// <summary>
    /// The <see cref="NearPointer"/> generates collision events with entities using near interaction such as touch.
    /// </summary>
    public class NearPointer : BasePointer
    {
        [BindComponent]
        private Transform3D transform3D = null;

        [BindComponent]
        private RigidBody3D rigidBody3D = null;

        /// <inheritdoc/>
        protected override void OnActivated()
        {
            base.OnActivated();

            //// TODO Move to OnAttached when the NRE bug is fixed
            this.rigidBody3D.IsSensor = true;
            this.rigidBody3D.CollisionCategories = this.CollisionCategory;
            this.rigidBody3D.MaskBits = this.CollisionCategoryMask & ~this.CollisionCategory;
        }

        /// <inheritdoc/>
        public override void UpdateInteractions()
        {
            // Update pointer position
            if (this.XRController.TryGetNearInteractionTransform(out var transform))
            {
                this.transform3D.Position = transform.Translation;
                this.transform3D.Orientation = transform.Orientation;
            }
        }
    }
}
