// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using System;
using Evergine.Common.Graphics;
using Evergine.Components.Graphics3D;
using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.MRTK.Toolkit.GUI;

namespace Evergine.MRTK.SDK.Features.UX.Components.Configurators
{
    /// <summary>
    /// Configures some UI elements for a slider.
    /// </summary>
    public class SliderConfigurator : Component
    {
        private readonly MaterialConfigurator trackConfigurator;
        private readonly MaterialConfigurator thumbConfigurator;

        [BindComponent(source: BindComponentSource.ChildrenSkipOwner, tag: "PART_Track", isRequired: false)]
        private MaterialComponent trackMaterial = null;

        [BindComponent(source: BindComponentSource.ChildrenSkipOwner, tag: "PART_Thumb", isRequired: false)]
        private MaterialComponent thumbMaterial = null;

        [BindComponent(source: BindComponentSource.ChildrenSkipOwner, tag: "PART_Title", isRequired: false)]
        private Text3D titleText = null;

        [BindComponent(source: BindComponentSource.ChildrenSkipOwner, tag: "PART_Value", isRequired: false)]
        private Text3D valueText = null;

        private Material track;
        private Material thumb;

        private Color primaryColor = Color.White;
        private Color secondaryColor = Color.White;
        private string title = "Title";

        /// <summary>
        /// Initializes a new instance of the <see cref="SliderConfigurator"/> class.
        /// </summary>
        public SliderConfigurator()
        {
            this.trackConfigurator = new MaterialConfigurator();
            this.thumbConfigurator = new MaterialConfigurator();
        }

        /// <summary>
        /// Gets or sets track material.
        /// </summary>
        public Material Track
        {
            get => this.track;

            set
            {
                if (this.track != value)
                {
                    this.track = value;
                    this.trackConfigurator.Material = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets thumb material.
        /// </summary>
        public Material Thumb
        {
            get => this.thumb;

            set
            {
                if (this.thumb != value)
                {
                    this.thumb = value;
                    this.thumbConfigurator.Material = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether track material should be a new material instance.
        /// </summary>
        public bool CreatesNewTrackMaterialInstance { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether thumb material should be a new material instance.
        /// </summary>
        public bool CreatesNewThumbMaterialInstance { get; set; }

        /// <summary>
        /// Gets or sets slider primary color. It affects to title color.
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
        /// Gets or sets slider secondary color. It affects to title color.
        /// </summary>
        public Color SecondaryColor
        {
            get => this.secondaryColor;
            set
            {
                if (this.secondaryColor != value)
                {
                    this.secondaryColor = value;
                    this.OnSecondaryColorUpdate();
                }
            }
        }

        /// <summary>
        /// Gets or sets slider title.
        /// </summary>
        public string Title
        {
            get => this.title;

            set
            {
                if (this.title != value)
                {
                    this.title = value;
                    this.OnUpdateTitle();
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

            this.trackConfigurator.UseAssetManager(this.Managers.AssetSceneManager);
            this.thumbConfigurator.UseAssetManager(this.Managers.AssetSceneManager);

            this.trackConfigurator.CreatesNewMaterialInstance = this.CreatesNewTrackMaterialInstance;
            this.thumbConfigurator.CreatesNewMaterialInstance = this.CreatesNewThumbMaterialInstance;

            this.trackConfigurator.TargetMaterialComponent = this.trackMaterial;
            this.thumbConfigurator.TargetMaterialComponent = this.thumbMaterial;

            return true;
        }

        /// <inheritdoc />
        protected override void OnActivated()
        {
            base.OnActivated();
            this.trackConfigurator.Apply();
            this.thumbConfigurator.Apply();
            this.OnUpdateTitle();
            this.OnPrimaryColorUpdate();
            this.OnSecondaryColorUpdate();
        }

        private void OnPrimaryColorUpdate()
        {
            if (this.titleText != null)
            {
                this.titleText.Foreground = this.primaryColor;
            }
        }

        private void OnSecondaryColorUpdate()
        {
            if (this.valueText != null)
            {
                this.valueText.Foreground = this.secondaryColor;
            }
        }

        private void OnUpdateTitle()
        {
            if (this.titleText != null)
            {
                this.titleText.Text = this.title;
            }
        }
    }
}
