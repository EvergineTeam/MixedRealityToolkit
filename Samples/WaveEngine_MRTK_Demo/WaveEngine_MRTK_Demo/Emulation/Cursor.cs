using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Physics3D;

namespace WaveEngine_MRTK_Demo.Emulation
{
    public class Cursor : Component
    {
        [BindComponent(isExactType: false)]
        public Collider3D Collider3D;

        [BindComponent]
        public StaticBody3D StaticBody3D;
    }
}
