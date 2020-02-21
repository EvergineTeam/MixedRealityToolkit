// Copyright © 2019 Wave Engine S.L. All rights reserved. Use is subject to license terms.

using WaveEngine.Framework.XR;
using WaveEngine.Mathematics;

namespace WaveEngine.MixedReality.Toolkit.Input
{
    /// <summary>
    /// Mixed Reality Toolkit controller definition, used to manage a specific controller type.
    /// </summary>
    public interface IMixedRealityController
    {
        /// <summary>
        /// Gets or sets a value indicating whether the controller is enabled?.
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Gets the current state of the Input Source, whether it is tracked or not. As defined by the SDK / Unity.
        /// </summary>
        TrackingState TrackingState { get; }

        /// <summary>
        /// Gets the designated hand that the Input Source is managing, as defined by the SDK / Unity.
        /// </summary>
        XRHandedness ControllerHandedness { get; }

        /// <summary>
        /// Gets the registered Input Source for this controller.
        /// </summary>
        IMixedRealityInputSource InputSource { get; }

        /// <summary>
        /// Gets the controller's "Visual" <see href="https://docs.unity3d.com/ScriptReference/Component.html">Component</see> in the scene.
        /// </summary>
        IMixedRealityControllerVisualizer Visualizer { get; }

        /// <summary>
        /// Gets a value indicating whether this controller is currently providing position data.
        /// </summary>
        /// <remarks>
        /// This value may change during usage for some controllers. As a best practice,
        /// be sure to check this value before using position data.
        /// </remarks>
        bool IsPositionAvailable { get; }

        /// <summary>
        /// Gets a value indicating whether the accuracy of the position data being reported.
        /// </summary>
        bool IsPositionApproximate { get; }

        /// <summary>
        /// Gets a value indicating whether this controller is currently providing rotation data.
        /// </summary>
        /// <remarks>
        /// This value may change during usage for some controllers. As a best practice,
        /// be sure to check this value before using rotation data.
        /// </remarks>
        bool IsRotationAvailable { get; }

        // TODO: MixedRealityInteractionMapping
        /////// <summary>
        /////// Gets the mapping definition for this controller, linking the Physical inputs to logical Input System Actions.
        /////// </summary>
        ////MixedRealityInteractionMapping[] Interactions { get; }

        /// <summary>
        /// Gets the angular velocity.
        /// </summary>
        Vector3 AngularVelocity { get; }

        /// <summary>
        /// Gets the velocity.
        /// </summary>
        Vector3 Velocity { get; }

        /// <summary>
        /// Gets a value indicating whether this controller is in pointing pose.
        /// </summary>
        /// <remarks>
        /// Some controllers such as articulated should only be able
        /// to invoke pointing/distant interactions in certain poses.
        /// </remarks>
        bool IsInPointingPose { get; }
    }
}
