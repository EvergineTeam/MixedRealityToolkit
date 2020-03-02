// Copyright © 2019 Wave Engine S.L. All rights reserved. Use is subject to license terms.

namespace WaveEngine.MixedReality.Toolkit
{
    /// <summary>
    /// Interface that represent an object that checks capabilities.
    /// </summary>
    public interface IMixedRealityCapabilityCheck
    {
        /// <summary>
        /// Checks to see if one or more registered data providers supports the requested capability
        /// on the current platform.
        /// </summary>
        /// <param name="capability">The capability to check.</param>
        /// <returns>True if the capability is supported, false otherwise.</returns>
        bool CheckCapability(MixedRealityCapability capability);
    }
}
