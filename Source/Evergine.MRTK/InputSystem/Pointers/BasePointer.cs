// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using Evergine.Framework;
using Evergine.Framework.Physics3D;
using Evergine.MRTK.InputSystem.Controllers;
using Evergine.MRTK.Services.InputSystem;

namespace Evergine.MRTK.InputSystem.Pointers
{
    /// <summary>
    /// Base class for every Pointer type.
    /// </summary>
    public abstract class BasePointer : Component
    {
        /// <summary>
        /// Gets or sets the <see cref="CollisionCategory3D"/> used by this <see cref="BasePointer"/>.
        /// Default: <see cref="CollisionCategory3D.Cat2"/>.
        /// </summary>
        public virtual CollisionCategory3D CollisionCategory { get; protected set; } = CollisionCategory3D.Cat2;

        /// <summary>
        /// Gets or sets the <see cref="CollisionCategory3D"/> mask used by this <see cref="BasePointer"/>.
        /// Default: <see cref="CollisionCategory3D.All"/>.
        /// <para>It indicates which <see cref="CollisionCategory3D"/> the pointer will collide with.</para>
        /// </summary>
        public virtual CollisionCategory3D CollisionCategoryMask { get; protected set; } = CollisionCategory3D.All;

        [BindSceneManager]
        private FocusProvider focusProvider = null;

        /// <summary>
        /// The <see cref="BaseXRController"/> component.
        /// </summary>
        [BindComponent(source: BindComponentSource.Parents, isExactType: false)]
        public BaseXRController XRController = null;

        /// <inheritdoc/>
        protected override bool OnAttached()
        {
            if (!base.OnAttached())
            {
                return false;
            }

            this.focusProvider.RegisterPointer(this);

            return true;
        }

        /// <inheritdoc/>
        protected override void OnDetach()
        {
            base.OnDetach();

            this.focusProvider.UnregisterPointer(this);
        }

        /// <summary>
        /// Update all interactions that this pointer has with the rest of objects on the scene.
        /// Updates pointer position and performs interaction checks to determine interacted objects.
        /// </summary>
        public abstract void UpdateInteractions();
    }
}
