using WaveEngine.Framework;
using WaveEngine.Mathematics;

namespace WaveEngine.MRTK.Base.EventDatum.Input
{
    public class MixedRealityPointerEventData
    {
        public Vector3 Position { get; set; }

        public Quaternion Orientation { get; set; }

        public Entity Cursor { get; set; }
    }
}
