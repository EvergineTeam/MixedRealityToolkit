using WaveEngine.MRTK.Base.EventDatum.Input;

namespace WaveEngine.MRTK.Base.Interfaces.InputSystem.Handlers
{
    /// <summary>
    /// Implementation of this interface causes a component to receive notifications of Pointer events
    /// </summary>
    public interface IMixedRealityPointerHandler
    {
        void OnPointerDown(MixedRealityPointerEventData eventData);

        void OnPointerDragged(MixedRealityPointerEventData eventData);

        void OnPointerUp(MixedRealityPointerEventData eventData);

        void OnPointerClicked(MixedRealityPointerEventData eventData);
    }
}
