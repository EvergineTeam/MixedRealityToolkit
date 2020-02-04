using WaveEngine.Framework;
using WaveEngine.Mathematics;

namespace WaveEngine.MRTK.EventDatum.Input
{
    public class HandTrackingInputEventData
    {
        public Vector3 Position { get; set; }

        public Entity Cursor { get; set; }
    }
}
