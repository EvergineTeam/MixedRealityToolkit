using System;
using WaveEngine.Common.Attributes;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Mathematics;
using WaveEngine.MRTK.Base.EventDatum.Input;
using WaveEngine.MRTK.Base.Interfaces.InputSystem.Handlers;
using WaveEngine.MRTK.Services.InputSystem;

namespace WaveEngine.MRTK.SDK.Features.UX.Components.Sliders
{
    public class PinchSlider : Component, IMixedRealityPointerHandler
    {
        [BindComponent]
        protected Transform3D transform;

        [BindComponent(source: BindComponentSource.Children)]
        protected NearInteractionGrabbable nearInteractionGrabbable;

        // Only Vector3.Up is currently supported in the slider, as rotating the base entity should suffice to accommodate the other axes.
        // It must be a normalized vector.
        private Vector3 SliderAxis { get; } = Vector3.Up;

        [RenderProperty(Tooltip = "Where the slider track starts, as distance from center along slider axis, in local space units.")]
        public float SliderStartDistance { get; set; } = -0.5f;

        [RenderProperty(Tooltip = "Where the slider track ends, as distance from center along slider axis, in local space units.")]
        public float SliderEndDistance { get; set; } = 0.5f;

        [RenderPropertyAsFInput(Tooltip = "The initial value for the slider, in the range of 0 to 1", MinLimit = 0, MaxLimit = 1)]
        public float InitialValue { get; set; } = 0.5f;

        [WaveIgnore]
        [DontRenderProperty]
        public float SliderValue
        {
            get => this.sliderValue;
            set
            {
                if (this.sliderValue != value)
                {
                    var oldValue = this.sliderValue;
                    this.sliderValue = value;
                    this.UpdateUI();

                    this.ValueUpdated?.Invoke(this, new SliderEventData(oldValue, this.sliderValue));
                }
            }
        }

        private Vector3 SliderStartPosition => this.SliderAxis * this.SliderStartDistance;

        private float SliderLength => this.SliderEndDistance - this.SliderStartDistance;

        public event EventHandler<SliderEventData> ValueUpdated;
        public event EventHandler InteractionStarted;
        public event EventHandler InteractionEnded;

        private Entity thumbRootEntity;
        private Transform3D thumbRootTransform;

        private float startSliderValue;
        private Vector3 startPointerPosition;
        private Entity activePointer;
        private Vector3 sliderThumbOffset;

        private float sliderValue;

        protected override void OnActivated()
        {
            base.OnActivated();

            this.thumbRootEntity = this.nearInteractionGrabbable.Owner;
            this.thumbRootTransform = this.thumbRootEntity.FindComponent<Transform3D>();

            // Store the position of the thumb entity relative to the closest point on the slider axis
            var startToThumb = this.thumbRootTransform.LocalPosition - this.SliderStartPosition;
            var thumbProjectedOnTrack = this.SliderStartPosition + Vector3Extensions.Project(startToThumb, this.SliderAxis);
            this.sliderThumbOffset = this.thumbRootTransform.LocalPosition - thumbProjectedOnTrack;

            this.sliderValue = this.InitialValue;
        }

        protected override void OnDeactivated()
        {
            base.OnDeactivated();

            this.EndInteraction();
        }

        private void UpdateUI()
        {
            this.thumbRootTransform.LocalPosition = this.SliderStartPosition + this.sliderThumbOffset + this.SliderAxis * this.SliderLength * this.sliderValue;
        }

        private void EndInteraction()
        {
            this.InteractionEnded?.Invoke(this, EventArgs.Empty);
            this.activePointer = null;
        }

        public void OnPointerDown(MixedRealityPointerEventData eventData)
        {
            if (this.activePointer == null)
            {
                this.activePointer = eventData.Cursor;
                this.InteractionStarted?.Invoke(this, EventArgs.Empty);

                this.startSliderValue = this.sliderValue;
                this.startPointerPosition = Vector3.TransformCoordinate(eventData.Position, this.transform.WorldInverseTransform);
            }
        }

        public void OnPointerDragged(MixedRealityPointerEventData eventData)
        {
            if (this.activePointer == eventData.Cursor)
            {
                var delta = Vector3.TransformCoordinate(eventData.Position, this.transform.WorldInverseTransform) - this.startPointerPosition;
                var handDelta = Vector3.Dot(this.SliderAxis, delta);

                this.SliderValue = MathHelper.Clamp(this.startSliderValue + handDelta / this.SliderLength, 0, 1);
            }
        }

        public void OnPointerUp(MixedRealityPointerEventData eventData)
        {
            if (this.activePointer == eventData.Cursor)
            {
                this.EndInteraction();
            }
        }

        public void OnPointerClicked(MixedRealityPointerEventData eventData)
        {
            // Nothing to do
        }
    }
}
