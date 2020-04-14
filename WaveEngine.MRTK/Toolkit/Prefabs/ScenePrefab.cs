// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using System.IO;
using System.Linq;
using WaveEngine.Common.Attributes;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Framework;
using WaveEngine.Framework.Assets;
using WaveEngine.Framework.Assets.Importers;
using WaveEngine.Framework.Services;
using WaveEngine.Platform;
using WaveEngine.MRTK.Toolkit.Extensions;

namespace WaveEngine.MRTK.Toolkit.Prefabs
{
    /// <summary>
    /// A component that can be used to instantiate prefabs using scenes.
    /// </summary>
    public class ScenePrefab : Component
    {
        [BindService]
        private AssetsDirectory assetsDirectory = null;

        [BindService]
        internal AssetsService AssetsService;

        private Guid prefabId;

        private bool duplicateMaterials;

        /// <summary>
        /// Gets or sets a value indicating whether the materials that this prefab uses will be duplicated.
        /// </summary>
        [RenderProperty(Tooltip= "Set whether the materials that this prefab uses will be duplicated.")]
        public bool DuplicateMaterials
        {
            get => this.duplicateMaterials;
            set
            {
                if (this.duplicateMaterials != value)
                {
                    this.duplicateMaterials = value;
                    this.RefreshEntity();
                }
            }
        }

        /// <summary>
        /// Gets or sets the prefab to use.
        /// </summary>
        [RenderProperty(Tooltip = "The prefab to use.")]
        public Guid PrefabId
        {
            get => this.prefabId;
            set
            {
                if (this.prefabId != value)
                {
                    this.prefabId = value;
                    this.RefreshEntity();
                }
            }
        }

        /// <inheritdoc/>
        protected override bool OnAttached()
        {
            if (!base.OnAttached())
            {
                return false;
            }

            this.RefreshEntity(false);

            return true;
        }

        /// <inheritdoc/>
        protected override void OnDetach()
        {
            this.ClearEntity();
            base.OnDetach();
        }

        private void ClearEntity()
        {
            while (this.Owner.NumChildren > 0)
            {
                this.Owner.RemoveChild(this.Owner.ChildEntities.First());
            }
        }

        internal void RefreshEntity(bool checkIsAttached = true)
        {
            if (checkIsAttached &&
                !this.IsAttached)
            {
                return;
            }

            this.ClearEntity();

            if (this.prefabId == Guid.Empty)
            {
                return;
            }

            var importer = new WaveSceneImporter();
            var source = new SceneSource();

            string path = this.GetAssetPath(this.prefabId);
            using (var stream = this.assetsDirectory.Open(path))
            {
                importer.ImportHeader(stream, out source);
                importer.ImportData(stream, source, false);
            }

            foreach (var item in source.SceneData.Items)
            {
                var child = item.Entity;
                this.PrepareEntity(child);
                this.Owner.AddChild(child);
            }
        }

        private void PrepareEntity(Entity entity)
        {
            entity.Id = Guid.NewGuid();
            entity.Flags = HideFlags.DontSave;
            foreach (var component in entity.Components)
            {
                component.Id = Guid.NewGuid();

                if (this.DuplicateMaterials &&
                    component is MaterialComponent materialComponent)
                {
                    materialComponent.Material = materialComponent.Material.Clone();
                }
            }

            foreach (var child in entity.ChildEntities)
            {
                this.PrepareEntity(child);
            }
        }

        private string GetAssetPath(Guid id)
        {
            return this.assetsDirectory.EnumerateFiles(string.Empty, $"{id}.*", SearchOption.AllDirectories).FirstOrDefault();
        }
    }
}
