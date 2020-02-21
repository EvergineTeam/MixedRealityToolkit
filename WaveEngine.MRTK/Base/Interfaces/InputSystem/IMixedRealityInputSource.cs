// Copyright © 2019 Wave Engine S.L. All rights reserved. Use is subject to license terms.

namespace WaveEngine.MixedReality.Toolkit.Input
{
    // TODO: IMixedRealityEventSource

    /// <summary>
    /// Interface for an input source.
    /// An input source is the origin of user input and generally comes from a physical controller, sensor, or other hardware device.
    /// </summary>
    public interface IMixedRealityInputSource ////: IMixedRealityEventSource
    {
        /// <summary>
        /// Gets the array of pointers associated with this input source.
        /// </summary>
        IMixedRealityPointer[] Pointers { get; }

        /// <summary>
        /// Gets the type of input source this object represents.
        /// </summary>
        InputSourceType SourceType { get; }
    }
}
