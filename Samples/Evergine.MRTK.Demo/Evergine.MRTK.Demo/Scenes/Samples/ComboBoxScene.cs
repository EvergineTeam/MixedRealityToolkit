using Evergine.Mathematics;
using Evergine.MRTK.SDK.Features.UX.Components.Lists;
using Evergine.MRTK.SDK.Features.UX.Components.PressableButtons;
using Evergine.MRTK.SDK.Features.UX.Components.Selection;
using Evergine.MRTK.SDK.Features.UX.Components.Sliders;
using System;
using System.Linq;

namespace Evergine.MRTK.Demo.Scenes.Samples
{
    internal class ComboBoxScene : DemoScene
    {
        private ComboBox configurationComboBox;
        private Vector2 maxComboBoxSize;
        private PressableButton configurationSelectedItemChangeButton;
        private PressableButton configurationSelectedItemClearButton;
        private ComboBox configurationCustomArrowComboBox;
        private PinchSlider customWidthSlider;
        private PinchSlider customHeightSlider;

        protected override void OnPostCreateXRScene()
        {
            base.OnPostCreateXRScene();

            this.configurationComboBox = this.Managers.EntityManager.FindAllByTag("Configuration_ComboBox").First().FindComponentInChildren<ComboBox>();
            this.configurationComboBox.DataSource = new ArrayAdapter<object>(GetSampleItems());
            this.configurationComboBox.PlaceholderText = "Select an item...";
            this.maxComboBoxSize = this.configurationComboBox.Size * 2;

            this.configurationSelectedItemChangeButton = this.Managers.EntityManager.FindAllByTag("controls_selectedItem_button").First().FindComponentInChildren<PressableButton>();
            this.configurationSelectedItemChangeButton.ButtonReleased += this.ConfigurationSelectedItemChangeButton_ButtonReleased;

            this.configurationSelectedItemClearButton = this.Managers.EntityManager.FindAllByTag("controls_selectedItem_clear").First().FindComponentInChildren<PressableButton>();
            this.configurationSelectedItemClearButton.ButtonReleased += this.ConfigurationSelectedItemClearButton_ButtonReleased;

            this.configurationCustomArrowComboBox = this.Managers.EntityManager.FindAllByTag("Configuration_ComboBox_CustomArrow").First().FindComponentInChildren<ComboBox>();
            this.configurationCustomArrowComboBox.DataSource = new ArrayAdapter<object>(GetSampleItems());

            this.customWidthSlider = this.Managers.EntityManager.FindAllByTag("configuration_width").First().FindComponentInChildren<PinchSlider>();
            this.customHeightSlider = this.Managers.EntityManager.FindAllByTag("configuration_height").First().FindComponentInChildren<PinchSlider>();
            this.customWidthSlider.InitialValue = this.customHeightSlider.InitialValue = 0.5f;

            var applyButton = this.Managers.EntityManager.FindAllByTag("controls_apply_button").First().FindComponentInChildren<PressableButton>();
            applyButton.ButtonPressed += this.ApplySizeButton_ButtonPressed;
        }

        private void ConfigurationSelectedItemChangeButton_ButtonReleased(object sender, EventArgs args)
        {
            var index = new Random().Next(0, this.configurationComboBox.DataSource.Count - 1);
            this.configurationComboBox.SelectedItem = this.configurationComboBox.DataSource.GetRowValue(index);
        }

        private void ConfigurationSelectedItemClearButton_ButtonReleased(object sender, EventArgs e)
        {
            this.configurationComboBox.SelectedItem = null;
        }

        private void ApplySizeButton_ButtonPressed(object sender, EventArgs e)
        {
            var listViewSize = this.configurationComboBox.Size;
            listViewSize.X = this.maxComboBoxSize.X * this.customWidthSlider.SliderValue;
            listViewSize.Y = this.maxComboBoxSize.Y * this.customHeightSlider.SliderValue;

            this.configurationComboBox.Size = listViewSize;
        }

        private class SampleItem
        {
            public string Name { get; set; }

            public override string ToString() => this.Name;
        }

        private static SampleItem[] GetSampleItems() => 
            Enumerable.Range(1, 20)
                      .Select(n => new SampleItem
                      {
                        Name = $"Item {n}",
                      }).ToArray();
    }
}
