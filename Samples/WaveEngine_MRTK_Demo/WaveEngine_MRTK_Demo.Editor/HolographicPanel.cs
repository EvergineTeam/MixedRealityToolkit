using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WaveEngine.Editor.Extension;
using WaveEngine.Editor.Extension.Attributes;
using WaveEngine_MRTK_Demo.Effects;

namespace WaveEngine_MRTK_Demo.Editor
{
    [CustomPanelEditor(typeof(HoloGraphic))]
    public class HolographicPanel : PanelEditor
    {
        private HoloGraphic instance;

        protected override void Loaded()
        {
            base.Loaded();

            this.instance = (HoloGraphic)this.Instance;

            this.instance.Material.MaterialStateChanged += Material_MaterialStateChanged;
        }

        private void Material_MaterialStateChanged(object sender, EventArgs e)
        {
            this.propertyPanelContainer.InvalidateLayout();
        }

        public override void GenerateUI()
        {
            base.GenerateUI();

            //Never show HoverLights or ProximityLights
            this.propertyPanelContainer.Remove(nameof(HoloGraphic.Parameters_HoverLightData));
            this.propertyPanelContainer.Remove(nameof(HoloGraphic.Parameters_ProximityLightData));

            this.propertyPanelContainer.Remove(nameof(HoloGraphic.PerCamera_EyeCount));

            //Always remove color and use Albedo instead
            this.propertyPanelContainer.Remove(nameof(HoloGraphic.Parameters_Color));
            this.propertyPanelContainer.Remove(nameof(HoloGraphic.Parameters_Alpha));
            //this.propertyPanelContainer.AddColor("Albedo", "Albedo", () => instance.Albedo, (albedo) => instance.Albedo = albedo) ;

            if (!this.instance.ActiveDirectivesNames.Contains("INNER_GLOW"))
            {
                this.propertyPanelContainer.Remove(nameof(HoloGraphic.Parameters_InnerGlowPower));
                this.propertyPanelContainer.Remove(nameof(HoloGraphic.Parameters_InnerGlowColor));
                this.propertyPanelContainer.Remove(nameof(HoloGraphic.Parameters_InnerGlowAlpha));
            }

            if (!this.instance.ActiveDirectivesNames.Contains("BORDER_LIGHT"))
            {
                this.propertyPanelContainer.Remove(nameof(HoloGraphic.Parameters_BorderWidth));
                this.propertyPanelContainer.Remove(nameof(HoloGraphic.Parameters_BorderMinValue));
                this.propertyPanelContainer.Remove(nameof(HoloGraphic.Parameters_FluentLightIntensity));
                this.propertyPanelContainer.Remove(nameof(HoloGraphic.Parameters_EdgeSmoothingValue));
            }

            if (!this.instance.ActiveDirectivesNames.Contains("ROUND_CORNERS"))
            {
                this.propertyPanelContainer.Remove(nameof(HoloGraphic.Parameters_RoundCornerMargin));
                this.propertyPanelContainer.Remove(nameof(HoloGraphic.Parameters_RoundCornerRadious));
                this.propertyPanelContainer.Remove(nameof(HoloGraphic.Parameters_Cutoff));
            }

            if (!this.instance.ActiveDirectivesNames.Contains("NEAR_LIGHT_FADE"))
            {
                this.propertyPanelContainer.Remove(nameof(HoloGraphic.Parameters_FadeBeginDistance));
                this.propertyPanelContainer.Remove(nameof(HoloGraphic.Parameters_FadeCompleteDistance));
                this.propertyPanelContainer.Remove(nameof(HoloGraphic.Parameters_FadeMinValue));
            }

            if (!this.instance.ActiveDirectivesNames.Contains("HOVER_LIGHT") || !this.instance.ActiveDirectivesNames.Contains("HOVER_COLOR_OVERRIDE"))
            {
                this.propertyPanelContainer.Remove(nameof(HoloGraphic.Parameters_HoverColorOverride));
            }

            if (!this.instance.ActiveDirectivesNames.Contains("PROXIMITY_LIGHT") || !this.instance.ActiveDirectivesNames.Contains("PROXIMITY_LIGHT_COLOR_OVERRIDE"))
            {
                this.propertyPanelContainer.Remove(nameof(HoloGraphic.Parameters_ProximityLightCenterColorOverride));
                this.propertyPanelContainer.Remove(nameof(HoloGraphic.Parameters_ProximityLightMiddleColorOverride));
                this.propertyPanelContainer.Remove(nameof(HoloGraphic.Parameters_ProximityLightOuterColorOverride));
            }

            this.propertyPanelContainer.Remove(nameof(HoloGraphic.Parameters_LightColor0));
            if (!this.instance.ActiveDirectivesNames.Contains("DIRECTIONAL_LIGHT"))
            {
                this.propertyPanelContainer.Remove(nameof(HoloGraphic.Parameters_Metallic));
                this.propertyPanelContainer.Remove(nameof(HoloGraphic.Parameters_Smoothness));
                this.propertyPanelContainer.Remove(nameof(HoloGraphic.LightColor));
            }

            if (!this.instance.ActiveDirectivesNames.Contains("ALBEDO_MAP"))
            {
                this.propertyPanelContainer.Remove(nameof(HoloGraphic.Texture));
                this.propertyPanelContainer.Remove(nameof(HoloGraphic.Sampler));
                this.propertyPanelContainer.Remove(nameof(HoloGraphic.Parameters_Tiling));
                this.propertyPanelContainer.Remove(nameof(HoloGraphic.Parameters_Offset));
            }
        }
    }
}
