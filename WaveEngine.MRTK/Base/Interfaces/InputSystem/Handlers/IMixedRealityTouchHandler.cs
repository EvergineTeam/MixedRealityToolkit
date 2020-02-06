using WaveEngine.MRTK.Base.EventDatum.Input;

namespace WaveEngine.MRTK.Base.Interfaces.InputSystem.Handlers
{
    /// <summary>
    /// Implementation of this interface causes a component to receive notifications of Touch events from HandTrackingInputSources
    /// </summary>
    public interface IMixedRealityTouchHandler
    {
        void OnTouchStarted(HandTrackingInputEventData eventData);

        void OnTouchCompleted(HandTrackingInputEventData eventData);

        void OnTouchUpdated(HandTrackingInputEventData eventData);
    }
}
