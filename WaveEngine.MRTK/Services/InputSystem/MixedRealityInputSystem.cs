// Copyright © 2019 Wave Engine S.L. All rights reserved. Use is subject to license terms.

using WaveEngine.MixedReality.Toolkit.Utilities;
using System;
using System.Collections.Generic;
using WaveEngine.Framework;
using System.Diagnostics;
using WaveEngine.Framework.XR;
using WaveEngine.Mathematics;

namespace WaveEngine.MixedReality.Toolkit.Input
{
    /// <summary>
    /// The Mixed Reality Toolkit's specific implementation of the <see cref="WaveEngine.MixedReality.Toolkit.Input.IMixedRealityInputSystem"/>.
    /// </summary>
    public class MixedRealityInputSystem : BaseDataProviderAccessCoreSystem, IMixedRealityInputSystem, IMixedRealityCapabilityCheck
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MixedRealityInputSystem"/> class.
        /// </summary>
        /// <param name="registrar">The <see cref="IMixedRealityServiceRegistrar"/> instance that loaded the service.</param>
        /// <param name="profile">The configuration profile for the service.</param>
        [Obsolete("This constructor is obsolete (registrar parameter is no longer required) and will be removed in a future version of the Microsoft Mixed Reality Toolkit.")]
        public MixedRealityInputSystem(
            IMixedRealityServiceRegistrar registrar,
            MixedRealityInputSystemProfile profile) : this(profile)
        {
            Registrar = registrar;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MixedRealityInputSystem"/> class.
        /// </summary>
        /// <param name="profile">The configuration profile for the service.</param>
        public MixedRealityInputSystem(
            MixedRealityInputSystemProfile profile) : base(profile)
        { }

        /// <inheritdoc/>
        public override string Name { get; protected set; } = "Mixed Reality Input System";

        /// <inheritdoc />
        public event Action InputEnabled;

        /// <inheritdoc />
        public event Action InputDisabled;

        /// <inheritdoc />
        public HashSet<IMixedRealityInputSource> DetectedInputSources { get; } = new HashSet<IMixedRealityInputSource>();

        /// <inheritdoc />
        public HashSet<IMixedRealityController> DetectedControllers { get; } = new HashSet<IMixedRealityController>();

        private MixedRealityInputSystemProfile inputSystemProfile = null;

        /// <inheritdoc/>
        public MixedRealityInputSystemProfile InputSystemProfile
        {
            get
            {
                if (this.inputSystemProfile == null)
                {
                    this.inputSystemProfile = ConfigurationProfile as MixedRealityInputSystemProfile;
                }

                return this.inputSystemProfile;
            }
        }

        /// <inheritdoc />
        public IMixedRealityFocusProvider FocusProvider => CoreServices.FocusProvider;

        /// <inheritdoc />
        public IMixedRealityRaycastProvider RaycastProvider => CoreServices.RaycastProvider;

        /// <inheritdoc />
        public IMixedRealityGazeProvider GazeProvider { get; private set; }

        /// <inheritdoc />
        public IMixedRealityEyeGazeProvider EyeGazeProvider { get; private set; }

        private readonly Stack<Entity> modalInputStack = new Stack<Entity>();
        private readonly Stack<Entity> fallbackInputStack = new Stack<Entity>();

        /// <inheritdoc />
        public bool IsInputEnabled => this.disabledRefCount <= 0;

        private int disabledRefCount;
        private bool isInputModuleAdded = false;

        private SourceStateEventData sourceStateEventData;
        private SourcePoseEventData<TrackingState> sourceTrackingEventData;
        private SourcePoseEventData<Vector2> sourceVector2EventData;
        private SourcePoseEventData<Vector3> sourcePositionEventData;
        private SourcePoseEventData<Quaternion> sourceRotationEventData;
        private SourcePoseEventData<MixedRealityPose> sourcePoseEventData;

        private FocusEventData focusEventData;

        private InputEventData inputEventData;
        private MixedRealityPointerEventData pointerEventData;

        private InputEventData<float> floatInputEventData;
        private InputEventData<Vector2> vector2InputEventData;
        private InputEventData<Vector3> positionInputEventData;
        private InputEventData<Quaternion> rotationInputEventData;
        private InputEventData<MixedRealityPose> poseInputEventData;
        private InputEventData<IDictionary<TrackedHandJoint, MixedRealityPose>> jointPoseInputEventData;
        private InputEventData<HandMeshInfo> handMeshInputEventData;

        private SpeechEventData speechEventData;
        private DictationEventData dictationEventData;

        private HandTrackingInputEventData handTrackingInputEventData;

        private MixedRealityInputActionRulesProfile CurrentInputActionRulesProfile { get; set; }

        #region IMixedRealityCapabilityCheck Implementation

        /// <inheritdoc />
        public bool CheckCapability(MixedRealityCapability capability)
        {
            foreach (var deviceManager in this.GetDataProviders<IMixedRealityInputDeviceManager>())
            {
                IMixedRealityCapabilityCheck capabilityChecker = deviceManager as IMixedRealityCapabilityCheck;

                // If one of the running data providers supports the requested capability,
                // the application has the needed support to leverage the desired functionality.
                if (capabilityChecker?.CheckCapability(capability) == true)
                {
                    return true;
                }
            }

            // Check GazeProvider directly since not populated in data provider list but life-cycle is managed by InputSystem
            var gazeProvider_CapabilityCheck = this.GazeProvider as IMixedRealityCapabilityCheck;
            if (gazeProvider_CapabilityCheck?.CheckCapability(capability) == true)
            {
                return true;
            }

            return false;
        }

        #endregion IMixedRealityCapabilityCheck Implementation

        #region IMixedRealityService Implementation

        /// <inheritdoc />
        /// <remarks>
        /// Input system is critical, so should be processed before all other managers.
        /// </remarks>
        public override uint Priority => 1;

        /// <inheritdoc />
        public override void Initialize()
        {
            MixedRealityInputSystemProfile profile = ConfigurationProfile as MixedRealityInputSystemProfile;
            if (profile == null)
            {
                Trace.TraceError("The Input system is missing the required Input System Profile!");
                return;
            }

            BaseInputModule[] inputModules = UnityEngine.Object.FindObjectsOfType<BaseInputModule>();

            if (inputModules.Length == 0)
            {
                // There is no input module attached to the camera, add one.
                CameraCache.Main.Entity.AddComponent<MixedRealityInputModule>();
                this.isInputModuleAdded = true;
            }
            else if ((inputModules.Length == 1) && (inputModules[0] is MixedRealityInputModule))
            { /* Nothing to do, a MixedRealityInputModule was applied in the editor. */
            }
            else
            {
                Trace.TraceError($"For Mixed Reality Toolkit input to work properly, please remove your other input module(s) and add a {typeof(MixedRealityInputModule).Name} to your main camera.", inputModules[0]);
            }

            if (this.InputSystemProfile == null)
            {
                Trace.TraceError("The Input system is missing the required Input System Profile!");
                return;
            }

            if (profile.InputActionRulesProfile != null)
            {
                this.CurrentInputActionRulesProfile = profile.InputActionRulesProfile;
            }
            else
            {
                Trace.TraceError("The Input system is missing the required Input Action Rules Profile!");
                return;
            }

            if (profile.PointerProfile != null)
            {
                InstantiateGazeProvider(profile.PointerProfile);
            }
            else
            {
                Trace.TraceError("The Input system is missing the required Pointer Profile!");
                return;
            }

            this.sourceStateEventData = new SourceStateEventData(EventSystem.current);

            this.sourceTrackingEventData = new SourcePoseEventData<TrackingState>(EventSystem.current);
            this.sourceVector2EventData = new SourcePoseEventData<Vector2>(EventSystem.current);
            this.sourcePositionEventData = new SourcePoseEventData<Vector3>(EventSystem.current);
            this.sourceRotationEventData = new SourcePoseEventData<Quaternion>(EventSystem.current);
            this.sourcePoseEventData = new SourcePoseEventData<MixedRealityPose>(EventSystem.current);

            this.focusEventData = new FocusEventData(EventSystem.current);

            this.inputEventData = new InputEventData(EventSystem.current);
            this.pointerEventData = new MixedRealityPointerEventData(EventSystem.current);

            this.floatInputEventData = new InputEventData<float>(EventSystem.current);
            this.vector2InputEventData = new InputEventData<Vector2>(EventSystem.current);
            this.positionInputEventData = new InputEventData<Vector3>(EventSystem.current);
            this.rotationInputEventData = new InputEventData<Quaternion>(EventSystem.current);
            this.poseInputEventData = new InputEventData<MixedRealityPose>(EventSystem.current);
            this.jointPoseInputEventData = new InputEventData<IDictionary<TrackedHandJoint, MixedRealityPose>>(EventSystem.current);
            this.handMeshInputEventData = new InputEventData<HandMeshInfo>(EventSystem.current);

            this.speechEventData = new SpeechEventData(EventSystem.current);
            this.dictationEventData = new DictationEventData(EventSystem.current);

            this.handTrackingInputEventData = new HandTrackingInputEventData(EventSystem.current);

            this.CreateDataProviders();
        }

        /// <inheritdoc />
        public override void Enable()
        {
            this.CreateDataProviders();

            // Ensure data providers are enabled (performed by the base class)
            base.Enable();

            this.InputEnabled?.Invoke();
        }

        private void CreateDataProviders()
        {
            MixedRealityInputSystemProfile profile = ConfigurationProfile as MixedRealityInputSystemProfile;

            // If the system gets disabled, the gaze provider is destroyed.
            // Ensure that it gets recreated on when re-enabled.
            if (this.GazeProvider == null && profile != null)
            {
                InstantiateGazeProvider(profile.PointerProfile);
            }

            if ((GetDataProviders().Count == 0) && (profile != null))
            {
                // Register the input device managers.
                for (int i = 0; i < profile.DataProviderConfigurations.Length; i++)
                {
                    MixedRealityInputDataProviderConfiguration configuration = profile.DataProviderConfigurations[i];
                    object[] args = { this, configuration.ComponentName, configuration.Priority, configuration.DeviceManagerProfile };

                    RegisterDataProvider<IMixedRealityInputDeviceManager>(
                        configuration.ComponentType.Type,
                        configuration.RuntimePlatform,
                        args);
                }
            }
        }

        private void InstantiateGazeProvider(MixedRealityPointerProfile pointerProfile)
        {
            if (pointerProfile != null && pointerProfile.GazeProviderType?.Type != null)
            {
                this.GazeProvider = CameraCache.Main.Entity.EnsureComponent(pointerProfile.GazeProviderType.Type) as IMixedRealityGazeProvider;
                this.GazeProvider.GazeCursorPrefab = pointerProfile.GazeCursorPrefab;

                // Current implementation implements both provider types in one concrete class.
                this.EyeGazeProvider = this.GazeProvider as IMixedRealityEyeGazeProvider;
            }
            else
            {
                Trace.TraceError("The Input system is missing the required GazeProviderType!");
                return;
            }
        }

        /// <inheritdoc />
        public override void Reset()
        {
            base.Reset();
            this.Disable();
            this.Initialize();
            this.Enable();
        }

        /// <inheritdoc />
        public override void Disable()
        {
            base.Disable();

            // Input System adds a gaze provider component on the main camera, which needs to be removed when the input system is disabled/removed.
            // Otherwise the component would keep references to dead objects.
            // Unity's way to remove component is to destroy it.
            if (this.GazeProvider != null)
            {
                if (Application.isPlaying)
                {
                    this.GazeProvider.GazePointer.BaseCursor.Destroy();
                }

                UnityObjectExtensions.DestroyObject(this.GazeProvider as Component);

                this.GazeProvider = null;
            }

            foreach (var provider in this.GetDataProviders<IMixedRealityInputDeviceManager>())
            {
                if (provider != null)
                {
                    UnregisterDataProvider<IMixedRealityInputDeviceManager>(provider);
                }
            }

            this.InputDisabled?.Invoke();
        }

        public override void Destroy()
        {
            if (this.isInputModuleAdded)
            {
                var inputModule = CameraCache.Main.Entity.GetComponent<MixedRealityInputModule>();
                if (inputModule)
                {
                    if (Application.isPlaying)
                    {
                        inputModule.DeactivateModule();
                    }

                    UnityObjectExtensions.DestroyObject(inputModule);
                }
            }

            base.Destroy();
        }
        #endregion IMixedRealityService Implementation

        #region IMixedRealityDataProviderAccess Implementation

        /// <inheritdoc />
        public override IReadOnlyList<T> GetDataProviders<T>()
        {
            if (!typeof(IMixedRealityInputDeviceManager).IsAssignableFrom(typeof(T)))
            {
                return null;
            }

            return base.GetDataProviders<T>();
        }

        /// <inheritdoc />
        public override T GetDataProvider<T>(string name = null)
        {
            if (!typeof(IMixedRealityInputDeviceManager).IsAssignableFrom(typeof(T)))
            {
                return default(T);
            }

            return base.GetDataProvider<T>(name);
        }

        #endregion IMixedRealityDataProviderAccess Implementation

        #region IMixedRealityEventSystem Implementation

        /// <inheritdoc />
        public override void HandleEvent<T>(BaseEventData eventData, ExecuteEvents.EventFunction<T> eventHandler)
        {
            if (this.disabledRefCount > 0)
            {
                return;
            }

            Debug.Assert(eventData != null);
            Debug.Assert(!(eventData is MixedRealityPointerEventData), "HandleEvent called with a pointer event. All events raised by pointer should call HandlePointerEvent");

            var baseInputEventData = ExecuteEvents.ValidateEventData<BaseInputEventData>(eventData);
            DispatchEventToGlobalListeners(baseInputEventData, eventHandler);

            if (baseInputEventData.used)
            {
                // All global listeners get a chance to see the event,
                // but if any of them marked it used,
                // we stop the event from going any further.
                return;
            }

            if (baseInputEventData.InputSource.Pointers == null)
            {
                Trace.TraceError($"InputSource {baseInputEventData.InputSource.SourceName} doesn't have any registered pointers! Input Sources without pointers should use the GazeProvider's pointer as a default fallback.");
            }

            var modalEventHandled = false;

            // Get the focused object for each pointer of the event source
            for (int i = 0; i < baseInputEventData.InputSource.Pointers.Length && !baseInputEventData.used; i++)
            {
                modalEventHandled = DispatchEventToObjectFocusedByPointer(baseInputEventData.InputSource.Pointers[i], baseInputEventData, modalEventHandled, eventHandler);
            }

            if (!baseInputEventData.used)
            {
                DispatchEventToFallbackHandlers(baseInputEventData, eventHandler);
            }
        }

        /// <summary>
        /// Handles focus changed events
        /// We send all focus events to all global listeners and the actual focus change receivers. the use flag is completely ignored to avoid any interception.
        /// </summary>
        private void HandleFocusChangedEvents(FocusEventData focusEventData, ExecuteEvents.EventFunction<IMixedRealityFocusChangedHandler> eventHandler)
        {
            Debug.Assert(focusEventData != null);

            DispatchEventToGlobalListeners(focusEventData, eventHandler);

            // Raise Focus Events on the old and new focused objects.
            if (focusEventData.OldFocusedObject != null)
            {
                ExecuteEvents.ExecuteHierarchy(focusEventData.OldFocusedObject, focusEventData, eventHandler);
            }

            if (focusEventData.NewFocusedObject != null)
            {
                ExecuteEvents.ExecuteHierarchy(focusEventData.NewFocusedObject, focusEventData, eventHandler);
            }

            // Raise Focus Events on the pointers cursor if it has one.
            if (focusEventData.Pointer != null && focusEventData.Pointer.BaseCursor != null)
            {
                try
                {
                    // When shutting down a game, we can sometime get old references to game objects that have been cleaned up.
                    // We'll ignore when this happens.
                    ExecuteEvents.ExecuteHierarchy(focusEventData.Pointer.BaseCursor.EntityReference, focusEventData, eventHandler);

                }
                catch (Exception)
                {
                    // ignored.
                }
            }
        }

        /// <summary>
        /// Handles focus enter and exit
        /// We send the focus event to all global listeners and the actual focus change receiver. the use flag is completely ignored to avoid any interception.
        /// </summary>
        private void HandleFocusEvent(Entity eventTarget, FocusEventData focusEventData, ExecuteEvents.EventFunction<IMixedRealityFocusHandler> eventHandler)
        {
            Debug.Assert(focusEventData != null);

            DispatchEventToGlobalListeners(focusEventData, eventHandler);

            ExecuteEvents.ExecuteHierarchy(eventTarget, focusEventData, eventHandler);
        }

        /// <summary>
        /// Handles a pointer event
        /// Assumption: We only send pointer events to the objects that pointers are focusing, except for global event listeners (which listen to everything)
        /// In contract, all other events get sent to all other pointers attached to a given input source.
        /// </summary>
        private void HandlePointerEvent<T>(BaseEventData eventData, ExecuteEvents.EventFunction<T> eventHandler) where T : IMixedRealityPointerHandler
        {
            if (this.disabledRefCount > 0)
            {
                return;
            }

            Debug.Assert(eventData != null);
            var baseInputEventData = ExecuteEvents.ValidateEventData<BaseInputEventData>(eventData);
            DispatchEventToGlobalListeners(baseInputEventData, eventHandler);

            if (baseInputEventData.used)
            {
                // All global listeners get a chance to see the event,
                // but if any of them marked it used,
                // we stop the event from going any further.
                return;
            }

            Debug.Assert(this.pointerEventData.Pointer != null, "Trying to dispatch event on pointer but pointerEventData is null");

            DispatchEventToObjectFocusedByPointer(this.pointerEventData.Pointer, baseInputEventData, false, eventHandler);

            if (!baseInputEventData.used)
            {
                DispatchEventToFallbackHandlers(baseInputEventData, eventHandler);
            }
        }

        /// <summary>
        /// Dispatch an input event to all global event listeners
        /// Return true if the event has been handled by a global event listener.
        /// </summary>
        private void DispatchEventToGlobalListeners<T>(BaseInputEventData baseInputEventData, ExecuteEvents.EventFunction<T> eventHandler) where T : IEventSystemHandler
        {
            Debug.Assert(baseInputEventData != null);
            Debug.Assert(!baseInputEventData.used);
            if (baseInputEventData.InputSource == null)
            {
                Debug.Assert(baseInputEventData.InputSource != null, $"Failed to find an input source for {baseInputEventData}");
            }

            // Send the event to global listeners
            base.HandleEvent(baseInputEventData, eventHandler);
        }

        /// <summary>
        /// Dispatch a focus event to all global event listeners.
        /// </summary>
        private void DispatchEventToGlobalListeners<T>(FocusEventData focusEventData, ExecuteEvents.EventFunction<T> eventHandler) where T : IEventSystemHandler
        {
            Debug.Assert(focusEventData != null);
            Debug.Assert(!focusEventData.used);

            // Send the event to global listeners
            base.HandleEvent(focusEventData, eventHandler);
        }

        private void DispatchEventToFallbackHandlers<T>(BaseInputEventData baseInputEventData, ExecuteEvents.EventFunction<T> eventHandler) where T : IEventSystemHandler
        {
            // If event was not handled by the focused object, pass it on to any fallback handlers
            if (!baseInputEventData.used && this.fallbackInputStack.Count > 0)
            {
                Entity fallbackInput = this.fallbackInputStack.Peek();
                ExecuteEvents.ExecuteHierarchy(fallbackInput, baseInputEventData, eventHandler);
            }
        }

        /// <summary>
        /// Dispatch an input event to the object focused by the given IMixedRealityPointer.
        /// If a modal dialog is active, dispatch the pointer event to that modal dialog
        /// Returns true if the event was handled by a modal handler.
        /// </summary>
        private bool DispatchEventToObjectFocusedByPointer<T>(IMixedRealityPointer mixedRealityPointer, BaseInputEventData baseInputEventData,
            bool modalEventHandled, ExecuteEvents.EventFunction<T> eventHandler) where T : IEventSystemHandler
        {
            Entity focusedObject = this.FocusProvider?.GetFocusedObject(mixedRealityPointer);

            // Handle modal input if one exists
            if (this.modalInputStack.Count > 0 && !modalEventHandled)
            {
                Entity modalInput = this.modalInputStack.Peek();

                if (modalInput != null)
                {
                    // If there is a focused object in the hierarchy of the modal handler, start the event bubble there
                    if (focusedObject != null && focusedObject.transform.IsChildOf(modalInput.transform))
                    {
                        if (ExecuteEvents.ExecuteHierarchy(focusedObject, baseInputEventData, eventHandler) && baseInputEventData.used)
                        {
                            return true;
                        }
                    }

                    // Otherwise, just invoke the event on the modal handler itself
                    else
                    {
                        if (ExecuteEvents.ExecuteHierarchy(modalInput, baseInputEventData, eventHandler) && baseInputEventData.used)
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    Trace.TraceError("ModalInput Entity reference was null!\nDid this Entity get destroyed?");
                }
            }

            // If event was not handled by modal, pass it on to the current focused object
            if (focusedObject != null)
            {
                ExecuteEvents.ExecuteHierarchy(focusedObject, baseInputEventData, eventHandler);
            }

            return modalEventHandled;
        }

        /// <summary>
        /// Register a <see href="https://docs.unity3d.com/ScriptReference/Entity.html">Entity</see> to listen to events that will receive all input events, regardless
        /// of which other <see href="https://docs.unity3d.com/ScriptReference/Entity.html">Entity</see>s might have handled the event beforehand.
        /// </summary>
        /// <remarks>Useful for listening to events when the <see href="https://docs.unity3d.com/ScriptReference/Entity.html">Entity</see> is currently not being raycasted against by the <see cref="FocusProvider"/>.</remarks>
        /// <param name="listener">Listener to add.</param>
        public override void Register(Entity listener)
        {
            base.Register(listener);
        }

        /// <summary>
        /// Unregister a <see href="https://docs.unity3d.com/ScriptReference/Entity.html">Entity</see> from listening to input events.
        /// </summary>
        public override void Unregister(Entity listener)
        {
            base.Unregister(listener);
        }

        #endregion IMixedRealityEventSystem Implementation

        #region Input Disabled Options

        /// <summary>
        /// Push a disabled input state onto the input manager.
        /// While input is disabled no events will be sent out and the cursor displays
        /// a waiting animation.
        /// </summary>
        public void PushInputDisable()
        {
            ++this.disabledRefCount;

            if (this.disabledRefCount == 1)
            {
                this.InputDisabled?.Invoke();

                if (this.GazeProvider != null)
                {
                    this.GazeProvider.Enabled = false;
                }
            }
        }

        /// <summary>
        /// Pop disabled input state. When the last disabled state is
        /// popped off the stack input will be re-enabled.
        /// </summary>
        public void PopInputDisable()
        {
            --this.disabledRefCount;
            Debug.Assert(this.disabledRefCount >= 0, "Tried to pop more input disable than the amount pushed.");

            if (this.disabledRefCount == 0)
            {
                this.InputEnabled?.Invoke();

                if (this.GazeProvider != null)
                {
                    this.GazeProvider.Enabled = true;
                }
            }
        }

        /// <summary>
        /// Clear the input disable stack, which will immediately re-enable input.
        /// </summary>
        public void ClearInputDisableStack()
        {
            bool wasInputDisabled = this.disabledRefCount > 0;
            this.disabledRefCount = 0;

            if (wasInputDisabled)
            {
                this.InputEnabled?.Invoke();

                if (this.GazeProvider != null)
                {
                    this.GazeProvider.Enabled = true;
                }
            }
        }

        #endregion Input Disabled Options

        #region Modal Input Options

        /// <summary>
        /// Push a game object into the modal input stack. Any input handlers
        /// on the game object are given priority to input events before any focused objects.
        /// </summary>
        /// <param name="inputHandler">The input handler to push.</param>
        public void PushModalInputHandler(Entity inputHandler)
        {
            this.modalInputStack.Push(inputHandler);
        }

        /// <summary>
        /// Remove the last game object from the modal input stack.
        /// </summary>
        public void PopModalInputHandler()
        {
            if (this.modalInputStack.Count > 0)
            {
                this.modalInputStack.Pop();

            }
        }

        /// <summary>
        /// Clear all modal input handlers off the stack.
        /// </summary>
        public void ClearModalInputStack()
        {
            this.modalInputStack.Clear();
        }

        #endregion Modal Input Options

        #region Fallback Input Handler Options

        /// <summary>
        /// Push a game object into the fallback input stack. Any input handlers on
        /// the game object are given input events when no modal or focused objects consume the event.
        /// </summary>
        /// <param name="inputHandler">The input handler to push.</param>
        public void PushFallbackInputHandler(Entity inputHandler)
        {
            this.fallbackInputStack.Push(inputHandler);
        }

        /// <summary>
        /// Remove the last game object from the fallback input stack.
        /// </summary>
        public void PopFallbackInputHandler()
        {
            this.fallbackInputStack.Pop();
        }

        /// <summary>
        /// Clear all fallback input handlers off the stack.
        /// </summary>
        public void ClearFallbackInputStack()
        {
            this.fallbackInputStack.Clear();
        }

        #endregion Fallback Input Handler Options

        #region Input Events

        #region Input Source Events

        /// <inheritdoc />
        public uint GenerateNewSourceId()
        {
            var newId = (uint)UnityEngine.Random.Range(1, int.MaxValue);

            foreach (var inputSource in this.DetectedInputSources)
            {
                if (inputSource.SourceId == newId)
                {
                    return this.GenerateNewSourceId();
                }
            }

            return newId;
        }

        /// <inheritdoc />
        public IMixedRealityInputSource RequestNewGenericInputSource(string name, IMixedRealityPointer[] pointers = null, InputSourceType sourceType = InputSourceType.Other)
        {
            return new BaseGenericInputSource(name, pointers, sourceType);
        }

        #region Input Source State Events

        /// <inheritdoc />
        public void RaiseSourceDetected(IMixedRealityInputSource source, IMixedRealityController controller = null)
        {
            // Create input event
            this.sourceStateEventData.Initialize(source, controller);

            if (this.DetectedInputSources.Contains(source))
            {
                Trace.TraceError($"{source.SourceName} has already been registered with the Input Manager!");
            }

            this.DetectedInputSources.Add(source);

            if (controller != null)
            {
                this.DetectedControllers.Add(controller);
            }

            this.FocusProvider?.OnSourceDetected(this.sourceStateEventData);

            // Pass handler through HandleEvent to perform modal/fallback logic
            HandleEvent(this.sourceStateEventData, OnSourceDetectedEventHandler);
        }

        private static readonly ExecuteEvents.EventFunction<IMixedRealitySourceStateHandler> OnSourceDetectedEventHandler =
            delegate (IMixedRealitySourceStateHandler handler, BaseEventData eventData)
            {
                var casted = ExecuteEvents.ValidateEventData<SourceStateEventData>(eventData);
                handler.OnSourceDetected(casted);
            };

        /// <inheritdoc />
        public void RaiseSourceLost(IMixedRealityInputSource source, IMixedRealityController controller = null)
        {
            // Create input event
            this.sourceStateEventData.Initialize(source, controller);

            if (!this.DetectedInputSources.Contains(source))
            {
                Trace.TraceError($"{source.SourceName} was never registered with the Input Manager!");
            }

            this.DetectedInputSources.Remove(source);

            if (controller != null)
            {
                this.DetectedControllers.Remove(controller);
            }

            // Pass handler through HandleEvent to perform modal/fallback logic
            // Events have to be handled before FocusProvider.OnSourceLost since they won't be passed on without a focused object
            HandleEvent(this.sourceStateEventData, OnSourceLostEventHandler);

            this.FocusProvider?.OnSourceLost(this.sourceStateEventData);
        }

        private static readonly ExecuteEvents.EventFunction<IMixedRealitySourceStateHandler> OnSourceLostEventHandler =
                delegate (IMixedRealitySourceStateHandler handler, BaseEventData eventData)
                {
                    var casted = ExecuteEvents.ValidateEventData<SourceStateEventData>(eventData);
                    handler.OnSourceLost(casted);
                };

        #endregion Input Source State Events

        #region Input Source Pose Events

        /// <inheritdoc />
        public void RaiseSourceTrackingStateChanged(IMixedRealityInputSource source, IMixedRealityController controller, TrackingState state)
        {
            // Create input event
            this.sourceTrackingEventData.Initialize(source, controller, state);

            // Pass handler through HandleEvent to perform modal/fallback logic
            HandleEvent(this.sourceTrackingEventData, OnSourceTrackingChangedEventHandler);
        }

        private static readonly ExecuteEvents.EventFunction<IMixedRealitySourcePoseHandler> OnSourceTrackingChangedEventHandler =
                delegate (IMixedRealitySourcePoseHandler handler, BaseEventData eventData)
                {
                    var casted = ExecuteEvents.ValidateEventData<SourcePoseEventData<TrackingState>>(eventData);
                    handler.OnSourcePoseChanged(casted);
                };

        /// <inheritdoc />
        public void RaiseSourcePositionChanged(IMixedRealityInputSource source, IMixedRealityController controller, Vector2 position)
        {
            // Create input event
            this.sourceVector2EventData.Initialize(source, controller, position);

            // Pass handler through HandleEvent to perform modal/fallback logic
            HandleEvent(this.sourceVector2EventData, OnSourcePoseVector2ChangedEventHandler);
        }

        private static readonly ExecuteEvents.EventFunction<IMixedRealitySourcePoseHandler> OnSourcePoseVector2ChangedEventHandler =
                delegate (IMixedRealitySourcePoseHandler handler, BaseEventData eventData)
                {
                    var casted = ExecuteEvents.ValidateEventData<SourcePoseEventData<Vector2>>(eventData);
                    handler.OnSourcePoseChanged(casted);
                };

        /// <inheritdoc />
        public void RaiseSourcePositionChanged(IMixedRealityInputSource source, IMixedRealityController controller, Vector3 position)
        {
            // Create input event
            this.sourcePositionEventData.Initialize(source, controller, position);

            // Pass handler through HandleEvent to perform modal/fallback logic
            HandleEvent(this.sourcePositionEventData, OnSourcePositionChangedEventHandler);
        }

        private static readonly ExecuteEvents.EventFunction<IMixedRealitySourcePoseHandler> OnSourcePositionChangedEventHandler =
                delegate (IMixedRealitySourcePoseHandler handler, BaseEventData eventData)
                {
                    var casted = ExecuteEvents.ValidateEventData<SourcePoseEventData<Vector3>>(eventData);
                    handler.OnSourcePoseChanged(casted);
                };

        /// <inheritdoc />
        public void RaiseSourceRotationChanged(IMixedRealityInputSource source, IMixedRealityController controller, Quaternion rotation)
        {
            // Create input event
            this.sourceRotationEventData.Initialize(source, controller, rotation);

            // Pass handler through HandleEvent to perform modal/fallback logic
            HandleEvent(this.sourceRotationEventData, OnSourceRotationChangedEventHandler);
        }

        private static readonly ExecuteEvents.EventFunction<IMixedRealitySourcePoseHandler> OnSourceRotationChangedEventHandler =
                delegate (IMixedRealitySourcePoseHandler handler, BaseEventData eventData)
                {
                    var casted = ExecuteEvents.ValidateEventData<SourcePoseEventData<Quaternion>>(eventData);
                    handler.OnSourcePoseChanged(casted);
                };

        /// <inheritdoc />
        public void RaiseSourcePoseChanged(IMixedRealityInputSource source, IMixedRealityController controller, MixedRealityPose position)
        {
            // Create input event
            this.sourcePoseEventData.Initialize(source, controller, position);

            // Pass handler through HandleEvent to perform modal/fallback logic
            HandleEvent(this.sourcePoseEventData, OnSourcePoseChangedEventHandler);
        }

        private static readonly ExecuteEvents.EventFunction<IMixedRealitySourcePoseHandler> OnSourcePoseChangedEventHandler =
                delegate (IMixedRealitySourcePoseHandler handler, BaseEventData eventData)
                {
                    var casted = ExecuteEvents.ValidateEventData<SourcePoseEventData<MixedRealityPose>>(eventData);
                    handler.OnSourcePoseChanged(casted);
                };

        #endregion Input Source Pose Events

        #endregion Input Source Events

        #region Focus Events

        /// <inheritdoc />
        public void RaisePreFocusChanged(IMixedRealityPointer pointer, Entity oldFocusedObject, Entity newFocusedObject)
        {
            this.focusEventData.Initialize(pointer, oldFocusedObject, newFocusedObject);

            this.HandleFocusChangedEvents(this.focusEventData, OnPreFocusChangedHandler);
        }

        private static readonly ExecuteEvents.EventFunction<IMixedRealityFocusChangedHandler> OnPreFocusChangedHandler =
                delegate (IMixedRealityFocusChangedHandler handler, BaseEventData eventData)
                {
                    var casted = ExecuteEvents.ValidateEventData<FocusEventData>(eventData);
                    handler.OnBeforeFocusChange(casted);
                };

        /// <inheritdoc />
        public void RaiseFocusChanged(IMixedRealityPointer pointer, Entity oldFocusedObject, Entity newFocusedObject)
        {
            this.focusEventData.Initialize(pointer, oldFocusedObject, newFocusedObject);

            this.HandleFocusChangedEvents(this.focusEventData, OnFocusChangedHandler);
        }

        private static readonly ExecuteEvents.EventFunction<IMixedRealityFocusChangedHandler> OnFocusChangedHandler =
            delegate (IMixedRealityFocusChangedHandler handler, BaseEventData eventData)
            {
                var casted = ExecuteEvents.ValidateEventData<FocusEventData>(eventData);
                handler.OnFocusChanged(casted);
            };

        /// <inheritdoc />
        public void RaiseFocusEnter(IMixedRealityPointer pointer, Entity focusedObject)
        {
            this.focusEventData.Initialize(pointer);

            this.HandleFocusEvent(focusedObject, this.focusEventData, OnFocusEnterEventHandler);
        }

        private static readonly ExecuteEvents.EventFunction<IMixedRealityFocusHandler> OnFocusEnterEventHandler =
                delegate (IMixedRealityFocusHandler handler, BaseEventData eventData)
                {
                    var casted = ExecuteEvents.ValidateEventData<FocusEventData>(eventData);
                    handler.OnFocusEnter(casted);
                };

        /// <inheritdoc />
        public void RaiseFocusExit(IMixedRealityPointer pointer, Entity unfocusedObject)
        {
            this.focusEventData.Initialize(pointer);

            this.HandleFocusEvent(unfocusedObject, this.focusEventData, OnFocusExitEventHandler);
        }

        private static readonly ExecuteEvents.EventFunction<IMixedRealityFocusHandler> OnFocusExitEventHandler =
                delegate (IMixedRealityFocusHandler handler, BaseEventData eventData)
                {
                    var casted = ExecuteEvents.ValidateEventData<FocusEventData>(eventData);
                    handler.OnFocusExit(casted);
                };

        #endregion Focus Events

        #region Pointers

        #region Pointer Down

        private static readonly ExecuteEvents.EventFunction<IMixedRealityPointerHandler> OnPointerDownEventHandler =
            delegate (IMixedRealityPointerHandler handler, BaseEventData eventData)
            {
                var casted = ExecuteEvents.ValidateEventData<MixedRealityPointerEventData>(eventData);
                handler.OnPointerDown(casted);
            };

        /// <inheritdoc />
        public void RaisePointerDown(IMixedRealityPointer pointer, MixedRealityInputAction inputAction, XRHandedness handedness = XRHandedness.Undefined, IMixedRealityInputSource inputSource = null)
        {
            // Only lock the object if there is a grabbable above in the hierarchy
            Transform currentObject = null;
            Entity currentEntity = pointer.Result?.Details.Object;
            if (currentEntity != null)
            {
                currentObject = currentEntity.transform;
            }

            IMixedRealityPointerHandler ancestorPointerHandler = null;
            while (currentObject != null && ancestorPointerHandler == null)
            {
                foreach (var component in currentObject.GetComponents<Component>())
                {
                    if (component is IMixedRealityPointerHandler)
                    {
                        ancestorPointerHandler = (IMixedRealityPointerHandler)component;
                        break;
                    }
                }

                currentObject = currentObject.transform.parent;
            }

            pointer.IsFocusLocked = ancestorPointerHandler != null;

            this.pointerEventData.Initialize(pointer, inputAction, handedness, inputSource);

            HandlePointerEvent(this.pointerEventData, OnPointerDownEventHandler);
        }

        #endregion Pointer Down

        #region Pointer Dragged

        private static readonly ExecuteEvents.EventFunction<IMixedRealityPointerHandler> OnPointerDraggedEventHandler =
            delegate (IMixedRealityPointerHandler handler, BaseEventData eventData)
            {
                var casted = ExecuteEvents.ValidateEventData<MixedRealityPointerEventData>(eventData);
                handler.OnPointerDragged(casted);
            };

        /// <inheritdoc />
        public void RaisePointerDragged(IMixedRealityPointer pointer, MixedRealityInputAction inputAction, XRHandedness handedness = XRHandedness.Undefined, IMixedRealityInputSource inputSource = null)
        {
            this.pointerEventData.Initialize(pointer, inputAction, handedness, inputSource);

            HandlePointerEvent(this.pointerEventData, OnPointerDraggedEventHandler);
        }

        #endregion Pointer Dragged

        #region Pointer Click

        private static readonly ExecuteEvents.EventFunction<IMixedRealityPointerHandler> OnInputClickedEventHandler =
                delegate (IMixedRealityPointerHandler handler, BaseEventData eventData)
                {
                    var casted = ExecuteEvents.ValidateEventData<MixedRealityPointerEventData>(eventData);
                    handler.OnPointerClicked(casted);
                };

        /// <inheritdoc />
        public void RaisePointerClicked(IMixedRealityPointer pointer, MixedRealityInputAction inputAction, int count, XRHandedness handedness = XRHandedness.Undefined, IMixedRealityInputSource inputSource = null)
        {
            // Create input event
            this.pointerEventData.Initialize(pointer, inputAction, handedness, inputSource, count);

            this.HandleClick();
        }

        private void HandleClick()
        {
            // Pass handler through HandleEvent to perform modal/fallback logic
            HandlePointerEvent(this.pointerEventData, OnInputClickedEventHandler);

            // NOTE: In Unity UI, a "click" happens on every pointer up, so we have RaisePointerUp call the PointerHandler.
        }

        #endregion Pointer Click

        #region Pointer Up

        private static readonly ExecuteEvents.EventFunction<IMixedRealityPointerHandler> OnPointerUpEventHandler =
            delegate (IMixedRealityPointerHandler handler, BaseEventData eventData)
            {
                var casted = ExecuteEvents.ValidateEventData<MixedRealityPointerEventData>(eventData);
                handler.OnPointerUp(casted);
            };

        /// <inheritdoc />
        public void RaisePointerUp(IMixedRealityPointer pointer, MixedRealityInputAction inputAction, XRHandedness handedness = XRHandedness.Undefined, IMixedRealityInputSource inputSource = null)
        {
            this.pointerEventData.Initialize(pointer, inputAction, handedness, inputSource);

            HandlePointerEvent(this.pointerEventData, OnPointerUpEventHandler);

            pointer.IsFocusLocked = false;
        }

        #endregion Pointer Up

        #endregion Pointers

        #region Generic Input Events

        #region Input Down

        private static readonly ExecuteEvents.EventFunction<IMixedRealityInputHandler> OnInputDownEventHandler =
            delegate (IMixedRealityInputHandler handler, BaseEventData eventData)
            {
                var casted = ExecuteEvents.ValidateEventData<InputEventData>(eventData);
                handler.OnInputDown(casted);
            };

        private static readonly ExecuteEvents.EventFunction<IMixedRealityBaseInputHandler> OnInputDownWithActionEventHandler =
            delegate (IMixedRealityBaseInputHandler handler, BaseEventData eventData)
            {
                var inputData = ExecuteEvents.ValidateEventData<InputEventData>(eventData);
                Debug.Assert(inputData.MixedRealityInputAction != MixedRealityInputAction.None);

                var inputHandler = handler as IMixedRealityInputHandler;
                if (inputHandler != null)
                {
                    inputHandler.OnInputDown(inputData);
                }

                var actionHandler = handler as IMixedRealityInputActionHandler;
                if (actionHandler != null)
                {
                    actionHandler.OnActionStarted(inputData);
                }
            };

        /// <inheritdoc />
        public void RaiseOnInputDown(IMixedRealityInputSource source, XRHandedness handedness, MixedRealityInputAction inputAction)
        {
            inputAction = this.ProcessRules(inputAction, true);

            // Create input event
            this.inputEventData.Initialize(source, handedness, inputAction);

            // Pass handler through HandleEvent to perform modal/fallback logic
            if (this.inputEventData.MixedRealityInputAction == MixedRealityInputAction.None)
            {
                HandleEvent(this.inputEventData, OnInputDownEventHandler);
            }
            else
            {
                HandleEvent(this.inputEventData, OnInputDownWithActionEventHandler);
            }
        }

        #endregion Input Down

        #region Input Up

        private static readonly ExecuteEvents.EventFunction<IMixedRealityInputHandler> OnInputUpEventHandler =
            delegate (IMixedRealityInputHandler handler, BaseEventData eventData)
            {
                var casted = ExecuteEvents.ValidateEventData<InputEventData>(eventData);
                handler.OnInputUp(casted);
            };

        private static readonly ExecuteEvents.EventFunction<IMixedRealityBaseInputHandler> OnInputUpWithActionEventHandler =
            delegate (IMixedRealityBaseInputHandler handler, BaseEventData eventData)
            {
                var inputData = ExecuteEvents.ValidateEventData<InputEventData>(eventData);
                Debug.Assert(inputData.MixedRealityInputAction != MixedRealityInputAction.None);

                var inputHandler = handler as IMixedRealityInputHandler;
                if (inputHandler != null)
                {
                    inputHandler.OnInputUp(inputData);
                }

                var actionHandler = handler as IMixedRealityInputActionHandler;
                if (actionHandler != null)
                {
                    actionHandler.OnActionEnded(inputData);
                }
            };

        /// <inheritdoc />
        public void RaiseOnInputUp(IMixedRealityInputSource source, XRHandedness handedness, MixedRealityInputAction inputAction)
        {
            inputAction = this.ProcessRules(inputAction, false);

            // Create input event
            this.inputEventData.Initialize(source, handedness, inputAction);

            // Pass handler through HandleEvent to perform modal/fallback logic
            if (this.inputEventData.MixedRealityInputAction == MixedRealityInputAction.None)
            {
                HandleEvent(this.inputEventData, OnInputUpEventHandler);
            }
            else
            {
                HandleEvent(this.inputEventData, OnInputUpWithActionEventHandler);
            }
        }

        #endregion Input Up

        #region Float Input Changed

        private static readonly ExecuteEvents.EventFunction<IMixedRealityInputHandler<float>> OnFloatInputChanged =
            delegate (IMixedRealityInputHandler<float> handler, BaseEventData eventData)
            {
                var casted = ExecuteEvents.ValidateEventData<InputEventData<float>>(eventData);
                handler.OnInputChanged(casted);
            };

        /// <inheritdoc />
        public void RaiseFloatInputChanged(IMixedRealityInputSource source, XRHandedness handedness, MixedRealityInputAction inputAction, float inputValue)
        {
            inputAction = this.ProcessRules(inputAction, inputValue);

            // Create input event
            this.floatInputEventData.Initialize(source, handedness, inputAction, inputValue);

            // Pass handler through HandleEvent to perform modal/fallback logic
            HandleEvent(this.floatInputEventData, OnFloatInputChanged);
        }

        #endregion Float Input Changed

        #region Input Position Changed

        private static readonly ExecuteEvents.EventFunction<IMixedRealityInputHandler<Vector2>> OnTwoDoFInputChanged =
            delegate (IMixedRealityInputHandler<Vector2> handler, BaseEventData eventData)
            {
                var casted = ExecuteEvents.ValidateEventData<InputEventData<Vector2>>(eventData);
                handler.OnInputChanged(casted);
            };

        /// <inheritdoc />
        public void RaisePositionInputChanged(IMixedRealityInputSource source, XRHandedness handedness, MixedRealityInputAction inputAction, Vector2 inputPosition)
        {
            inputAction = this.ProcessRules(inputAction, inputPosition);

            // Create input event
            this.vector2InputEventData.Initialize(source, handedness, inputAction, inputPosition);

            // Pass handler through HandleEvent to perform modal/fallback logic
            HandleEvent(this.vector2InputEventData, OnTwoDoFInputChanged);
        }

        private static readonly ExecuteEvents.EventFunction<IMixedRealityInputHandler<Vector3>> OnPositionInputChanged =
            delegate (IMixedRealityInputHandler<Vector3> handler, BaseEventData eventData)
            {
                var casted = ExecuteEvents.ValidateEventData<InputEventData<Vector3>>(eventData);
                handler.OnInputChanged(casted);
            };

        /// <inheritdoc />
        public void RaisePositionInputChanged(IMixedRealityInputSource source, XRHandedness handedness, MixedRealityInputAction inputAction, Vector3 position)
        {
            inputAction = this.ProcessRules(inputAction, position);

            // Create input event
            this.positionInputEventData.Initialize(source, handedness, inputAction, position);

            // Pass handler through HandleEvent to perform modal/fallback logic
            HandleEvent(this.positionInputEventData, OnPositionInputChanged);
        }

        #endregion Input Position Changed

        #region Input Rotation Changed

        private static readonly ExecuteEvents.EventFunction<IMixedRealityInputHandler<Quaternion>> OnRotationInputChanged =
            delegate (IMixedRealityInputHandler<Quaternion> handler, BaseEventData eventData)
            {
                var casted = ExecuteEvents.ValidateEventData<InputEventData<Quaternion>>(eventData);
                handler.OnInputChanged(casted);
            };

        /// <inheritdoc />
        public void RaiseRotationInputChanged(IMixedRealityInputSource source, XRHandedness handedness, MixedRealityInputAction inputAction, Quaternion rotation)
        {
            inputAction = this.ProcessRules(inputAction, rotation);

            // Create input event
            this.rotationInputEventData.Initialize(source, handedness, inputAction, rotation);

            // Pass handler through HandleEvent to perform modal/fallback logic
            HandleEvent(this.positionInputEventData, OnRotationInputChanged);
        }

        #endregion Input Rotation Changed

        #region Input Pose Changed

        private static readonly ExecuteEvents.EventFunction<IMixedRealityInputHandler<MixedRealityPose>> OnPoseInputChanged =
            delegate (IMixedRealityInputHandler<MixedRealityPose> handler, BaseEventData eventData)
            {
                var casted = ExecuteEvents.ValidateEventData<InputEventData<MixedRealityPose>>(eventData);
                handler.OnInputChanged(casted);
            };

        /// <inheritdoc />
        public void RaisePoseInputChanged(IMixedRealityInputSource source, XRHandedness handedness, MixedRealityInputAction inputAction, MixedRealityPose inputData)
        {
            inputAction = this.ProcessRules(inputAction, inputData);

            // Create input event
            this.poseInputEventData.Initialize(source, handedness, inputAction, inputData);

            // Pass handler through HandleEvent to perform modal/fallback logic
            HandleEvent(this.poseInputEventData, OnPoseInputChanged);
        }

        #endregion Input Pose Changed

        #endregion Generic Input Events

        #region Gesture Events

        private static readonly ExecuteEvents.EventFunction<IMixedRealityGestureHandler> OnGestureStarted =
            delegate (IMixedRealityGestureHandler handler, BaseEventData eventData)
            {
                var casted = ExecuteEvents.ValidateEventData<InputEventData>(eventData);
                handler.OnGestureStarted(casted);
            };

        private static readonly ExecuteEvents.EventFunction<IMixedRealityBaseInputHandler> OnGestureStartedWithAction =
            delegate (IMixedRealityBaseInputHandler handler, BaseEventData eventData)
            {
                var inputData = ExecuteEvents.ValidateEventData<InputEventData>(eventData);
                Debug.Assert(inputData.MixedRealityInputAction != MixedRealityInputAction.None);

                var gestureHandler = handler as IMixedRealityGestureHandler;
                if (gestureHandler != null)
                {
                    gestureHandler.OnGestureStarted(inputData);
                }

                var actionHandler = handler as IMixedRealityInputActionHandler;
                if (actionHandler != null)
                {
                    actionHandler.OnActionStarted(inputData);
                }
            };

        /// <inheritdoc />
        public void RaiseGestureStarted(IMixedRealityController controller, MixedRealityInputAction action)
        {
            action = this.ProcessRules(action, true);
            this.inputEventData.Initialize(controller.InputSource, controller.ControllerHandedness, action);

            if (action == MixedRealityInputAction.None)
            {
                HandleEvent(this.inputEventData, OnGestureStarted);
            }
            else
            {
                HandleEvent(this.inputEventData, OnGestureStartedWithAction);
            }
        }

        private static readonly ExecuteEvents.EventFunction<IMixedRealityGestureHandler> OnGestureUpdated =
            delegate (IMixedRealityGestureHandler handler, BaseEventData eventData)
            {
                var casted = ExecuteEvents.ValidateEventData<InputEventData>(eventData);
                handler.OnGestureUpdated(casted);
            };

        /// <inheritdoc />
        public void RaiseGestureUpdated(IMixedRealityController controller, MixedRealityInputAction action)
        {
            action = this.ProcessRules(action, true);
            this.inputEventData.Initialize(controller.InputSource, controller.ControllerHandedness, action);
            HandleEvent(this.inputEventData, OnGestureUpdated);
        }

        private static readonly ExecuteEvents.EventFunction<IMixedRealityGestureHandler<Vector2>> OnGestureVector2PositionUpdated =
                delegate (IMixedRealityGestureHandler<Vector2> handler, BaseEventData eventData)
                {
                    var casted = ExecuteEvents.ValidateEventData<InputEventData<Vector2>>(eventData);
                    handler.OnGestureUpdated(casted);
                };

        /// <inheritdoc />
        public void RaiseGestureUpdated(IMixedRealityController controller, MixedRealityInputAction action, Vector2 inputData)
        {
            action = this.ProcessRules(action, inputData);
            this.vector2InputEventData.Initialize(controller.InputSource, controller.ControllerHandedness, action, inputData);
            HandleEvent(this.vector2InputEventData, OnGestureVector2PositionUpdated);
        }

        private static readonly ExecuteEvents.EventFunction<IMixedRealityGestureHandler<Vector3>> OnGesturePositionUpdated =
            delegate (IMixedRealityGestureHandler<Vector3> handler, BaseEventData eventData)
            {
                var casted = ExecuteEvents.ValidateEventData<InputEventData<Vector3>>(eventData);
                handler.OnGestureUpdated(casted);
            };

        /// <inheritdoc />
        public void RaiseGestureUpdated(IMixedRealityController controller, MixedRealityInputAction action, Vector3 inputData)
        {
            action = this.ProcessRules(action, inputData);
            this.positionInputEventData.Initialize(controller.InputSource, controller.ControllerHandedness, action, inputData);
            HandleEvent(this.positionInputEventData, OnGesturePositionUpdated);
        }

        private static readonly ExecuteEvents.EventFunction<IMixedRealityGestureHandler<Quaternion>> OnGestureRotationUpdated =
            delegate (IMixedRealityGestureHandler<Quaternion> handler, BaseEventData eventData)
            {
                var casted = ExecuteEvents.ValidateEventData<InputEventData<Quaternion>>(eventData);
                handler.OnGestureUpdated(casted);
            };

        /// <inheritdoc />
        public void RaiseGestureUpdated(IMixedRealityController controller, MixedRealityInputAction action, Quaternion inputData)
        {
            action = this.ProcessRules(action, inputData);
            this.rotationInputEventData.Initialize(controller.InputSource, controller.ControllerHandedness, action, inputData);
            HandleEvent(this.rotationInputEventData, OnGestureRotationUpdated);
        }

        private static readonly ExecuteEvents.EventFunction<IMixedRealityGestureHandler<MixedRealityPose>> OnGesturePoseUpdated =
            delegate (IMixedRealityGestureHandler<MixedRealityPose> handler, BaseEventData eventData)
            {
                var casted = ExecuteEvents.ValidateEventData<InputEventData<MixedRealityPose>>(eventData);
                handler.OnGestureUpdated(casted);
            };

        /// <inheritdoc />
        public void RaiseGestureUpdated(IMixedRealityController controller, MixedRealityInputAction action, MixedRealityPose inputData)
        {
            action = this.ProcessRules(action, inputData);
            this.poseInputEventData.Initialize(controller.InputSource, controller.ControllerHandedness, action, inputData);
            HandleEvent(this.poseInputEventData, OnGesturePoseUpdated);
        }

        private static readonly ExecuteEvents.EventFunction<IMixedRealityGestureHandler> OnGestureCompleted =
            delegate (IMixedRealityGestureHandler handler, BaseEventData eventData)
            {
                var casted = ExecuteEvents.ValidateEventData<InputEventData>(eventData);
                handler.OnGestureCompleted(casted);
            };

        private static readonly ExecuteEvents.EventFunction<IMixedRealityBaseInputHandler> OnGestureCompletedWithAction =
            delegate (IMixedRealityBaseInputHandler handler, BaseEventData eventData)
            {
                var inputData = ExecuteEvents.ValidateEventData<InputEventData>(eventData);
                Debug.Assert(inputData.MixedRealityInputAction != MixedRealityInputAction.None);

                var gestureHandler = handler as IMixedRealityGestureHandler;
                if (gestureHandler != null)
                {
                    gestureHandler.OnGestureCompleted(inputData);
                }

                var actionHandler = handler as IMixedRealityInputActionHandler;
                if (actionHandler != null)
                {
                    actionHandler.OnActionEnded(inputData);
                }
            };

        /// <inheritdoc />
        public void RaiseGestureCompleted(IMixedRealityController controller, MixedRealityInputAction action)
        {
            action = this.ProcessRules(action, false);
            this.inputEventData.Initialize(controller.InputSource, controller.ControllerHandedness, action);

            if (action == MixedRealityInputAction.None)
            {
                HandleEvent(this.inputEventData, OnGestureCompleted);
            }
            else
            {
                HandleEvent(this.inputEventData, OnGestureCompletedWithAction);
            }
        }

        private static readonly ExecuteEvents.EventFunction<IMixedRealityGestureHandler<Vector2>> OnGestureVector2PositionCompleted =
                delegate (IMixedRealityGestureHandler<Vector2> handler, BaseEventData eventData)
                {
                    var casted = ExecuteEvents.ValidateEventData<InputEventData<Vector2>>(eventData);
                    handler.OnGestureCompleted(casted);
                };

        /// <inheritdoc />
        public void RaiseGestureCompleted(IMixedRealityController controller, MixedRealityInputAction action, Vector2 inputData)
        {
            action = this.ProcessRules(action, inputData);
            this.vector2InputEventData.Initialize(controller.InputSource, controller.ControllerHandedness, action, inputData);
            HandleEvent(this.vector2InputEventData, OnGestureVector2PositionCompleted);
        }

        private static readonly ExecuteEvents.EventFunction<IMixedRealityGestureHandler<Vector3>> OnGesturePositionCompleted =
            delegate (IMixedRealityGestureHandler<Vector3> handler, BaseEventData eventData)
            {
                var casted = ExecuteEvents.ValidateEventData<InputEventData<Vector3>>(eventData);
                handler.OnGestureCompleted(casted);
            };

        /// <inheritdoc />
        public void RaiseGestureCompleted(IMixedRealityController controller, MixedRealityInputAction action, Vector3 inputData)
        {
            action = this.ProcessRules(action, inputData);
            this.positionInputEventData.Initialize(controller.InputSource, controller.ControllerHandedness, action, inputData);
            HandleEvent(this.positionInputEventData, OnGesturePositionCompleted);
        }

        private static readonly ExecuteEvents.EventFunction<IMixedRealityGestureHandler<Quaternion>> OnGestureRotationCompleted =
            delegate (IMixedRealityGestureHandler<Quaternion> handler, BaseEventData eventData)
            {
                var casted = ExecuteEvents.ValidateEventData<InputEventData<Quaternion>>(eventData);
                handler.OnGestureCompleted(casted);
            };

        /// <inheritdoc />
        public void RaiseGestureCompleted(IMixedRealityController controller, MixedRealityInputAction action, Quaternion inputData)
        {
            action = this.ProcessRules(action, inputData);
            this.rotationInputEventData.Initialize(controller.InputSource, controller.ControllerHandedness, action, inputData);
            HandleEvent(this.rotationInputEventData, OnGestureRotationCompleted);
        }

        private static readonly ExecuteEvents.EventFunction<IMixedRealityGestureHandler<MixedRealityPose>> OnGesturePoseCompleted =
            delegate (IMixedRealityGestureHandler<MixedRealityPose> handler, BaseEventData eventData)
            {
                var casted = ExecuteEvents.ValidateEventData<InputEventData<MixedRealityPose>>(eventData);
                handler.OnGestureCompleted(casted);
            };

        /// <inheritdoc />
        public void RaiseGestureCompleted(IMixedRealityController controller, MixedRealityInputAction action, MixedRealityPose inputData)
        {
            action = this.ProcessRules(action, inputData);
            this.poseInputEventData.Initialize(controller.InputSource, controller.ControllerHandedness, action, inputData);
            HandleEvent(this.poseInputEventData, OnGesturePoseCompleted);
        }

        private static readonly ExecuteEvents.EventFunction<IMixedRealityGestureHandler> OnGestureCanceled =
                delegate (IMixedRealityGestureHandler handler, BaseEventData eventData)
                {
                    var casted = ExecuteEvents.ValidateEventData<InputEventData>(eventData);
                    handler.OnGestureCanceled(casted);
                };

        /// <inheritdoc />
        public void RaiseGestureCanceled(IMixedRealityController controller, MixedRealityInputAction action)
        {
            action = this.ProcessRules(action, false);
            this.inputEventData.Initialize(controller.InputSource, controller.ControllerHandedness, action);
            HandleEvent(this.inputEventData, OnGestureCanceled);
        }

        #endregion Gesture Events

        #region Speech Keyword Events

        private static readonly ExecuteEvents.EventFunction<IMixedRealitySpeechHandler> OnSpeechKeywordRecognizedEventHandler =
            delegate (IMixedRealitySpeechHandler handler, BaseEventData eventData)
            {
                var casted = ExecuteEvents.ValidateEventData<SpeechEventData>(eventData);
                handler.OnSpeechKeywordRecognized(casted);
            };

        private static readonly ExecuteEvents.EventFunction<IMixedRealityBaseInputHandler> OnSpeechKeywordRecognizedWithActionEventHandler =
            delegate (IMixedRealityBaseInputHandler handler, BaseEventData eventData)
            {
                var speechData = ExecuteEvents.ValidateEventData<SpeechEventData>(eventData);
                Debug.Assert(speechData.MixedRealityInputAction != MixedRealityInputAction.None);

                var speechHandler = handler as IMixedRealitySpeechHandler;
                if (speechHandler != null)
                {
                    speechHandler.OnSpeechKeywordRecognized(speechData);
                }

                var actionHandler = handler as IMixedRealityInputActionHandler;
                if (actionHandler != null)
                {
                    actionHandler.OnActionStarted(speechData);
                    actionHandler.OnActionEnded(speechData);
                }
            };

        /// <inheritdoc />
        public void RaiseSpeechCommandRecognized(IMixedRealityInputSource source, RecognitionConfidenceLevel confidence, TimeSpan phraseDuration, DateTime phraseStartTime, SpeechCommands command)
        {
            // Create input event
            this.speechEventData.Initialize(source, confidence, phraseDuration, phraseStartTime, command);

            this.FocusProvider?.OnSpeechKeywordRecognized(this.speechEventData);

            // Pass handler through HandleEvent to perform modal/fallback logic
            if (command.Action == MixedRealityInputAction.None)
            {
                HandleEvent(this.speechEventData, OnSpeechKeywordRecognizedEventHandler);
            }
            else
            {
                HandleEvent(this.speechEventData, OnSpeechKeywordRecognizedWithActionEventHandler);
            }
        }

        #endregion Speech Keyword Events

        #region Dictation Events

        private static readonly ExecuteEvents.EventFunction<IMixedRealityDictationHandler> OnDictationHypothesisEventHandler =
            delegate (IMixedRealityDictationHandler handler, BaseEventData eventData)
            {
                var casted = ExecuteEvents.ValidateEventData<DictationEventData>(eventData);
                handler.OnDictationHypothesis(casted);
            };

        /// <inheritdoc />
        public void RaiseDictationHypothesis(IMixedRealityInputSource source, string dictationHypothesis, AudioClip dictationAudioClip = null)
        {
            // Create input event
            this.dictationEventData.Initialize(source, dictationHypothesis, dictationAudioClip);

            // Pass handler through HandleEvent to perform modal/fallback logic
            HandleEvent(this.dictationEventData, OnDictationHypothesisEventHandler);
        }

        private static readonly ExecuteEvents.EventFunction<IMixedRealityDictationHandler> OnDictationResultEventHandler =
            delegate (IMixedRealityDictationHandler handler, BaseEventData eventData)
            {
                var casted = ExecuteEvents.ValidateEventData<DictationEventData>(eventData);
                handler.OnDictationResult(casted);
            };

        /// <inheritdoc />
        public void RaiseDictationResult(IMixedRealityInputSource source, string dictationResult, AudioClip dictationAudioClip = null)
        {
            // Create input event
            this.dictationEventData.Initialize(source, dictationResult, dictationAudioClip);

            // Pass handler through HandleEvent to perform modal/fallback logic
            HandleEvent(this.dictationEventData, OnDictationResultEventHandler);
        }

        private static readonly ExecuteEvents.EventFunction<IMixedRealityDictationHandler> OnDictationCompleteEventHandler =
            delegate (IMixedRealityDictationHandler handler, BaseEventData eventData)
            {
                var casted = ExecuteEvents.ValidateEventData<DictationEventData>(eventData);
                handler.OnDictationComplete(casted);
            };

        /// <inheritdoc />
        public void RaiseDictationComplete(IMixedRealityInputSource source, string dictationResult, AudioClip dictationAudioClip)
        {
            // Create input event
            this.dictationEventData.Initialize(source, dictationResult, dictationAudioClip);

            // Pass handler through HandleEvent to perform modal/fallback logic
            HandleEvent(this.dictationEventData, OnDictationCompleteEventHandler);
        }

        private static readonly ExecuteEvents.EventFunction<IMixedRealityDictationHandler> OnDictationErrorEventHandler =
            delegate (IMixedRealityDictationHandler handler, BaseEventData eventData)
            {
                var casted = ExecuteEvents.ValidateEventData<DictationEventData>(eventData);
                handler.OnDictationError(casted);
            };

        /// <inheritdoc />
        public void RaiseDictationError(IMixedRealityInputSource source, string dictationResult, AudioClip dictationAudioClip = null)
        {
            // Create input event
            this.dictationEventData.Initialize(source, dictationResult, dictationAudioClip);

            // Pass handler through HandleEvent to perform modal/fallback logic
            HandleEvent(this.dictationEventData, OnDictationErrorEventHandler);
        }

        #endregion Dictation Events

        #region Hand Events

        private static readonly ExecuteEvents.EventFunction<IMixedRealityHandJointHandler> OnHandJointsUpdatedEventHandler =
            delegate (IMixedRealityHandJointHandler handler, BaseEventData eventData)
            {
                var casted = ExecuteEvents.ValidateEventData<InputEventData<IDictionary<TrackedHandJoint, MixedRealityPose>>>(eventData);

                handler.OnHandJointsUpdated(casted);
            };

        public void RaiseHandJointsUpdated(IMixedRealityInputSource source, XRHandedness handedness, IDictionary<TrackedHandJoint, MixedRealityPose> jointPoses)
        {
            // Create input event
            this.jointPoseInputEventData.Initialize(source, handedness, MixedRealityInputAction.None, jointPoses);

            // Pass handler through HandleEvent to perform modal/fallback logic
            HandleEvent(this.jointPoseInputEventData, OnHandJointsUpdatedEventHandler);
        }

        private static readonly ExecuteEvents.EventFunction<IMixedRealityHandMeshHandler> OnHandMeshUpdatedEventHandler =
            delegate (IMixedRealityHandMeshHandler handler, BaseEventData eventData)
            {
                var casted = ExecuteEvents.ValidateEventData<InputEventData<HandMeshInfo>>(eventData);

                handler.OnHandMeshUpdated(casted);
            };

        public void RaiseHandMeshUpdated(IMixedRealityInputSource source, XRHandedness handedness, HandMeshInfo handMeshInfo)
        {
            // Create input event
            this.handMeshInputEventData.Initialize(source, handedness, MixedRealityInputAction.None, handMeshInfo);

            // Pass handler through HandleEvent to perform modal/fallback logic
            HandleEvent(this.handMeshInputEventData, OnHandMeshUpdatedEventHandler);
        }

        private static readonly ExecuteEvents.EventFunction<IMixedRealityTouchHandler> OnTouchStartedEventHandler =
            delegate (IMixedRealityTouchHandler handler, BaseEventData eventData)
            {
                var casted = ExecuteEvents.ValidateEventData<HandTrackingInputEventData>(eventData);
                handler.OnTouchStarted(casted);
            };

        /// <inheritdoc />
        public void RaiseOnTouchStarted(IMixedRealityInputSource source, IMixedRealityController controller, XRHandedness handedness, Vector3 touchPoint)
        {
            // Create input event
            this.handTrackingInputEventData.Initialize(source, controller, handedness, touchPoint);

            // Pass handler through HandleEvent to perform modal/fallback logic
            HandleEvent(this.handTrackingInputEventData, OnTouchStartedEventHandler);
        }

        private static readonly ExecuteEvents.EventFunction<IMixedRealityTouchHandler> OnTouchCompletedEventHandler =
            delegate (IMixedRealityTouchHandler handler, BaseEventData eventData)
            {
                var casted = ExecuteEvents.ValidateEventData<HandTrackingInputEventData>(eventData);
                handler.OnTouchCompleted(casted);
            };

        /// <inheritdoc />
        public void RaiseOnTouchCompleted(IMixedRealityInputSource source, IMixedRealityController controller, XRHandedness handedness, Vector3 touchPoint)
        {
            // Create input event
            this.handTrackingInputEventData.Initialize(source, controller, handedness, touchPoint);

            // Pass handler through HandleEvent to perform modal/fallback logic
            HandleEvent(this.handTrackingInputEventData, OnTouchCompletedEventHandler);
        }

        private static readonly ExecuteEvents.EventFunction<IMixedRealityTouchHandler> OnTouchUpdatedEventHandler =
            delegate (IMixedRealityTouchHandler handler, BaseEventData eventData)
            {
                var casted = ExecuteEvents.ValidateEventData<HandTrackingInputEventData>(eventData);
                handler.OnTouchUpdated(casted);
            };

        /// <inheritdoc />
        public void RaiseOnTouchUpdated(IMixedRealityInputSource source, IMixedRealityController controller, XRHandedness handedness, Vector3 touchPoint)
        {
            // Create input event
            this.handTrackingInputEventData.Initialize(source, controller, handedness, touchPoint);

            // Pass handler through HandleEvent to perform modal/fallback logic
            HandleEvent(this.handTrackingInputEventData, OnTouchUpdatedEventHandler);
        }

        #endregion Hand Events

        #endregion Input Events

        #region Rules

        private static MixedRealityInputAction ProcessRules_Internal<T1, T2>(MixedRealityInputAction inputAction, T1[] inputActionRules, T2 criteria) where T1 : struct, IInputActionRule<T2>
        {
            for (int i = 0; i < inputActionRules.Length; i++)
            {
                if (inputActionRules[i].BaseAction == inputAction && inputActionRules[i].Criteria.Equals(criteria))
                {
                    if (inputActionRules[i].RuleAction == inputAction)
                    {
                        Trace.TraceError("Input Action Rule cannot be the same as the rule's Base Action!");
                        return inputAction;
                    }

                    if (inputActionRules[i].BaseAction.AxisConstraint != inputActionRules[i].RuleAction.AxisConstraint)
                    {
                        Trace.TraceError("Input Action Rule doesn't have the same Axis Constraint as the Base Action!");
                        return inputAction;
                    }

                    return inputActionRules[i].RuleAction;
                }
            }

            return inputAction;
        }

        private MixedRealityInputAction ProcessRules(MixedRealityInputAction inputAction, bool criteria)
        {
            if (this.CurrentInputActionRulesProfile != null && this.CurrentInputActionRulesProfile.InputActionRulesDigital?.Length > 0)
            {
                return ProcessRules_Internal(inputAction, this.CurrentInputActionRulesProfile.InputActionRulesDigital, criteria);
            }

            return inputAction;
        }

        private MixedRealityInputAction ProcessRules(MixedRealityInputAction inputAction, float criteria)
        {
            if (this.CurrentInputActionRulesProfile != null && this.CurrentInputActionRulesProfile.InputActionRulesSingleAxis?.Length > 0)
            {
                return ProcessRules_Internal(inputAction, this.CurrentInputActionRulesProfile.InputActionRulesSingleAxis, criteria);
            }

            return inputAction;
        }

        private MixedRealityInputAction ProcessRules(MixedRealityInputAction inputAction, Vector2 criteria)
        {
            if (this.CurrentInputActionRulesProfile != null && this.CurrentInputActionRulesProfile.InputActionRulesDualAxis?.Length > 0)
            {
                return ProcessRules_Internal(inputAction, this.CurrentInputActionRulesProfile.InputActionRulesDualAxis, criteria);
            }

            return inputAction;
        }

        private MixedRealityInputAction ProcessRules(MixedRealityInputAction inputAction, Vector3 criteria)
        {
            if (this.CurrentInputActionRulesProfile != null && this.CurrentInputActionRulesProfile.InputActionRulesVectorAxis?.Length > 0)
            {
                return ProcessRules_Internal(inputAction, this.CurrentInputActionRulesProfile.InputActionRulesVectorAxis, criteria);
            }

            return inputAction;
        }

        private MixedRealityInputAction ProcessRules(MixedRealityInputAction inputAction, Quaternion criteria)
        {
            if (this.CurrentInputActionRulesProfile != null && this.CurrentInputActionRulesProfile.InputActionRulesQuaternionAxis?.Length > 0)
            {
                return ProcessRules_Internal(inputAction, this.CurrentInputActionRulesProfile.InputActionRulesQuaternionAxis, criteria);
            }

            return inputAction;
        }

        private MixedRealityInputAction ProcessRules(MixedRealityInputAction inputAction, XRPose criteria)
        {
            if (this.CurrentInputActionRulesProfile != null && this.CurrentInputActionRulesProfile.InputActionRulesPoseAxis?.Length > 0)
            {
                return ProcessRules_Internal(inputAction, this.CurrentInputActionRulesProfile.InputActionRulesPoseAxis, criteria);
            }

            return inputAction;
        }

        #endregion Rules
    }
}
