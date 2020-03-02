// Copyright © 2019 Wave Engine S.L. All rights reserved. Use is subject to license terms.

namespace WaveEngine.MixedReality.Toolkit
{
    /// <summary>
    /// Mixed reality platform capabilities.
    /// </summary>
    public enum MixedRealityCapability
    {
        /// <summary>
        /// Articulated hand input
        /// </summary>
        ArticulatedHand = 0,

        /// <summary>
        /// Gaze-Gesture-Voice hand input
        /// </summary>
        GGVHand,

        /// <summary>
        /// Motion controller input
        /// </summary>
        MotionController,

        /// <summary>
        /// Eye gaze targeting
        /// </summary>
        EyeTracking,

        /// <summary>
        /// Voice commands using app defined keywords
        /// </summary>
        VoiceCommand,

        /// <summary>
        /// Voice to text dictation
        /// </summary>
        VoiceDictation,

        /// <summary>
        /// Spatial meshes
        /// </summary>
        SpatialAwarenessMesh,

        /// <summary>
        /// Spatial planes
        /// </summary>
        SpatialAwarenessPlane,

        /// <summary>
        /// Spatial points
        /// </summary>
        SpatialAwarenessPoint,
    }
}
