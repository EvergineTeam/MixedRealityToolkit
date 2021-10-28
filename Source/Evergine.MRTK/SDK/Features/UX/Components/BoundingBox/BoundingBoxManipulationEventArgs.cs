// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using System;
using System.Collections.Generic;
using System.Text;
using Evergine.Framework;

namespace Evergine.MRTK.SDK.Features.UX.Components.BoundingBox
{
    /// <summary>
    /// Information for a manipulation event in <see cref="BoundingBox"/>.
    /// </summary>
    public class BoundingBoxManipulationEventArgs
    {
        /// <summary>
        /// Gets the handle that initiated the manipulation.
        /// </summary>
        public BoundingBoxHelper Handle { get; internal set; }

        /// <summary>
        /// Gets the value of the manipulation. If it is a uniform scale manipulation, it will represent the change in scale applied to the object.
        /// If it is a non-uniform scale or a rotation, it will represent the amount of transformation applied in <see cref="AxisType"/>.
        /// It has a null value in manipulation start and end.
        /// </summary>
        public float? Value { get; internal set; }
    }
}
