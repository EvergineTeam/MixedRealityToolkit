// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using Evergine.Framework.Managers;
using Evergine.MRTK.InputSystem.Pointers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Evergine.MRTK.InputSystem
{
    /// <summary>
    /// The <see cref="FocusProvider"/> is responsible for managing focus on entities and coordinating all pointer updates.
    /// </summary>
    public class FocusProvider : UpdatableSceneManager
    {
        private List<PointerData> pointers = new List<PointerData>();

        /// <inheritdoc/>
        public override void Update(TimeSpan gameTime)
        {
            foreach (var pointer in this.pointers)
            {
                pointer.Pointer.UpdateInteractions();
            }
        }

        /// <summary>
        /// Register a pointer into the <see cref="FocusProvider"/>.
        /// </summary>
        /// <param name="pointer">The pointer to be registered.</param>
        public void RegisterPointer(BasePointer pointer)
        {
            if (!this.pointers.Any(p => p.Pointer == pointer))
            {
                this.pointers.Add(new PointerData()
                {
                    Pointer = pointer,
                });
            }
        }

        /// <summary>
        /// Unregister a pointer from the <see cref="FocusProvider"/>.
        /// </summary>
        /// <param name="pointer">The pointer to be unregistered.</param>
        public void UnregisterPointer(BasePointer pointer)
        {
            var pointerData = this.pointers.FirstOrDefault(p => p.Pointer == pointer);
            if (pointerData != null)
            {
                this.pointers.Remove(pointerData);
            }
        }

        private class PointerData
        {
            public BasePointer Pointer { get; set; }
        }
    }
}
