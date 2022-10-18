// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using System;
using System.Collections.Generic;
using System.Linq;
using Evergine.Components.Graphics3D;
using Evergine.Framework;
using Evergine.MRTK.Effects;

namespace Evergine.MRTK.Emulation
{
    /// <summary>
    /// Disables batching feature on meshes that uses <see cref="HoloGraphic"/> materials that does not allows batching.
    /// </summary>
    public class HolographicBatching : Component
    {
        private bool alreadyStarted;

        /// <inheritdoc/>
        protected override void OnActivated()
        {
            base.OnActivated();

            if (this.alreadyStarted)
            {
                this.Initialize();
            }
        }

        /// <inheritdoc/>
        protected override void Start()
        {
            base.Start();

            this.alreadyStarted = true;
            this.Initialize();
        }

        /// <inheritdoc/>
        protected override void OnDeactivated()
        {
            base.OnDeactivated();
            this.Managers.EntityManager.EntityAdded -= this.EntityManager_EntityAdded;
        }

        private void Initialize()
        {
            var materialComponents = this.Managers.EntityManager.FindComponentsOfType<MaterialComponent>();
            this.DisableBatchingOnRequiredHolographicMaterials(materialComponents);

            this.Managers.EntityManager.EntityAdded += this.EntityManager_EntityAdded;
        }

        private void EntityManager_EntityAdded(object sender, Entity entity)
        {
            this.DisableBatchingOnRequiredHolographicMaterials(entity);
        }

        /// <summary>
        /// Disables batching feature on meshes that uses <see cref="HoloGraphic"/> materials that does not allows batching.
        /// </summary>
        /// <param name="entity">The entity to process.</param>
        public void DisableBatchingOnRequiredHolographicMaterials(Entity entity)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var materialComponents = entity.FindComponentsInChildren<MaterialComponent>();
            this.DisableBatchingOnRequiredHolographicMaterials(materialComponents);
        }

        /// <summary>
        /// Disables batching feature on meshes that use <see cref="HoloGraphic"/> materials that do not allow batching.
        /// </summary>
        /// <param name="materialComponents">The collection of <see cref="MaterialComponent"/> to process.</param>
        public void DisableBatchingOnRequiredHolographicMaterials(IEnumerable<MaterialComponent> materialComponents)
        {
            var holographicEffectsByOwner = materialComponents.Where(m => m.Material?.Effect?.Id == HoloGraphic.EffectId)
                                                              .Where(m => m.IsAttached) // Exclude unattached components
                                                              .ToDictionary(m => m.Owner, m => new HoloGraphic(m.Material));

            foreach (var pair in holographicEffectsByOwner)
            {
                if (pair.Value.AllowBatching)
                {
                    continue;
                }

                // Border Light and inner glow don't work if batching is enabled
                var meshComponent = pair.Key.FindComponent<MeshComponent>(isExactType: false);
                if (meshComponent == null || meshComponent.Meshes == null)
                {
                    continue;
                }

                foreach (var mesh in meshComponent.Meshes)
                {
                    mesh.AllowBatching = false;
                }
            }
        }
    }
}
