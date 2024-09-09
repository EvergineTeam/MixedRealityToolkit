using System.Linq;
using Evergine.Mathematics;
using Evergine.MRTK.SDK.Features.UX.Components.Scrolling;
using Evergine.MRTK.SDK.Features.UX.Components.Sliders;

namespace Evergine.MRTK.Demo.Scenes.Samples
{
    public class ScrollViewScene : DemoScene
    {
        private Vector2 originalDimensions;
        private ScrollView customScrollView;
        private PinchSlider customWidthSlider;
        private PinchSlider customHeightSlider;
        private PinchSlider barSizeSlider;
        private float originalBarSize;

        protected override void OnPostCreateXRScene()
        {
            base.OnPostCreateXRScene();

            var assetsManager = this.Managers.AssetSceneManager;

            this.customScrollView = this.Managers.EntityManager.FindAllByTag("configurationScrollView").First().FindComponentInChildren<ScrollView>();
            this.originalDimensions = this.customScrollView.Size;

            this.customWidthSlider = this.Managers.EntityManager.FindAllByTag("configuration_width").First().FindComponentInChildren<PinchSlider>();
            this.customHeightSlider = this.Managers.EntityManager.FindAllByTag("configuration_height").First().FindComponentInChildren<PinchSlider>();

            this.customWidthSlider.ValueUpdated += this.CustomSlider_ValueUpdated;
            this.customHeightSlider.ValueUpdated += this.CustomSlider_ValueUpdated;

            this.barSizeSlider = this.Managers.EntityManager.FindAllByTag("configuration_barSize").First().FindComponentInChildren<PinchSlider>();
            this.barSizeSlider.ValueUpdated += this.BarSizeSlider_ValueUpdated;
            this.originalBarSize = this.customScrollView.BarWidth;
        }

        private void CustomSlider_ValueUpdated(object sender, SliderEventData data)
        {
            this.customScrollView.Size = new Vector2(
                this.originalDimensions.X * this.customWidthSlider.SliderValue, 
                this.originalDimensions.Y * this.customHeightSlider.SliderValue);
        }

        private void BarSizeSlider_ValueUpdated(object sender, SliderEventData data)
        {
            this.customScrollView.BarWidth = 2 * data.NewValue * this.originalBarSize;
        }
    }
}
