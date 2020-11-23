// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using WaveEngine.Common.Attributes;
using Color = WaveEngine.Common.Graphics.Color;
using LinearColor = WaveEngine.Common.Graphics.LinearColor;

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
                LinearColor linearColor = default;
                linearColor.A = this.Parameters_Alpha;
                linearColor.AsVector3 = this.Parameters_Color;
                return linearColor.ToColor();
            }

            set
            {
                var linearColor = new LinearColor(ref value);
                this.Parameters_Color = linearColor.AsVector3;
                this.Parameters_Alpha = linearColor.A / 255.0f;
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
                LinearColor linearColor = default;
                linearColor.AsVector4 = this.Parameters_LightColor0;
                return linearColor.ToColor();
            }

            set
            {
                var linearColor = new LinearColor(ref value);
                this.Parameters_LightColor0 = linearColor.AsVector4;
            }
        }

        /// <summary>
        /// Gets or sets the HoverColorOverride.
        /// </summary>
        public Color HoverColorOverride
        {
            get
            {
                LinearColor linearColor = default;
                linearColor.A = 1;
                linearColor.AsVector3 = this.Parameters_HoverColorOverride;
                return linearColor.ToColor();
            }

            set
            {
                var linearColor = new LinearColor(ref value);
                this.Parameters_HoverColorOverride = linearColor.AsVector3;
            }
        }

        /// <summary>
        /// Gets or sets the ProximityLightCenterColorOverride.
        /// </summary>
        public Color ProximityLightCenterColorOverride
        {
            get
            {
                LinearColor linearColor = default;
                linearColor.AsVector4 = this.Parameters_ProximityLightCenterColorOverride;
                return linearColor.ToColor();
            }

            set
            {
                var linearColor = new LinearColor(ref value);
                this.Parameters_ProximityLightCenterColorOverride = linearColor.AsVector4;
            }
        }

        /// <summary>
        /// Gets or sets the ProximityLightMiddleColorOverride.
        /// </summary>
        public Color ProximityLightMiddleColorOverride
        {
            get
            {
                LinearColor linearColor = default;
                linearColor.AsVector4 = this.Parameters_ProximityLightMiddleColorOverride;
                return linearColor.ToColor();
            }

            set
            {
                var linearColor = new LinearColor(ref value);
                this.Parameters_ProximityLightMiddleColorOverride = linearColor.AsVector4;
            }
        }

        /// <summary>
        /// Gets or sets the ProximityLightOuterColorOverride.
        /// </summary>
        public Color ProximityLightOuterColorOverride
        {
            get
            {
                LinearColor linearColor = default;
                linearColor.AsVector4 = this.Parameters_ProximityLightOuterColorOverride;
                return linearColor.ToColor();
            }

            set
            {
                var linearColor = new LinearColor(ref value);
                this.Parameters_ProximityLightOuterColorOverride = linearColor.AsVector4;
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
                LinearColor linearColor = default;
                linearColor.A = this.Parameters_InnerGlowAlpha;
                linearColor.AsVector3 = this.Parameters_InnerGlowColor;
                return linearColor.ToColor();
            }

            set
            {
                var linearColor = new LinearColor(ref value);
                this.Parameters_InnerGlowColor = linearColor.AsVector3;
                this.Parameters_InnerGlowAlpha = linearColor.A / 255.0f;
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
