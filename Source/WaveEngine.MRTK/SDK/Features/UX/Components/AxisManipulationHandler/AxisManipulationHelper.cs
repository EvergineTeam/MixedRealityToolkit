// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;

namespace WaveEngine.MRTK.SDK.Features.UX.Components.AxisManipulationHandler
{
    /// <summary>
    /// The type of axis manipulator helper, used to differentiate the various types of handles.
    /// </summary>
    public enum AxisManipulationHelperType
    {
        /// <summary>
        /// The center manipulator.
        /// </summary>
        Center = 0,

        /// <summary>
        /// The axis manipulators.
        /// </summary>
        Axis,

        /// <summary>
        /// The plane manipulators.
        /// </summary>
        Plane,
    }

    /// <summary>
    /// The axes the movement is restricted to.
    /// </summary>
    [Flags]
    public enum AxisType : int
    {
        /// <summary>
        /// No axis.
        /// </summary>
        None = 0,

        /// <summary>
        /// Only the X axis.
        /// </summary>
        X = 1 << 0,

        /// <summary>
        /// Only the Y axis.
        /// </summary>
        Y = 1 << 1,

        /// <summary>
        /// Only the Z axis.
        /// </summary>
        Z = 1 << 2,

        /// <summary>
        /// Only the X and Y axes.
        /// </summary>
        XY = X | Y,

        /// <summary>
        /// Only the Y and Z axes.
        /// </summary>
        YZ = Y | Z,

        /// <summary>
        /// Only the X and Z axes.
        /// </summary>
        XZ = X | Z,

        /// <summary>
        /// All axes.
        /// </summary>
        All = X | Y | Z,
    }

    /// <summary>
    /// Container for handle references.
    /// </summary>
    public class AxisManipulationHelper
    {
        /// <summary>
        /// The helper type.
        /// </summary>
        public AxisManipulationHelperType Type;

        /// <summary>
        /// The axis type.
        /// </summary>
        public AxisType AxisType;

        /// <summary>
        /// The handle base entity.
        /// </summary>
        public Entity BaseEntity;

        /// <summary>
        /// The list of MaterialComponents that will get their Materials changed when grabbing or focusing happen.
        /// </summary>
        public MaterialComponent[] MaterialComponents;

        /// <summary>
        /// The material applied to this helper when not grabbed.
        /// </summary>
        public Material IdleMaterial;

        /// <summary>
        /// The material applied to this helper when grabbed.
        /// </summary>
        public Material GrabbedMaterial;

        /// <summary>
        /// The material applied to this helper when focused.
        /// </summary>
        public Material FocusedMaterial;

        /// <summary>
        /// The related handles for this handle. Used for axis handles related to plane handles, in order to change their material in case the component is configured to do so.
        /// </summary>
        public AxisManipulationHelper[] RelatedHandles;
    }
}
