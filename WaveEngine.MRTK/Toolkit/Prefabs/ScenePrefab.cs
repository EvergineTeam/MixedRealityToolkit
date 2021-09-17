// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WaveEngine.Common.Attributes;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Framework;
using WaveEngine.Framework.Assets;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Managers;
using WaveEngine.Framework.Services;

namespace WaveEngine.MRTK.Toolkit.Prefabs
{
    /// <summary>
    /// A component that can be used to instantiate prefabs using scenes.
    /// </summary>
    public class ScenePrefab : Component
    {
        /// <summary>
        /// The asset service.
        /// </summary>
        [BindService]
        protected AssetsService assetService;

        /// <summary>
        /// The asset scene manager.
        /// </summary>
        [BindSceneManager]
        protected AssetSceneManager assetSceneManager;

        private bool duplicateMaterials;

        private IEnumerable<Entity> prefabEntities;

        /// <summary>
        /// Gets the scene prefab ID that will be used.
        /// </summary>
        [RenderProperty(CustomPropertyName = "Prefab")]
        public ScenePrefabProperty ScenePrefabProperty = new ScenePrefabProperty();

        /// <summary>
        /// Gets or sets a value indicating whether the materials that this prefab uses will be duplicated.
        /// </summary>
        [RenderProperty(Tooltip = "Set whether the materials that this prefab uses will be duplicated.")]
        public bool DuplicateMaterials
        {
            get => this.duplicateMaterials;
            set
            {
                if (this.duplicateMaterials != value)
                {
                    this.duplicateMaterials = value;
                    this.AttachPrefabEntities();
                }
            }
        }

        /// <summary>
        /// Occurs before a prefab entity will be added to the <see cref="ScenePrefab"/> owner.
        /// </summary>
        public event EventHandler<Entity> AddingPrefabEntity;

        /// <summary>
        /// Occurs when the prefab entities are instanced.
        /// </summary>
        public event EventHandler PrefabEntityRefreshed;

        /// <inheritdoc/>
        protected override bool OnAttached()
        {
            if (!base.OnAttached())
            {
                return false;
            }

            this.ScenePrefabProperty.OnScenePrefabChanged += this.ScenePrefab_OnScenePrefabChanged;

            this.AttachPrefabEntities();

            return true;
        }

        /// <inheritdoc/>
        protected override void OnDetach()
        {
            this.DetachPrefabEntities();

            this.ScenePrefabProperty.OnScenePrefabChanged -= this.ScenePrefab_OnScenePrefabChanged;

            base.OnDetach();
        }

        /// <inheritdoc/>
        protected override void OnDestroy()
        {
            base.OnDestroy();
            this.DestroyPrefabEntities();
        }

        private void ScenePrefab_OnScenePrefabChanged(object sender, EventArgs e)
        {
            this.RefreshEntity();
        }

        private void AttachPrefabEntities()
        {
            this.DetachPrefabEntities();

            var alreadyInstantiated = this.prefabEntities != null;
            this.prefabEntities = this.prefabEntities ?? this.CreatePrefabEntities();

            if (this.prefabEntities == null)
            {
                return;
            }

            foreach (var entity in this.prefabEntities)
            {
                this.AddingPrefabEntity?.Invoke(this, entity);
                this.Owner.AddChild(entity);
            }

            if (!alreadyInstantiated)
            {
                this.PrefabEntityRefreshed?.Invoke(this, EventArgs.Empty);
            }
        }

        private void DetachPrefabEntities()
        {
            if (this.prefabEntities == null)
            {
                return;
            }

            foreach (var entity in this.prefabEntities)
            {
                this.Owner.DetachChild(entity);
            }
        }

        private IEnumerable<Entity> CreatePrefabEntities()
        {
            if (!this.ScenePrefabProperty.IsPrefabIdValid)
            {
                return null;
            }

            var st = Stopwatch.StartNew();
            var source = this.assetService.GetAssetSource<SceneSource>(this.ScenePrefabProperty.PrefabId);
            var sceneItems = source.SceneData.Items.ToArray();
            foreach (var item in sceneItems)
            {
                this.PrepareEntity(item.Entity);
            }

            Trace.WriteLine($"Prefab with id '{this.ScenePrefabProperty.PrefabId}' created in {st.ElapsedMilliseconds}ms");

            return sceneItems.Select(item => item.Entity);
        }

        private void PrepareEntity(Entity entity)
        {
            entity.Id = Guid.NewGuid();
            entity.Flags = HideFlags.DontSave | HideFlags.DontShow;
            foreach (var component in entity.Components)
            {
                component.Id = Guid.NewGuid();

                if (this.DuplicateMaterials &&
                    component is MaterialComponent materialComponent &&
                    materialComponent.Material != null)
                {
                    var clonedMaterial = this.assetSceneManager.Load<Material>(materialComponent.Material.Id, forceNewInstance: true);
                    clonedMaterial.Id = Guid.NewGuid();
                    materialComponent.Material = clonedMaterial;
                }
            }

            foreach (var child in entity.ChildEntities.ToArray())
            {
                entity.DetachChild(child);
                this.PrepareEntity(child);
                entity.AddChild(child);
            }
        }

        private void DestroyPrefabEntities()
        {
            if (this.prefabEntities == null)
            {
                return;
            }

            foreach (var entity in this.prefabEntities)
            {
                entity.Destroy();
            }

            this.prefabEntities = null;
        }

        /// <summary>
        /// Refresh the entity, re-instancing the prefab.
        /// </summary>
        /// <param name="checkIsAttached">Check if it's attached.</param>
        public void RefreshEntity(bool checkIsAttached = true)
        {
            if (checkIsAttached &&
                !this.IsAttached)
            {
                return;
            }

            this.DetachPrefabEntities();
            this.DestroyPrefabEntities();
            this.AttachPrefabEntities();
        }
    }
}
