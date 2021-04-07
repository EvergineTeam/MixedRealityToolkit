using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WaveEngine.Editor.Extension;
using WaveEngine.Editor.Extension.Attributes;
using WaveEngine.MRTK.Effects;

namespace WaveEngine_MRTK_Demo.Editor
{
    [CustomPanelEditor(typeof(HoloGraphic))]
    public class HolographicPanel : PanelEditor
    {
        private Dictionary<string, MemberInfo> members;

        public new HoloGraphic Instance => (HoloGraphic)base.Instance;

        protected override void Loaded()
        {
            base.Loaded();

            this.Instance.Material.MaterialStateChanged += this.Material_MaterialStateChanged;
            this.members = PanelEditor.GetMembersForType(this.Instance.GetType());
        }

        private void Material_MaterialStateChanged(object sender, EventArgs e)
        {
            this.propertyPanelContainer.InvalidateLayout();
        }

        public override void GenerateUI()
        {
            this.AddMember(nameof(HoloGraphic.Albedo));

            if (this.AddDirectiveCheckbox("Albedo Map", HoloGraphic.AlbedoMapDirective))
            {
                this.AddMember(nameof(HoloGraphic.Texture));
                this.AddMember(nameof(HoloGraphic.Sampler));
                this.AddMember(nameof(HoloGraphic.Parameters_Tiling));
                this.AddMember(nameof(HoloGraphic.Parameters_Offset));
            }

            // Rendering Options
            if (this.AddDirectiveCheckbox("Directional Light", HoloGraphic.DirectionalLightDirective))
            {
                this.AddRange(nameof(HoloGraphic.Metallic), 0, 1, 0);
                this.AddRange(nameof(HoloGraphic.Smoothness), 0, 1, 0.5f);
            }

            // Fluent Options
            var hoverLight = this.AddDirectiveCheckbox("Hover Light", HoloGraphic.HoverLightDirective);
            if (hoverLight &&
                this.AddDirectiveCheckbox("Override Color", HoloGraphic.HoverColorOverrideDirective))
            {
                this.AddMember(nameof(HoloGraphic.HoverColorOverride));
            }

            var proximityLight = this.AddDirectiveCheckbox("Proximity Light", HoloGraphic.ProximityLightDirective);
            if (proximityLight)
            {
                this.AddDirectiveCheckbox("Two Sided", HoloGraphic.ProximityLightTwoSidedDirective);
                this.AddDirectiveCheckbox("Substractive", HoloGraphic.ProximityLightSubtractiveDirective);

                if (this.AddDirectiveCheckbox("Override Color", HoloGraphic.ProximityLightColorOverrideDirective))
                {
                    this.AddMember(nameof(HoloGraphic.ProximityLightCenterColorOverride));
                    this.AddMember(nameof(HoloGraphic.ProximityLightMiddleColorOverride));
                    this.AddMember(nameof(HoloGraphic.ProximityLightOuterColorOverride));
                }
            }

            var borderLight = this.AddDirectiveCheckbox("Border Light", HoloGraphic.BorderLightDirective);
            if (borderLight)
            {
                this.AddDirectiveCheckbox("Border Light Uses Hover Color", HoloGraphic.BorderLightUsesHoverColorDirective);
                this.AddDirectiveCheckbox("Border Light Replaces Albedo", HoloGraphic.BorderLightReplacesAlbedoDirective);
                this.AddRange(nameof(HoloGraphic.BorderWidth), 0.0f, 1.0f, 0.1f);
                this.AddRange(nameof(HoloGraphic.BorderMinValue), 0.0f, 1.0f, 0.1f);
            }

            if (hoverLight || proximityLight || borderLight)
            {
                this.AddMember(nameof(HoloGraphic.FluentLightIntensity));
            }

            var roundCorners = this.AddDirectiveCheckbox("Round Corners", HoloGraphic.RoundCornersDirective);
            if (roundCorners)
            {
                if (this.AddDirectiveCheckbox("Independent Corners", HoloGraphic.IndependentCornersDirective))
                {
                    this.AddMember(nameof(HoloGraphic.RoundCornersRadius));
                }
                else
                {
                    this.AddRange(nameof(HoloGraphic.RoundCornerRadius), 0, 0.5f, 0.25f);
                }

                this.AddRange(nameof(HoloGraphic.RoundCornerMargin), 0, 0.5f, 0.01f);
            }

            if (roundCorners || borderLight)
            {
                this.AddRange(nameof(HoloGraphic.EdgeSmoothingValue), 0.0001f, 0.2f, 0.002f);
                this.AddDirectiveCheckbox("Ignore Z Scale", HoloGraphic.IgnoreZScaleDirective);
            }

            var alphaClip = this.AddDirectiveCheckbox("Alpha Clip", HoloGraphic.AlphaClipDirective);
            if (alphaClip || roundCorners)
            {
                this.AddRange(nameof(HoloGraphic.AlphaCutoff), 0, 1, 0.5f);
            }

            if (this.AddDirectiveCheckbox("Inner Glow", HoloGraphic.InnerGlowDirective))
            {
                this.AddMember(nameof(HoloGraphic.InnerGlowColor));
                this.AddRange(nameof(HoloGraphic.InnerGlowPower), 2, 32, 4);
            }

            if (this.AddDirectiveCheckbox("Iridescence", HoloGraphic.IridescenceDirective))
            {
                this.AddMember(nameof(HoloGraphic.IridescentSpectrumMap));
                this.AddMember(nameof(HoloGraphic.IridescentSpectrumMapSampler));
                this.AddRange(nameof(HoloGraphic.IridescenceIntensity), 0, 1, 0.5f);
                this.AddRange(nameof(HoloGraphic.IridescenceThreshold), 0, 1, 0.05f);
                this.AddRange(nameof(HoloGraphic.IridescenceAngle), -0.78f, 0.78f, -0.78f);
            }

            // TODO: In Unity this is related to nearPlaneFade.
            if (this.AddDirectiveCheckbox("Near Light Fade", HoloGraphic.NearLightFadeDirective))
            {
                this.AddRange(nameof(HoloGraphic.FadeBeginDistance), 0.01f, 10f, 0.85f);
                this.AddRange(nameof(HoloGraphic.FadeCompleteDistance), 0.01f, 10f, 0.5f);
                this.AddMember(nameof(HoloGraphic.FadeMinValue));
            }

            foreach (var item in this.propertyPanelContainer.Properties)
            {
                item.Name = item.Name.Replace("Parameters_", string.Empty);
            }

            this.AddMember(nameof(HoloGraphic.LayerDescription));
        }

        private void AddMember(string memberName)
        {
            if (this.members.TryGetValue(memberName, out var memberInfo))
            {
                this.propertyPanelContainer.Add(memberInfo);
            }
        }

        private void AddRange(string memberName, float min, float max, float defaultValue)
        {
            if (this.members.TryGetValue(memberName, out var memberInfo))
            {
                this.propertyPanelContainer.AddNumeric(memberName, memberName, min, max, max - min / 100, max - min / 10, defaultValue,
                                                getValue: () => (float)((PropertyInfo)memberInfo).GetValue(this.Instance),
                                                setValue: (x) => ((PropertyInfo)memberInfo).SetValue(this.Instance, x),
                                                true);
            }
        }

        private bool AddDirectiveCheckbox(string name, string directiveOn)
        {
            var directiveOff = directiveOn + "_OFF";
            this.propertyPanelContainer.AddBoolean(
                            directiveOn,
                            name,
                            false,
                            () => this.Instance.ActiveDirectivesNames.Contains(directiveOn),
                            (val) =>
                            {
                                var currentDirectives = this.Instance.ActiveDirectivesNames.ToList();
                                currentDirectives.Remove(directiveOn);
                                currentDirectives.Remove(directiveOff);
                                currentDirectives.Add(val ? directiveOn : directiveOff);
                                this.Instance.ActiveDirectivesNames = currentDirectives.ToArray();
                            });

            return this.Instance.ActiveDirectivesNames.Contains(directiveOn);
        }
    }
}
