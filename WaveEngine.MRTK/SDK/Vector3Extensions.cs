using WaveEngine.Mathematics;

namespace WaveEngine.MRTK.SDK
{
    public class Vector3Extensions
    {
        public static Vector3 Project(Vector3 vector, Vector3 onVector)
        {
            var lengthSquared = onVector.LengthSquared();

            if (MathHelper.FloatEquals(lengthSquared, 0.0f))
            {
                return Vector3.Zero;
            }
            else
            {
                return onVector * Vector3.Dot(vector, onVector) / lengthSquared;
            }
        }
    }
}
