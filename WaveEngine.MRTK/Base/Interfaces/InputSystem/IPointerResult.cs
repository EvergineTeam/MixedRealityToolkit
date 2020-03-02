// Copyright © 2019 Wave Engine S.L. All rights reserved. Use is subject to license terms.

using WaveEngine.Framework;
using WaveEngine.Mathematics;
using WaveEngine.MixedReality.Toolkit.Physics;

namespace WaveEngine.MixedReality.Toolkit.Input
{
    /// <summary>
    /// Interface defining a pointer result.
    /// </summary>
    public interface IPointerResult
    {
        /// <summary>
        /// Gets the starting point of the Pointer RaySteps.
        /// </summary>
        Vector3 StartPoint { get; }

        /// <summary>
        /// Gets the details about the currently focused <see cref="Entity"/>.
        /// </summary>
        FocusDetails Details { get; }

        /// <summary>
        /// Gets the current pointer's target <see cref="Entity"/>.
        /// </summary>
        Entity CurrentPointerTarget { get; }

        /// <summary>
        /// Gets the previous pointer target.
        /// </summary>
        Entity PreviousPointerTarget { get; }

        /// <summary>
        /// Gets the index of the step that produced the last raycast hit, 0 when no raycast hit.
        /// </summary>
        int RayStepIndex { get; }
    }
}
