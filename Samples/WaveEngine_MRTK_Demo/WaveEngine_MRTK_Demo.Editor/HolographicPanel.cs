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
                this.AddMember(nameof(HoloGraphic.Texture), indent: 1);
                this.AddMember(nameof(HoloGraphic.Sampler), indent: 1);
            }

            this.AddMember(nameof(HoloGraphic.Parameters_Tiling));
            this.AddMember(nameof(HoloGraphic.Parameters_Offset));

            // Rendering Options
            if (this.AddDirectiveCheckbox("Directional Light", HoloGraphic.DirectionalLightDirective))
            {
                this.AddRange(nameof(HoloGraphic.Metallic), 0, 1, 0, indent: 1);
                this.AddRange(nameof(HoloGraphic.Smoothness), 0, 1, 0.5f, indent: 1);
            }

            // TODO: In Unity this is related to nearPlaneFade.
            if (this.AddDirectiveCheckbox("Near Light Fade", HoloGraphic.NearLightFadeDirective))
            {
                this.AddRange(nameof(HoloGraphic.FadeBeginDistance), 0.0f, 10f, 0.01f, name: "Fade Begin", indent: 1);
                this.AddRange(nameof(HoloGraphic.FadeCompleteDistance), 0.0f, 10f, 0.1f, name: "Fade Complete", indent: 1);
                this.AddRange(nameof(HoloGraphic.FadeMinValue), 0.0f, 1f, 0.0f, name: "Fade Min Value", indent: 1);
            }

            // Fluent Options
            var hoverLight = this.AddDirectiveCheckbox("Hover Light", HoloGraphic.HoverLightDirective);
            var hoverLightOverrideColor = false;
            if (hoverLight)
            {
                hoverLightOverrideColor = this.AddDirectiveCheckbox("Override Color", HoloGraphic.HoverColorOverrideDirective, indent: 1);
                if (hoverLightOverrideColor)
                {
                    this.AddMember(nameof(HoloGraphic.HoverColorOverride), name: "Color", indent: 2);
                }
            }

            var proximityLight = this.AddDirectiveCheckbox("Proximity Light", HoloGraphic.ProximityLightDirective);
            if (proximityLight)
            {
                if (this.AddDirectiveCheckbox("Override Color", HoloGraphic.ProximityLightColorOverrideDirective, indent: 1))
                {
                    this.AddMember(nameof(HoloGraphic.ProximityLightCenterColorOverride), name: "Center Color", indent: 2);
                    this.AddMember(nameof(HoloGraphic.ProximityLightMiddleColorOverride), name: "Middle Color", indent: 2);
                    this.AddMember(nameof(HoloGraphic.ProximityLightOuterColorOverride), name: "Outer Color", indent: 2);
                }

                this.AddDirectiveCheckbox("Subtractive", HoloGraphic.ProximityLightSubtractiveDirective, indent: 1);
                this.AddDirectiveCheckbox("Two Sided", HoloGraphic.ProximityLightTwoSidedDirective, indent: 1);
            }

            var borderLight = this.AddDirectiveCheckbox("Border Light", HoloGraphic.BorderLightDirective);
            if (borderLight)
            {
                this.AddRange(nameof(HoloGraphic.BorderWidth), 0.0f, 1.0f, 0.1f, name: "Width %", indent: 1);
                this.AddRange(nameof(HoloGraphic.BorderMinValue), 0.0f, 1.0f, 0.1f, name: "Brightness", indent: 1);
                this.AddDirectiveCheckbox("Replace Albedo", HoloGraphic.BorderLightReplacesAlbedoDirective, indent: 1);

                if (hoverLightOverrideColor)
                {
                    this.AddDirectiveCheckbox("Use Hover Color", HoloGraphic.BorderLightUsesHoverColorDirective, indent: 1);
                }
            }

            if (hoverLight || proximityLight || borderLight)
            {
                this.AddRange(nameof(HoloGraphic.FluentLightIntensity), 0.0f, 1.0f, 0.1f, name: "Light intensity");
            }

            var roundCorners = this.AddDirectiveCheckbox("Round Corners", HoloGraphic.RoundCornersDirective);
            if (roundCorners)
            {
                if (this.AddDirectiveCheckbox("Independent Corners", HoloGraphic.IndependentCornersDirective, indent: 1))
                {
                    this.AddMember(nameof(HoloGraphic.RoundCornersRadius), name: "Corner Radius", indent: 1);
                }
                else
                {
                    this.AddRange(nameof(HoloGraphic.RoundCornerRadius), 0.0f, 0.5f, 0.25f, name: "Unit Radius", indent: 1);
                }

                this.AddRange(nameof(HoloGraphic.RoundCornerMargin), 0.0f, 0.5f, 0.01f, name: "Margin %", indent: 1);
            }

            if (roundCorners || borderLight)
            {
                this.AddRange(nameof(HoloGraphic.EdgeSmoothingValue), 0.0001f, 0.2f, 0.002f, name: "Edge Smoothing Value");
            }

            if (this.AddDirectiveCheckbox("Inner Glow", HoloGraphic.InnerGlowDirective))
            {
                this.AddMember(nameof(HoloGraphic.InnerGlowColor), name: "Color", indent: 1);
                this.AddRange(nameof(HoloGraphic.InnerGlowPower), 2.0f, 32.0f, 4.0f, name: "Power", indent: 1);
            }

            if (this.AddDirectiveCheckbox("Iridescence", HoloGraphic.IridescenceDirective))
            {
                this.AddMember(nameof(HoloGraphic.IridescentSpectrumMap), name: "Spectrum Map", indent: 1);
                this.AddMember(nameof(HoloGraphic.IridescentSpectrumMapSampler), name: "Spectrum Map Sampler", indent: 1);
                this.AddRange(nameof(HoloGraphic.IridescenceIntensity), 0.0f, 1.0f, 0.5f, name: "Intensity", indent: 1);
                this.AddRange(nameof(HoloGraphic.IridescenceThreshold), 0.0f, 1.0f, 0.05f, name: "Threshold", indent: 1);
                this.AddRange(nameof(HoloGraphic.IridescenceAngle), -0.78f, 0.78f, -0.78f, name: "Angle", indent: 1);
            }

            // Advanced Options
            if (roundCorners || borderLight)
            {
                this.AddDirectiveCheckbox("Ignore Z Scale", HoloGraphic.IgnoreZScaleDirective);
            }

            this.AddMember(nameof(HoloGraphic.LayerDescription));

            var blendEnable = this.Instance.Material.LayerDescription.RenderState.BlendState.RenderTarget0.BlendEnable;
            this.Instance.EnsureDirectiveIsActive(HoloGraphic.AlphaBlendDirective, blendEnable);
            if (!blendEnable)
            {
                var alphaClip = this.AddDirectiveCheckbox("Alpha Test", HoloGraphic.AlphaTestDirective);
                if (alphaClip || roundCorners)
                {
                    this.AddRange(nameof(HoloGraphic.AlphaCutoff), 0.0f, 1.0f, 0.5f, name: "Alpha Cutoff");
                }
            }
            else
            {
                this.Instance.EnsureDirectiveIsActive(HoloGraphic.AlphaTestDirective, false);
            }

            foreach (var item in this.propertyPanelContainer.Properties)
            {
                item.Name = item.Name.Replace("Parameters_", string.Empty);
            }
        }

        private void AddIndent(IPropertyInfo propertyInfo, int indent)
        {
            if (indent > 0)
            {
                propertyInfo.Name = string.Join(string.Empty, Enumerable.Repeat(' ', 8 * indent).Concat(propertyInfo.Name));
            }
        }

        private void AddMember(string memberName, string name = null, int indent = 0)
        {
            if (this.members.TryGetValue(memberName, out var memberInfo))
            {
                this.propertyPanelContainer.Add(memberInfo);

                if (name != null || indent > 0)
                {
                    var propertyInfo = this.propertyPanelContainer.Find(memberName);
                    propertyInfo.Name = name ?? propertyInfo.Name;

                    this.AddIndent(propertyInfo, indent);
                }
            }
        }

        private void AddRange(string memberName, float min, float max, float defaultValue, string name = null, int indent = 0)
        {
            if (this.members.TryGetValue(memberName, out var memberInfo))
            {
                var propertyInfo = this.propertyPanelContainer.AddNumeric(memberName, name ?? memberName, min, max, max - min / 100, max - min / 10, defaultValue,
                                                getValue: () => (float)((PropertyInfo)memberInfo).GetValue(this.Instance),
                                                setValue: (x) => ((PropertyInfo)memberInfo).SetValue(this.Instance, x),
                                                true);
                this.AddIndent(propertyInfo, indent);
            }
        }

        private bool AddDirectiveCheckbox(string name, string directiveOn, int indent = 0)
        {
            var directiveOff = directiveOn + "_OFF";
            var propertyInfo = this.propertyPanelContainer.AddBoolean(
                            directiveOn,
                            name,
                            false,
                            () => this.IsDirectiveActive(directiveOn),
                            (val) =>
                            {
                                this.Instance.EnsureDirectiveIsActive(directiveOn, val);
                            });

            this.AddIndent(propertyInfo, indent);

            return this.IsDirectiveActive(directiveOn);
        }

        private bool IsDirectiveActive(string directiveOn)
        {
            return this.Instance.ActiveDirectivesNames.Contains(directiveOn);
        }
    }
}
