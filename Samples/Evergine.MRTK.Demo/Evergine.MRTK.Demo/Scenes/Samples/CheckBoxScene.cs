using Evergine.Components.Fonts;
using Evergine.Mathematics;
using Evergine.MRTK.SDK.Features.UX.Components.Selection;
using Evergine.MRTK.SDK.Features.UX.Components.Sliders;
using System;
using System.Linq;

namespace Evergine.MRTK.Demo.Scenes.Samples
{
    internal class CheckBoxScene : DemoScene
    {
        private CheckBox configurationCheckBox;
        private Vector2 configurationDefaultSize;
        private PinchSlider customWidthSlider;
        private PinchSlider customHeightSlider;
        private Text3DMesh textEventChangedText;

        protected override void OnPostCreateXRScene()
        {
            base.OnPostCreateXRScene();

            this.configurationCheckBox = this.Managers.EntityManager.FindAllByTag("configuration_checkbox").First().FindComponent<CheckBox>();
            this.customWidthSlider = this.Managers.EntityManager.FindAllByTag("configuration_width").First().FindComponentInChildren<PinchSlider>();
            this.customHeightSlider = this.Managers.EntityManager.FindAllByTag("configuration_height").First().FindComponentInChildren<PinchSlider>();

            this.configurationDefaultSize = this.configurationCheckBox.Size;
            this.customWidthSlider.ValueUpdated += this.CustomSlider_ValueUpdated;
            this.customHeightSlider.ValueUpdated += this.CustomSlider_ValueUpdated;

            this.textEventChangedText = this.Managers.EntityManager.FindAllByTag("textEventChanged").First().FindComponent<Text3DMesh>();
            this.configurationCheckBox.IsCheckedChanged += this.ConfigurationCheckBox_CheckedChanged;
        }

        private void CustomSlider_ValueUpdated(object sender, SliderEventData e)
        {
            this.configurationCheckBox.Size = new Vector2(
                this.configurationDefaultSize.X + this.configurationDefaultSize.X * this.customWidthSlider.SliderValue,
                this.configurationDefaultSize.Y + this.configurationDefaultSize.Y * this.customHeightSlider.SliderValue);
        }

        private void ConfigurationCheckBox_CheckedChanged(object sender, EventArgs e) =>
            this.textEventChangedText.Text = $"Checked changed to {this.configurationCheckBox.IsChecked}";
    }
}
