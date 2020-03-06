// Copyright © 2019 Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;

namespace WaveEngine.MixedReality.Toolkit
{
    /// <summary>
    /// Generic interface for all Mixed Reality Services.
    /// </summary>
    public interface IMixedRealityService : IDisposable
    {
        /// <summary>
        /// Gets the pptional Priority attribute if multiple services of the same type are required, enables targeting a service for action.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the optional Priority to reorder registered managers based on their respective priority, reduces the risk of race conditions by prioritizing the order in which managers are evaluated.
        /// </summary>
        uint Priority { get; }

        /// <summary>
        /// The initialize function is used to setup the service once created.
        /// This method is called once all services have been registered in the Mixed Reality Toolkit.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Optional Reset function to perform that will Reset the service, for example, whenever there is a profile change.
        /// </summary>
        void Reset();

        /// <summary>
        /// Optional Activate function to enable / re-enable the service.
        /// </summary>
        void Activate();

        /// <summary>
        /// Optional Update function to perform per-frame updates of the service.
        /// </summary>
        void Update();

        /// <summary>
        /// Optional Disable function to deactivate the service.
        /// </summary>
        void Deactivate();

        /// <summary>
        /// Optional Detach function to detach the service.
        /// </summary>
        void Detach();

        /// <summary>
        /// Optional Destroy function to perform cleanup of the service before the Mixed Reality Toolkit is destroyed.
        /// </summary>
        void Destroy();
    }
}
