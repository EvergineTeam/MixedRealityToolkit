// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using WaveEngine.Components.Graphics3D;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;

namespace WaveEngine.MRTK.SDK.Features.UX.Components.Configurators
{
    /// <summary>
    /// Configures some UI elements for a standard rounded UI button.
    /// </summary>
    public class RoundedButtonConfigurator : Component
    {
        private readonly MaterialConfigurator frontMaterialConfigurator;
        private readonly MaterialConfigurator backMaterialConfigurator;

        [BindComponent(source: BindComponentSource.ChildrenSkipOwner, tag: "PART_Front", isRequired: false)]
        private MaterialComponent frontMaterial = null;

        [BindComponent(source: BindComponentSource.ChildrenSkipOwner, tag: "PART_Back", isRequired: false)]
        private MaterialComponent backMaterial = null;

        private Material front;
        private Material back;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoundedButtonConfigurator"/> class.
        /// </summary>
        public RoundedButtonConfigurator()
        {
            this.frontMaterialConfigurator = new MaterialConfigurator();
            this.backMaterialConfigurator = new MaterialConfigurator();
        }

        /// <summary>
        /// Gets or sets front material.
        /// </summary>
        public Material Front
        {
            get => this.front;

            set
            {
                if (this.front != value)
                {
                    this.front = value;
                    this.frontMaterialConfigurator.Material = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets back material.
        /// </summary>
        public Material Back
        {
            get => this.back;

            set
            {
                if (this.back != value)
                {
                    this.back = value;
                    this.backMaterialConfigurator.Material = value;
                }
            }
        }

        /// <inheritdoc />
        protected override bool OnAttached()
        {
            bool attached = base.OnAttached();
            if (attached)
            {
                this.frontMaterialConfigurator.UseAssetManager(this.Managers.AssetSceneManager);
                this.backMaterialConfigurator.UseAssetManager(this.Managers.AssetSceneManager);
                this.frontMaterialConfigurator.TargetMaterialComponent = this.frontMaterial;
                this.backMaterialConfigurator.TargetMaterialComponent = this.backMaterial;
            }

            return attached;
        }

        /// <inheritdoc />
        protected override void OnActivated()
        {
            base.OnActivated();
            this.frontMaterialConfigurator.Apply();
            this.backMaterialConfigurator.Apply();
        }
    }
}
