using Noesis;
using System;
using System.Collections.Generic;
using System.Text;
using WaveEngine.Common.Attributes;
using WaveEngine.Common.Graphics;
using Color = WaveEngine.Common.Graphics.Color;

namespace WaveEngine_MRTK_Demo.Effects
{
    [WaveEngine.Framework.Graphics.MaterialDecoratorAttribute("4f7e4c24-e83c-4350-9cd4-511fb2199cf4")]
    public partial class HoloGraphic : WaveEngine.Framework.Graphics.MaterialDecorator
    {
        public Color Albedo
        {
            get
            {
                return new Color(Parameters_Color.X, Parameters_Color.Y, Parameters_Color.Z, Parameters_Alpha);
            }
            set
            {
                Parameters_Color = value.ToVector3();
                Parameters_Alpha = value.A / 255.0f;
            }
        }

        [RenderPropertyAsFInput(minLimit: 0.0f, maxLimit: 1.0f)]
        public float Metallic
        {
            get => this.Parameters_Metallic;
            set => this.Parameters_Metallic = value;
        }

        [RenderPropertyAsFInput(minLimit: 0.0f, maxLimit: 1.0f)]
        public float Smoothness
        {
            get => this.Parameters_Smoothness;
            set => this.Parameters_Smoothness = value;
        }

        public Color LightColor
        {
            get
            {
                var colorVector = Parameters_LightColor0;
                return Color.FromVector4(ref colorVector);
            }
            set
            {
                Parameters_LightColor0 = value.ToVector4();
            }
        }

        public Color HoverColorOverride
        {
            get
            {
                var colorVector = Parameters_HoverColorOverride;
                return Color.FromVector3(ref colorVector);
            }
            set
            {
                Parameters_HoverColorOverride = value.ToVector3();
            }
        }

        public Color ProximityLightCenterColorOverride
        {
            get
            {
                var colorVector = Parameters_ProximityLightCenterColorOverride;
                return Color.FromVector4(ref colorVector);
            }
            set
            {
                Parameters_ProximityLightCenterColorOverride = value.ToVector4();
            }
        }

        public Color ProximityLightMiddleColorOverride
        {
            get
            {
                var colorVector = Parameters_ProximityLightMiddleColorOverride;
                return Color.FromVector4(ref colorVector);
            }
            set
            {
                Parameters_ProximityLightMiddleColorOverride = value.ToVector4();
            }
        }

        public Color ProximityLightOuterColorOverride
        {
            get
            {
                var colorVector = Parameters_ProximityLightOuterColorOverride;
                return Color.FromVector4(ref colorVector);
            }
            set
            {
                Parameters_ProximityLightOuterColorOverride = value.ToVector4();
            }
        }

        [RenderPropertyAsFInput(minLimit: 0.0f, maxLimit: 1.0f)]
        public float BorderWidth
        {
            get => this.Parameters_BorderWidth;
            set => this.Parameters_BorderWidth = value;
        }

        [RenderPropertyAsFInput(minLimit: 0.0f, maxLimit: 1.0f)]
        public float BorderMinValue
        {
            get => this.Parameters_BorderMinValue;
            set => this.Parameters_BorderMinValue = value;
        }

        [RenderPropertyAsFInput(minLimit: 0.0f, maxLimit: 1.0f)]
        public float FluentLightIntensity
        {
            get => this.Parameters_FluentLightIntensity;
            set => this.Parameters_FluentLightIntensity = value;
        }

        [RenderPropertyAsFInput(minLimit: 0.0f, maxLimit: 0.5f)]
        public float RoundCornerRadius
        {
            get => this.Parameters_RoundCornerRadius;
            set => this.Parameters_RoundCornerRadius = value;
        }

        [RenderPropertyAsFInput(minLimit: 0.0f, maxLimit: 0.5f)]
        public float RoundCornerMargin
        {
            get => this.Parameters_RoundCornerMargin;
            set => this.Parameters_RoundCornerMargin = value;
        }

        [RenderPropertyAsFInput(minLimit: 0.0f, maxLimit: 0.5f)]
        public float Cutoff
        {
            get => this.Parameters_Cutoff;
            set => this.Parameters_Cutoff = value;
        }

        [RenderPropertyAsFInput(minLimit: 0.0f, maxLimit: 0.2f)]
        public float EdgeSmoothingValue
        {
            get => this.Parameters_EdgeSmoothingValue;
            set => this.Parameters_EdgeSmoothingValue = value;
        }

        public Color InnerGlowColor
        {
            get
            {
                return new Color(Parameters_InnerGlowColor.X, Parameters_InnerGlowColor.Y, Parameters_InnerGlowColor.Z, Parameters_InnerGlowAlpha);
            }
            set
            {
                Parameters_InnerGlowColor = value.ToVector3();
                Parameters_InnerGlowAlpha = value.A / 255.0f;
            }
        }

        [RenderPropertyAsFInput(minLimit: 2.0f, maxLimit: 32.0f)]
        public float InnerGlowPower
        {
            get => this.Parameters_InnerGlowPower;
            set => this.Parameters_InnerGlowPower = value;
        }

        [RenderPropertyAsFInput(minLimit: 0.0f, maxLimit: 10.0f)]
        public float FadeBeginDistance
        {
            get => this.Parameters_FadeBeginDistance;
            set => this.Parameters_FadeBeginDistance = value;
        }

        [RenderPropertyAsFInput(minLimit: 0.0f, maxLimit: 10.0f)]
        public float FadeCompleteDistance
        {
            get => this.Parameters_FadeCompleteDistance;
            set => this.Parameters_FadeCompleteDistance = value;
        }

        [RenderPropertyAsFInput(minLimit: 0.0f, maxLimit: 1.0f)]
        public float FadeMinValue
        {
            get => this.Parameters_FadeMinValue;
            set => this.Parameters_FadeMinValue = value;
        }
    }
}
