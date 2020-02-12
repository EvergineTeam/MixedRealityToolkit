using WaveEngine.Framework;
using WaveEngine.Framework.Physics3D;

namespace WaveEngine.MRTK.Services.InputSystem
{
    public class NearInteractionGrabbable : Component
    {
        [BindComponent(isExactType: false)]
        public Collider3D Collider3D;

        [BindComponent]
        public StaticBody3D StaticBody3D;
    }
}
