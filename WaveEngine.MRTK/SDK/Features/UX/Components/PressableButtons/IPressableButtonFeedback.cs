using WaveEngine.Mathematics;

namespace WaveEngine.MRTK.SDK.Features.UX.Components.PressableButtons
{
    public interface IPressableButtonFeedback
    {
        void Feedback(Vector3 pushVector, Matrix4x4 colliderTransform, float pressRatio, bool pressed);
    }
}