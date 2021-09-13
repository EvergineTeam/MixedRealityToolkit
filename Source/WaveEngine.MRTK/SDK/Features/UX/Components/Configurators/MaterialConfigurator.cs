// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using WaveEngine.Common.Graphics;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Managers;
using WaveEngine.MRTK.Effects;
using WaveEngine.MRTK.Extensions;

namespace WaveEngine.MRTK.SDK.Features.UX.Components.Configurators
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
            if (this.sourceMaterial != null && this.TargetMaterialComponent != null)
            {
                if (this.cachedMaterial == null)
                {
                    this.cachedMaterial = this.CreatesNewMaterialInstance
                        ? this.sourceMaterial.LoadNewInstance(this.assetManager)
                        : this.sourceMaterial;
                }

                this.TargetMaterialComponent.Material = this.cachedMaterial;
                this.holoMaterial = new HoloGraphic(this.cachedMaterial);
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
