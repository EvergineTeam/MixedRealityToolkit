// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using WaveEngine.Common.Attributes;
using Color = WaveEngine.Common.Graphics.Color;

namespace WaveEngine.MRTK.Effects
{
    /// <summary>
    /// Partial class for Holographic, needed for Custom Editor.
    /// </summary>
    [WaveEngine.Framework.Graphics.MaterialDecoratorAttribute("4f7e4c24-e83c-4350-9cd4-511fb2199cf4")]
    public partial class HoloGraphic : WaveEngine.Framework.Graphics.MaterialDecorator
    {
        /// <summary>
        /// Gets or sets the Albedo.
        /// </summary>
        public Color Albedo
        {
            get
            {
                return new Color(this.Parameters_Color.X, this.Parameters_Color.Y, this.Parameters_Color.Z, this.Parameters_Alpha);
            }

            set
            {
                this.Parameters_Color = value.ToVector3();
                this.Parameters_Alpha = value.A / 255.0f;
            }
        }

        /// <summary>
        /// Gets or sets the Metallic attribute.
        /// </summary>
        [RenderPropertyAsFInput(minLimit: 0.0f, maxLimit: 1.0f)]
        public float Metallic
        {
            get => this.Parameters_Metallic;
            set => this.Parameters_Metallic = value;
        }

        /// <summary>
        /// Gets or sets the smoothness attribute.
        /// </summary>
        [RenderPropertyAsFInput(minLimit: 0.0f, maxLimit: 1.0f)]
        public float Smoothness
        {
            get => this.Parameters_Smoothness;
            set => this.Parameters_Smoothness = value;
        }

        /// <summary>
        /// Gets or sets the light color.
        /// </summary>
        public Color LightColor
        {
            get
            {
                var colorVector = this.Parameters_LightColor0;
                return Color.FromVector4(ref colorVector);
            }

            set
            {
                this.Parameters_LightColor0 = value.ToVector4();
            }
        }

        /// <summary>
        /// Gets or sets the HoverColorOverride.
        /// </summary>
        public Color HoverColorOverride
        {
            get
            {
                var colorVector = this.Parameters_HoverColorOverride;
                return Color.FromVector3(ref colorVector);
            }

            set
            {
                this.Parameters_HoverColorOverride = value.ToVector3();
            }
        }

        /// <summary>
        /// Gets or sets the ProximityLightCenterColorOverride.
        /// </summary>
        public Color ProximityLightCenterColorOverride
        {
            get
            {
                var colorVector = this.Parameters_ProximityLightCenterColorOverride;
                return Color.FromVector4(ref colorVector);
            }

            set
            {
                this.Parameters_ProximityLightCenterColorOverride = value.ToVector4();
            }
        }

        /// <summary>
        /// Gets or sets the ProximityLightMiddleColorOverride.
        /// </summary>
        public Color ProximityLightMiddleColorOverride
        {
            get
            {
                var colorVector = this.Parameters_ProximityLightMiddleColorOverride;
                return Color.FromVector4(ref colorVector);
            }

            set
            {
                this.Parameters_ProximityLightMiddleColorOverride = value.ToVector4();
            }
        }

        /// <summary>
        /// Gets or sets the ProximityLightOuterColorOverride.
        /// </summary>
        public Color ProximityLightOuterColorOverride
        {
            get
            {
                var colorVector = this.Parameters_ProximityLightOuterColorOverride;
                return Color.FromVector4(ref colorVector);
            }

            set
            {
                this.Parameters_ProximityLightOuterColorOverride = value.ToVector4();
            }
        }

        /// <summary>
        /// Gets or sets the BorderWidth.
        /// </summary>
        [RenderPropertyAsFInput(minLimit: 0.0f, maxLimit: 1.0f)]
        public float BorderWidth
        {
            get => this.Parameters_BorderWidth;
            set => this.Parameters_BorderWidth = value;
        }

        /// <summary>
        /// Gets or sets the BorderMinValue.
        /// </summary>
        [RenderPropertyAsFInput(minLimit: 0.0f, maxLimit: 1.0f)]
        public float BorderMinValue
        {
            get => this.Parameters_BorderMinValue;
            set => this.Parameters_BorderMinValue = value;
        }

        /// <summary>
        /// Gets or sets the FluentLightIntensity.
        /// </summary>
        [RenderPropertyAsFInput(minLimit: 0.0f, maxLimit: 1.0f)]
        public float FluentLightIntensity
        {
            get => this.Parameters_FluentLightIntensity;
            set => this.Parameters_FluentLightIntensity = value;
        }

        /// <summary>
        /// Gets or sets the RoundCornerRadius.
        /// </summary>
        [RenderPropertyAsFInput(minLimit: 0.0f, maxLimit: 0.5f)]
        public float RoundCornerRadius
        {
            get => this.Parameters_RoundCornerRadious;
            set => this.Parameters_RoundCornerRadious = value;
        }

        /// <summary>
        /// Gets or sets the RoundCornerMargin.
        /// </summary>
        [RenderPropertyAsFInput(minLimit: 0.0f, maxLimit: 0.5f)]
        public float RoundCornerMargin
        {
            get => this.Parameters_RoundCornerMargin;
            set => this.Parameters_RoundCornerMargin = value;
        }

        /// <summary>
        /// Gets or sets the Cutoff.
        /// </summary>
        [RenderPropertyAsFInput(minLimit: 0.0f, maxLimit: 0.5f)]
        public float Cutoff
        {
            get => this.Parameters_Cutoff;
            set => this.Parameters_Cutoff = value;
        }

        /// <summary>
        /// Gets or sets the EdgeSmoothingValue.
        /// </summary>
        [RenderPropertyAsFInput(minLimit: 0.0f, maxLimit: 0.2f)]
        public float EdgeSmoothingValue
        {
            get => this.Parameters_EdgeSmoothingValue;
            set => this.Parameters_EdgeSmoothingValue = value;
        }

        /// <summary>
        /// Gets or sets the InnerGlowColor.
        /// </summary>
        public Color InnerGlowColor
        {
            get
            {
                return new Color(this.Parameters_InnerGlowColor.X, this.Parameters_InnerGlowColor.Y, this.Parameters_InnerGlowColor.Z, this.Parameters_InnerGlowAlpha);
            }

            set
            {
                this.Parameters_InnerGlowColor = value.ToVector3();
                this.Parameters_InnerGlowAlpha = value.A / 255.0f;
            }
        }

        /// <summary>
        /// Gets or sets the InnerGlowPower.
        /// </summary>
        [RenderPropertyAsFInput(minLimit: 2.0f, maxLimit: 32.0f)]
        public float InnerGlowPower
        {
            get => this.Parameters_InnerGlowPower;
            set => this.Parameters_InnerGlowPower = value;
        }

        /// <summary>
        /// Gets or sets the FadeBeginDistance.
        /// </summary>
        [RenderPropertyAsFInput(minLimit: 0.0f, maxLimit: 10.0f)]
        public float FadeBeginDistance
        {
            get => this.Parameters_FadeBeginDistance;
            set => this.Parameters_FadeBeginDistance = value;
        }

        /// <summary>
        /// Gets or sets the FadeCompleteDistance.
        /// </summary>
        [RenderPropertyAsFInput(minLimit: 0.0f, maxLimit: 10.0f)]
        public float FadeCompleteDistance
        {
            get => this.Parameters_FadeCompleteDistance;
            set => this.Parameters_FadeCompleteDistance = value;
        }

        /// <summary>
        /// Gets or sets the FadeMinValue.
        /// </summary>
        [RenderPropertyAsFInput(minLimit: 0.0f, maxLimit: 1.0f)]
        public float FadeMinValue
        {
            get => this.Parameters_FadeMinValue;
            set => this.Parameters_FadeMinValue = value;
        }
    }
}
