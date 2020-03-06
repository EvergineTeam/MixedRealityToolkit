// Copyright © 2019 Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using System.Collections.Generic;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.XR;
using WaveEngine.Mathematics;

namespace WaveEngine.MixedReality.Toolkit.Input
{
    /// <summary>
    /// Manager interface for a Input system in the Mixed Reality Toolkit
    /// All replacement systems for providing Input System functionality should derive from this interface.
    /// </summary>
    public interface IMixedRealityInputSystem : IMixedRealityEventSystem
    {
        /// <summary>
        /// Event that's raised when the Input is enabled.
        /// </summary>
        event Action InputEnabled;

        /// <summary>
        /// Event that's raised when the Input is disabled.
        /// </summary>
        event Action InputDisabled;

        /// <summary>
        /// Gets the list of the Interaction Input Sources as detected by the input manager like hands or motion controllers.
        /// </summary>
        HashSet<IMixedRealityInputSource> DetectedInputSources { get; }

        /// <summary>
        /// Gets the list of <see cref="IMixedRealityController"/>s currently detected by the input manager.
        /// </summary>
        /// <remarks>
        /// This property is similar to <see cref="DetectedInputSources"/>, as this is a subset of those <see cref="IMixedRealityInputSource"/>s in that list.
        /// </remarks>
        HashSet<IMixedRealityController> DetectedControllers { get; }

        // TODO: MixedRealityInputSystemProfile.
        /////// <summary>
        /////// Typed representation of the ConfigurationProfile property.
        /////// </summary>
        ////MixedRealityInputSystemProfile InputSystemProfile { get; }

        /// <summary>
        /// Gets the current Focus Provider that's been implemented by this Input System.
        /// </summary>
        IMixedRealityFocusProvider FocusProvider { get; }

        // TODO: IMixedRealityRaycastProvider.

        /////// <summary>
        /////// The current Raycast Provider that's been implemented by this Input System.
        /////// </summary>
        ////IMixedRealityRaycastProvider RaycastProvider { get; }

        // TODO: IMixedRealityGazeProvider.

        /////// <summary>
        /////// The current Gaze Provider that's been implemented by this Input System.
        /////// </summary>
        ////IMixedRealityGazeProvider GazeProvider { get; }

        // TODO: IMixedRealityEyeGazeProvider.

        /////// <summary>
        /////// The current Eye Gaze Provider that's been implemented by this Input System.
        /////// </summary>
        ////IMixedRealityEyeGazeProvider EyeGazeProvider { get; }

        /// <summary>
        /// Gets a value indicating whether the input is currently enabled or not.
        /// </summary>
        bool IsInputEnabled { get; }

        /// <summary>
        /// Push a disabled input state onto the Input System.
        /// While input is disabled no events will be sent out and the cursor displays
        /// a waiting animation.
        /// </summary>
        void PushInputDisable();

        /// <summary>
        /// Pop disabled input state. When the last disabled state is
        /// popped off the stack input will be re-enabled.
        /// </summary>
        void PopInputDisable();

        /// <summary>
        /// Clear the input disable stack, which will immediately re-enable input.
        /// </summary>
        void ClearInputDisableStack();

        /// <summary>
        /// Push a game object into the modal input stack. Any input handlers
        /// on the game object are given priority to input events before any focused objects.
        /// </summary>
        /// <param name="inputHandler">The input handler to push.</param>
        void PushModalInputHandler(Entity inputHandler);

        /// <summary>
        /// Remove the last game object from the modal input stack.
        /// </summary>
        void PopModalInputHandler();

        /// <summary>
        /// Clear all modal input handlers off the stack.
        /// </summary>
        void ClearModalInputStack();

        /// <summary>
        /// Push a game object into the fallback input stack. Any input handlers on
        /// the game object are given input events when no modal or focused objects consume the event.
        /// </summary>
        /// <param name="inputHandler">The input handler to push.</param>
        void PushFallbackInputHandler(Entity inputHandler);

        /// <summary>
        /// Remove the last game object from the fallback input stack.
        /// </summary>
        void PopFallbackInputHandler();

        /// <summary>
        /// Clear all fallback input handlers off the stack.
        /// </summary>
        void ClearFallbackInputStack();

        #region Input Events

        #region Input Source Events

        /// <summary>
        /// Generates a new unique input source id.<para/>
        /// </summary>
        /// <remarks>All Input Sources are required to call this method in their constructor or initialization.</remarks>
        /// <returns>a new unique Id for the input source.</returns>
        uint GenerateNewSourceId();

        /// <summary>
        /// Request a new generic input source.
        /// </summary>
        /// <param name="name">The input source name.</param>
        /// <param name="pointers">The associated pointers.</param>
        /// <param name="sourceType">The source type.</param>
        /// <returns>The new input source.</returns>
        IMixedRealityInputSource RequestNewGenericInputSource(string name, IMixedRealityPointer[] pointers = null, InputSourceType sourceType = InputSourceType.Other);

        /// <summary>
        /// Raise the event that the Input Source was detected.
        /// </summary>
        /// <param name="source">The detected Input Source.</param>
        /// <param name="controller">The controller.</param>
        void RaiseSourceDetected(IMixedRealityInputSource source, IMixedRealityController controller = null);

        /// <summary>
        /// Raise the event that the Input Source was lost.
        /// </summary>
        /// <param name="source">The lost Input Source.</param>
        /// <param name="controller">The controller.</param>
        void RaiseSourceLost(IMixedRealityInputSource source, IMixedRealityController controller = null);

        /// <summary>
        /// Raise the event that the Input Source's tracking state has changed.
        /// </summary>
        /// <param name="source">The detected Input Source.</param>
        /// <param name="controller">The controller.</param>
        /// <param name="state">The trackingstate.</param>
        void RaiseSourceTrackingStateChanged(IMixedRealityInputSource source, IMixedRealityController controller, TrackingState state);

        /// <summary>
        /// Raise the event that the Input Source position was changed.
        /// </summary>
        /// <param name="source">The detected Input Source.</param>
        /// <param name="controller">The controller.</param>
        /// <param name="position">The position.</param>
        void RaiseSourcePositionChanged(IMixedRealityInputSource source, IMixedRealityController controller, Vector2 position);

        /// <summary>
        /// Raise the event that the Input Source position was changed.
        /// </summary>
        /// <param name="source">The detected Input Source.</param>
        /// <param name="controller">The controller.</param>
        /// <param name="position">The position.</param>
        void RaiseSourcePositionChanged(IMixedRealityInputSource source, IMixedRealityController controller, Vector3 position);

        /// <summary>
        /// Raise the event that the Input Source position was changed.
        /// </summary>
        /// <param name="source">The detected Input Source.</param>
        /// <param name="controller">The controller.</param>
        /// <param name="rotation">The rotation.</param>
        void RaiseSourceRotationChanged(IMixedRealityInputSource source, IMixedRealityController controller, Quaternion rotation);

        /// <summary>
        /// Raise the event that the Input Source position was changed.
        /// </summary>
        /// <param name="source">The detected Input Source.</param>
        /// <param name="controller">The controller.</param>
        /// <param name="position">The position.</param>
        void RaiseSourcePoseChanged(IMixedRealityInputSource source, IMixedRealityController controller, XRPose position);

        #endregion Input Source Events

        #region Focus Events

        /// <summary>
        /// Raise the pre-focus changed event.
        /// </summary>
        /// <remarks>This event is useful for doing logic before the focus changed event.</remarks>
        /// <param name="pointer">The pointer that the focus change event is raised on.</param>
        /// <param name="oldfocusedEntity">The old focused object.</param>
        /// <param name="newfocusedEntity">The new focused object.</param>
        void RaisePreFocusChanged(IMixedRealityPointer pointer, Entity oldfocusedEntity, Entity newfocusedEntity);

        /// <summary>
        /// Raise the focus changed event.
        /// </summary>
        /// <param name="pointer">The pointer that the focus change event is raised on.</param>
        /// <param name="oldfocusedEntity">The old focused object.</param>
        /// <param name="newfocusedEntity">The new focused object.</param>
        void RaiseFocusChanged(IMixedRealityPointer pointer, Entity oldfocusedEntity, Entity newfocusedEntity);

        /// <summary>
        /// Raise the focus enter event.
        /// </summary>
        /// <param name="pointer">The pointer that has focus.</param>
        /// <param name="focusedEntity">The <see cref="Entity"/> that the pointer has entered focus on.</param>
        void RaiseFocusEnter(IMixedRealityPointer pointer, Entity focusedEntity);

        /// <summary>
        /// Raise the focus exit event.
        /// </summary>
        /// <param name="pointer">The pointer that has lost focus.</param>
        /// <param name="unfocusedEntity">The <see cref="Entity"/> that the pointer has exited focus on.</param>
        void RaiseFocusExit(IMixedRealityPointer pointer, Entity unfocusedEntity);

        #endregion Focus Events

        #region Pointers

        #region Pointer Down

        /// <summary>
        /// Raise the pointer down event.
        /// </summary>
        /// <param name="pointer">The pointer where the event originates.</param>
        /// <param name="inputAction">The input action.</param>
        /// <param name="handedness">The handedness.</param>
        /// <param name="inputSource">The input source.</param>
        void RaisePointerDown(IMixedRealityPointer pointer, MixedRealityInputAction inputAction, XRHandedness handedness = XRHandedness.Undefined, IMixedRealityInputSource inputSource = null);

        #endregion Pointer Down

        #region Pointer Dragged

        /// <summary>
        /// Raise the pointer dragged event.
        /// </summary>
        /// <param name="pointer">The pointer where the event originates.</param>
        /// <param name="inputAction">The input action.</param>
        /// <param name="handedness">The handedness.</param>
        /// <param name="inputSource">The input source.</param>
        void RaisePointerDragged(IMixedRealityPointer pointer, MixedRealityInputAction inputAction, XRHandedness handedness = XRHandedness.Undefined, IMixedRealityInputSource inputSource = null);

        #endregion Pointer Dragged

        #region Pointer Click

        /// <summary>
        /// Raise the pointer clicked event.
        /// </summary>
        /// <param name="pointer">The pointer where the event originates.</param>
        /// <param name="inputAction">The input action.</param>
        /// <param name="count">The pointer count.</param>
        /// <param name="handedness">The handedness.</param>
        /// <param name="inputSource">The input source.</param>
        void RaisePointerClicked(IMixedRealityPointer pointer, MixedRealityInputAction inputAction, int count, XRHandedness handedness = XRHandedness.Undefined, IMixedRealityInputSource inputSource = null);

        #endregion Pointer Click

        #region Pointer Up

        /// <summary>
        /// Raise the pointer up event.
        /// </summary>
        /// <param name="pointer">The pointer where the event originates.</param>
        /// <param name="inputAction">The input action.</param>
        /// <param name="handedness">The handedness.</param>
        /// <param name="inputSource">The input source.</param>
        void RaisePointerUp(IMixedRealityPointer pointer, MixedRealityInputAction inputAction, XRHandedness handedness = XRHandedness.Undefined, IMixedRealityInputSource inputSource = null);

        #endregion Pointer Up

        #endregion Pointers

        #region Generic Input Events

        #region Input Down

        /// <summary>
        /// Raise the input down event.
        /// </summary>
        /// <param name="source">The pointer where the event originates.</param>
        /// <param name="handedness">The handedness.</param>
        /// <param name="inputAction">The input action.</param>
        void RaiseOnInputDown(IMixedRealityInputSource source, XRHandedness handedness, MixedRealityInputAction inputAction);

        #endregion Input Down

        #region Input Up

        /// <summary>
        /// Raise the input up event.
        /// </summary>
        /// <param name="source">The pointer where the event originates.</param>
        /// <param name="handedness">The handedness.</param>
        /// <param name="inputAction">The input action.</param>
        void RaiseOnInputUp(IMixedRealityInputSource source, XRHandedness handedness, MixedRealityInputAction inputAction);

        #endregion Input Up

        #region Float Input Changed

        /// <summary>
        /// Raise Float Input Changed.
        /// </summary>
        /// <param name="source">The pointer where the event originates.</param>
        /// <param name="handedness">The handedness.</param>
        /// <param name="inputAction">The input action.</param>
        /// <param name="inputValue">The input value.</param>
        void RaiseFloatInputChanged(IMixedRealityInputSource source, XRHandedness handedness, MixedRealityInputAction inputAction, float inputValue);

        #endregion Float Input Changed

        #region Input Position Changed

        /// <summary>
        /// Raise the 2 degrees of freedom input event.
        /// </summary>
        /// <param name="source">The pointer where the event originates.</param>
        /// <param name="handedness">The handedness.</param>
        /// <param name="inputAction">The input action.</param>
        /// <param name="position">The input position.</param>
        void RaisePositionInputChanged(IMixedRealityInputSource source, XRHandedness handedness, MixedRealityInputAction inputAction, Vector2 position);

        /// <summary>
        /// Raise the 3 degrees of freedom input event.
        /// </summary>
        /// <param name="source">The pointer where the event originates.</param>
        /// <param name="handedness">The handedness.</param>
        /// <param name="inputAction">The input action.</param>
        /// <param name="position">The input position.</param>
        void RaisePositionInputChanged(IMixedRealityInputSource source, XRHandedness handedness, MixedRealityInputAction inputAction, Vector3 position);

        #endregion Input Position Changed

        #region Input Rotation Changed

        /// <summary>
        /// Raise the 3 degrees of freedom input event.
        /// </summary>
        /// <param name="source">The pointer where the event originates.</param>
        /// <param name="handedness">The handedness.</param>
        /// <param name="inputAction">The input action.</param>
        /// <param name="rotation">The input rotation.</param>
        void RaiseRotationInputChanged(IMixedRealityInputSource source, XRHandedness handedness, MixedRealityInputAction inputAction, Quaternion rotation);

        #endregion Input Rotation Changed

        #region Input Pose Changed

        /// <summary>
        /// Raise the 6 degrees of freedom input event.
        /// </summary>
        /// <param name="source">The pointer where the event originates.</param>
        /// <param name="handedness">The handedness.</param>
        /// <param name="inputAction">The input action.</param>
        /// <param name="inputData">The pose.</param>
        void RaisePoseInputChanged(IMixedRealityInputSource source, XRHandedness handedness, MixedRealityInputAction inputAction, XRPose inputData);

        #endregion Input Pose Changed

        #endregion Generic Input Events

        #region Generic Gesture Events

        /// <summary>
        /// Raise the Gesture Started Event.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="action">The input action.</param>
        void RaiseGestureStarted(IMixedRealityController controller, MixedRealityInputAction action);

        /// <summary>
        /// Raise the Gesture Updated Event.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="action">The input action.</param>
        void RaiseGestureUpdated(IMixedRealityController controller, MixedRealityInputAction action);

        /// <summary>
        /// Raise the Gesture Updated Event.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="action">The input action.</param>
        /// <param name="inputData">The input data.</param>
        void RaiseGestureUpdated(IMixedRealityController controller, MixedRealityInputAction action, Vector2 inputData);

        /// <summary>
        /// Raise the Gesture Updated Event.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="action">The input action.</param>
        /// <param name="inputData">The input data.</param>
        void RaiseGestureUpdated(IMixedRealityController controller, MixedRealityInputAction action, Vector3 inputData);

        /// <summary>
        /// Raise the Gesture Updated Event.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="action">The input action.</param>
        /// <param name="inputData">The input data.</param>
        void RaiseGestureUpdated(IMixedRealityController controller, MixedRealityInputAction action, Quaternion inputData);

        /// <summary>
        /// Raise the Gesture Updated Event.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="action">The input action.</param>
        /// <param name="inputData">The input data.</param>
        void RaiseGestureUpdated(IMixedRealityController controller, MixedRealityInputAction action, XRPose inputData);

        /// <summary>
        /// Raise the Gesture Completed Event.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="action">The input action.</param>
        void RaiseGestureCompleted(IMixedRealityController controller, MixedRealityInputAction action);

        /// <summary>
        /// Raise the Gesture Completed Event.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="action">The input action.</param>
        /// <param name="inputData">The input data.</param>
        void RaiseGestureCompleted(IMixedRealityController controller, MixedRealityInputAction action, Vector2 inputData);

        /// <summary>
        /// Raise the Gesture Completed Event.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="action">The input action.</param>
        /// <param name="inputData">The input data.</param>
        void RaiseGestureCompleted(IMixedRealityController controller, MixedRealityInputAction action, Vector3 inputData);

        /// <summary>
        /// Raise the Gesture Completed Event.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="action">The input action.</param>
        /// <param name="inputData">The input data.</param>
        void RaiseGestureCompleted(IMixedRealityController controller, MixedRealityInputAction action, Quaternion inputData);

        /// <summary>
        /// Raise the Gesture Completed Event.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="action">The input action.</param>
        /// <param name="inputData">The input data.</param>
        void RaiseGestureCompleted(IMixedRealityController controller, MixedRealityInputAction action, XRPose inputData);

        /// <summary>
        /// Raise the Gesture Canceled Event.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="action">The input action.</param>
        void RaiseGestureCanceled(IMixedRealityController controller, MixedRealityInputAction action);

        #endregion

        // TODO: Speech command events.
        /*
        #region Speech Keyword Events

        /// <summary>
        ///
        /// </summary>
        void RaiseSpeechCommandRecognized(IMixedRealityInputSource source, RecognitionConfidenceLevel confidence, TimeSpan phraseDuration, DateTime phraseStartTime, SpeechCommands command);

        #endregion Speech Keyword Events
        */

        // TODO: Dictation events.
        /*
        #region Dictation Events

        /// <summary>
        ///
        /// </summary>
        void RaiseDictationHypothesis(IMixedRealityInputSource source, string dictationHypothesis, AudioClip dictationAudioClip = null);

        /// <summary>
        ///
        /// </summary>
        void RaiseDictationResult(IMixedRealityInputSource source, string dictationResult, AudioClip dictationAudioClip = null);

        /// <summary>
        ///
        /// </summary>
        void RaiseDictationComplete(IMixedRealityInputSource source, string dictationResult, AudioClip dictationAudioClip);

        /// <summary>
        ///
        /// </summary>
        void RaiseDictationError(IMixedRealityInputSource source, string dictationResult, AudioClip dictationAudioClip = null);

        #endregion Dictation Events
        */

        #region Hand Events

        /// <summary>
        /// Notify system that articulated hand joint info has been updated.
        /// </summary>
        /// <param name="source">The input source.</param>
        /// <param name="handedness">The handedness.</param>
        /// <param name="jointPoses">The joint poses.</param>
        void RaiseHandJointsUpdated(IMixedRealityInputSource source, XRHandedness handedness, IDictionary<XRHandJoint, XRPose> jointPoses);

        /// <summary>
        /// Notify system that articulated hand mesh has been updated.
        /// </summary>
        /// <param name="source">The input source.</param>
        /// <param name="handedness">The handedness.</param>
        /// <param name="handMeshInfo">The hand mesh info.</param>
        void RaiseHandMeshUpdated(IMixedRealityInputSource source, XRHandedness handedness, Model handMeshInfo);

        /// <summary>
        /// Modify system, that a touch has been started.
        /// </summary>
        /// <param name="source">The input source.</param>
        /// <param name="controller">The controller.</param>
        /// <param name="handedness">The handedness.</param>
        /// <param name="touchPoint">The touch point.</param>
        void RaiseOnTouchStarted(IMixedRealityInputSource source, IMixedRealityController controller, XRHandedness handedness, Vector3 touchPoint);

        /// <summary>
        /// Modify system, that a touch has been updated.
        /// </summary>
        /// <param name="source">The input source.</param>
        /// <param name="controller">The controller.</param>
        /// <param name="handedness">The handedness.</param>
        /// <param name="touchPoint">The touch point.</param>
        void RaiseOnTouchUpdated(IMixedRealityInputSource source, IMixedRealityController controller, XRHandedness handedness, Vector3 touchPoint);

        /// <summary>
        /// Modify system, that a touch has been completed.
        /// </summary>
        /// <param name="source">The input source.</param>
        /// <param name="controller">The controller.</param>
        /// <param name="handedness">The handedness.</param>
        /// <param name="touchPoint">The touch point.</param>
        void RaiseOnTouchCompleted(IMixedRealityInputSource source, IMixedRealityController controller, XRHandedness handedness, Vector3 touchPoint);

        #endregion Hand Events

        #endregion Input Events
    }
}
