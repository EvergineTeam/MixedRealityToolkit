// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using Evergine.Common.Graphics;
using Evergine.Components.Graphics3D;
using Evergine.Framework.Graphics;
using Evergine.Framework.Managers;
using Evergine.MRTK.Effects;
using Evergine.MRTK.Extensions;

namespace Evergine.MRTK.SDK.Features.UX.Components.Configurators
{
    /// <summary>
    /// Configurator for materials.
    /// </summary>
    public class MaterialConfigurator
    {
        private Material sourceMaterial;
        private Material cachedMaterial;
        private HoloGraphic holoMaterial;
        private AssetSceneManager assetManager;
        private Color? tintColor;

        /// <summary>
        /// Gets or sets material.
        /// </summary>
        public Material Material
        {
            get => this.sourceMaterial;

            set
            {
                if (this.sourceMaterial != value)
                {
                    this.sourceMaterial = value;
                    this.InvalidateMaterial(ref this.cachedMaterial);
                    this.OnMaterialUpdate();
                }
            }
        }

        /// <summary>
        /// Gets or sets material tint color.
        /// text color.
        /// </summary>
        public Color? TintColor
        {
            get => this.tintColor;
            set
            {
                if (this.tintColor != value)
                {
                    this.tintColor = value;
                    this.OnTintColorUpdate();
                }
            }
        }

        /// <summary>
        /// Gets or sets target material component.
        /// </summary>
        public MaterialComponent TargetMaterialComponent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether material should be a new material instance.
        /// </summary>
        public bool CreatesNewMaterialInstance { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the null material will be allowed.
        /// </summary>
        public bool AllowNullMaterial { get; set; }

        /// <summary>
        /// Indicates manager instance to be used.
        /// </summary>
        /// <param name="assetManager">Asset manager.</param>
        public void UseAssetManager(AssetSceneManager assetManager) => this.assetManager = assetManager;

        /// <summary>
        /// Applies configurator.
        /// </summary>
        public void Apply()
        {
            this.OnMaterialUpdate();
            this.OnTintColorUpdate();
        }

        private void OnMaterialUpdate()
        {
            if ((this.AllowNullMaterial || this.sourceMaterial != null) && this.TargetMaterialComponent != null)
            {
                if (this.cachedMaterial == null)
                {
                    this.cachedMaterial = this.CreatesNewMaterialInstance
                        ? this.sourceMaterial?.LoadNewInstance(this.assetManager)
                        : this.sourceMaterial;
                }

                this.TargetMaterialComponent.Material = this.cachedMaterial;

                if (this.cachedMaterial != null)
                {
                    this.holoMaterial = new HoloGraphic(this.cachedMaterial);
                }
            }
        }

        private void InvalidateMaterial(ref Material material) => material = null;

        private void OnTintColorUpdate()
        {
            if (this.holoMaterial != null && this.tintColor.HasValue)
            {
                this.holoMaterial.Albedo = this.tintColor.Value;
            }
        }
    }
}
