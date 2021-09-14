// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.
using WaveEngine.Common.Graphics;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.MRTK.Toolkit.GUI;

namespace WaveEngine.MRTK.SDK.Features.UX.Components.Configurators
{
    /// <summary>
    /// Configures some UI elements for a standard UI button.
    /// </summary>
    public class StandardButtonConfigurator : Component
    {
        private readonly MaterialConfigurator plateConfigurator;
        private readonly MaterialConfigurator iconConfigurator;

        [BindComponent(source: BindComponentSource.ChildrenSkipOwner, tag: "PART_Plate", isRequired: false)]
        private MaterialComponent plateMaterial = null;

        [BindComponent(source: BindComponentSource.ChildrenSkipOwner, tag: "PART_Icon", isRequired: false)]
        private MaterialComponent iconMaterial = null;

        [BindComponent(source: BindComponentSource.ChildrenSkipOwner, tag: "PART_Text", isRequired: false)]
        private Text3D buttonText = null;

        private Material plate;
        private Material icon;

        private Color primaryColor = Color.White;
        private string text;

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardButtonConfigurator"/> class.
        /// </summary>
        public StandardButtonConfigurator()
        {
            this.plateConfigurator = new MaterialConfigurator();
            this.iconConfigurator = new MaterialConfigurator();
        }

        /// <summary>
        /// Gets or sets plate material.
        /// </summary>
        public Material Plate
        {
            get => this.plate;

            set
            {
                if (this.plate != value)
                {
                    this.plate = value;
                    this.plateConfigurator.Material = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether back plate material should be a new material instance.
        /// </summary>
        public bool CreatesNewBackPlateMaterialInstance { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether icon material should be a new material instance.
        /// </summary>
        public bool CreatesNewIconMaterialInstance { get; set; }

        /// <summary>
        /// Gets or sets button icon.
        /// </summary>
        public Material Icon
        {
            get => this.icon;

            set
            {
                if (this.icon != value)
                {
                    this.icon = value;
                    this.iconConfigurator.Material = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets button primary color. This color is used to tint icon and set
        /// text color.
        /// </summary>
        public Color PrimaryColor
        {
            get => this.primaryColor;
            set
            {
                if (this.primaryColor != value)
                {
                    this.primaryColor = value;
                    this.OnPrimaryColorUpdate();
                }
            }
        }

        /// <summary>
        /// Gets or sets button text.
        /// </summary>
        public string Text
        {
            get => this.text;

            set
            {
                if (this.text != value)
                {
                    this.text = value;
                    this.OnUpdateText();
                }
            }
        }

        /// <inheritdoc />
        protected override bool OnAttached()
        {
            if (!base.OnAttached())
            {
                return false;
            }

            this.plateConfigurator.UseAssetManager(this.Managers.AssetSceneManager);
            this.iconConfigurator.UseAssetManager(this.Managers.AssetSceneManager);

            this.plateConfigurator.CreatesNewMaterialInstance = this.CreatesNewBackPlateMaterialInstance;
            this.iconConfigurator.CreatesNewMaterialInstance = this.CreatesNewIconMaterialInstance;

            this.plateConfigurator.TargetMaterialComponent = this.plateMaterial;
            this.iconConfigurator.TargetMaterialComponent = this.iconMaterial;

            return true;
        }

        /// <inheritdoc />
        protected override void OnActivated()
        {
            base.OnActivated();
            this.plateConfigurator.Apply();
            this.iconConfigurator.Apply();
            this.OnUpdateText();
            this.UpdateTextColor();
        }

        private void OnPrimaryColorUpdate()
        {
            this.iconConfigurator.TintColor = this.primaryColor;
            this.UpdateTextColor();
        }

        private void OnUpdateText()
        {
            if (this.buttonText != null)
            {
                this.buttonText.Text = this.text.AsSafeStringForText3D();
            }
        }

        private void UpdateTextColor()
        {
            if (this.buttonText != null)
            {
                this.buttonText.Foreground = this.primaryColor;
            }
        }
    }
}
