// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Mathematics;

namespace WaveEngine.MRTK.SDK.Features.UX.Components.BoundingBox
{
    internal enum BoundingBoxHelperType
    {
        WireframeLink = 0,
        RotationHandle,
        ScaleHandle,
        NonUniformScaleHandle,
    }

    internal enum AxisType
    {
        None = 0,
        X,
        Y,
        Z,
    }

    /// <summary>
    /// Container for handle references.
    /// </summary>
    internal class BoundingBoxHelper
    {
        public BoundingBoxHelperType Type;
        public AxisType AxisType;

        public Entity Entity;
        public Transform3D Transform;

        public Vector3 OppositeHandlePosition;

        public Vector3 GetRotationAxis(Matrix4x4 transform)
        {
            switch (this.AxisType)
            {
                case AxisType.X:
                    return transform.Right;
                case AxisType.Y:
                    return transform.Up;
                case AxisType.Z:
                    return transform.Forward;
            }

            return Vector3.Zero;
        }
    }
}
