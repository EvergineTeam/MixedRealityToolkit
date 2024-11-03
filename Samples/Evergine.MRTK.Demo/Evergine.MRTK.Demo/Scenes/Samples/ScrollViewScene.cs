using System.Linq;
using Evergine.Components.Fonts;
using Evergine.Mathematics;
using Evergine.MRTK.Demo.Components.Scrolling;
using Evergine.MRTK.SDK.Features.UX.Components.Scrolling;
using Evergine.MRTK.SDK.Features.UX.Components.Sliders;
using Evergine.MRTK.SDK.Features.UX.Components.States;
using Evergine.MRTK.SDK.Features.UX.Components.ToggleButtons;

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

        private ScrollView directionScrollView;
        private ScrollBarVisibilityStateManager directionsHorizontalBarManager;
        private ToggleButton directionsScrollHorizontal;
        private ToggleButton directionsScrollVertical;
        private ScrollBarVisibilityStateManager directionsVerticalBarManager;
        private Text3DMesh directionsScrolledToText;

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

            this.directionScrollView = this.Managers.EntityManager.FindAllByTag("directions_ScrollView").First().FindComponentInChildren<ScrollView>();
            this.directionsHorizontalBarManager = this.Managers.EntityManager.FindAllByTag("directions_bar_horizontal").First().FindComponentInChildren<ScrollBarVisibilityStateManager>();
            this.directionsVerticalBarManager = this.Managers.EntityManager.FindAllByTag("directions_bar_vertical").First().FindComponentInChildren<ScrollBarVisibilityStateManager>();
            this.directionsHorizontalBarManager.StateChanged += this.DirectionsBar_StateChanged;
            this.directionsVerticalBarManager.StateChanged += this.DirectionsBar_StateChanged;

            this.directionsScrollHorizontal = this.Managers.EntityManager.FindAllByTag("directions_scroll_horizontal").First().FindComponentInChildren<ToggleButton>();
            this.directionsScrollVertical = this.Managers.EntityManager.FindAllByTag("directions_scroll_vertical").First().FindComponentInChildren<ToggleButton>();
            this.directionsScrollHorizontal.Toggled += this.DirectionsScroll_Toggled;
            this.directionsScrollVertical.Toggled += this.DirectionsScroll_Toggled;

            this.directionsScrolledToText = this.Managers.EntityManager.FindAllByTag("directions_ScrolledTo").First().FindComponent<Text3DMesh>();
            this.directionScrollView.Scrolled += DirectionScrollView_Scrolled;
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

        private void DirectionsBar_StateChanged(object sender, StateChangedEventArgs<ScrollBarVisibility> args)
        {
            if (this.directionsHorizontalBarManager.CurrentState != null)
            {
                this.directionScrollView.HorizontalScrollBarVisibility = this.directionsHorizontalBarManager.CurrentState.Value;
            }

            if (this.directionsVerticalBarManager.CurrentState != null)
            {
                this.directionScrollView.VerticalScrollBarVisibility = this.directionsVerticalBarManager.CurrentState.Value;
            }
        }

        private void DirectionsScroll_Toggled(object sender, System.EventArgs e)
        {
            this.directionScrollView.HorizontalScrollEnabled = this.directionsScrollHorizontal.IsOn;
            this.directionScrollView.VerticalScrollEnabled = this.directionsScrollVertical.IsOn;
        }

        private void DirectionScrollView_Scrolled(object sender, System.EventArgs e) =>
            this.directionsScrolledToText.Text = this.directionScrollView.ScrollPosition.ToString();
    }
}
