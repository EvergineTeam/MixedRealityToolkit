// Copyright © 2019 Wave Engine S.L. All rights reserved. Use is subject to license terms.

using WaveEngine.Framework;

namespace WaveEngine.MixedReality.Toolkit.Input
{
    /// <summary>
    /// The visualizer reference for a controller.
    /// </summary>
    public interface IMixedRealityControllerVisualizer
    {
        /// <summary>
        /// Gets the <see cref="Entity"/> reference for this controller.
        /// </summary>
        /// <remarks>
        /// This reference may not always be available when called.
        /// </remarks>
        Entity EntityProxy { get; }

        /// <summary>
        /// Gets or sets the current controller reference.
        /// </summary>
        IMixedRealityController Controller { get; set; }

        // TODO add defined elements or transforms?
    }
}
