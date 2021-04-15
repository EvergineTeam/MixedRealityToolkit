// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Mathematics;

namespace WaveEngine.MRTK.SDK.Features.UX.Components.BoundingBox
{
    /// <summary>
    /// The type of bounding box helper, used to differentiate the various types of handles.
    /// </summary>
    public enum BoundingBoxHelperType
    {
        /// <summary>
        /// A wireframe link
        /// </summary>
        WireframeLink = 0,

        /// <summary>
        /// A rotation handle.
        /// </summary>
        RotationHandle,

        /// <summary>
        /// A scale handle.
        /// </summary>
        ScaleHandle,

        /// <summary>
        /// A non-uniform scale handle.
        /// </summary>
        NonUniformScaleHandle,
    }

    /// <summary>
    /// The axis type for a bounding box helper.
    /// </summary>
    public enum AxisType
    {
        /// <summary>
        /// No specific axis.
        /// </summary>
        None = 0,

        /// <summary>
        /// The X axis.
        /// </summary>
        X,

        /// <summary>
        /// The Y axis.
        /// </summary>
        Y,

        /// <summary>
        /// The Z axis.
        /// </summary>
        Z,
    }

    /// <summary>
    /// Container for handle references.
    /// </summary>
    public class BoundingBoxHelper
    {
        /// <summary>
        /// The helper type.
        /// </summary>
        public BoundingBoxHelperType Type;

        /// <summary>
        /// The axis type.
        /// </summary>
        public AxisType AxisType;

        /// <summary>
        /// The handle entity.
        /// </summary>
        public Entity Entity;

        /// <summary>
        /// The handle transform.
        /// </summary>
        public Transform3D Transform;

        internal Vector3 OppositeHandlePosition;

        internal Vector3 GetRotationAxis(Matrix4x4 transform)
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
