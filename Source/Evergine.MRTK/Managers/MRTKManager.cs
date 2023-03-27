// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using Evergine.Bullet;
using Evergine.Framework;
using Evergine.Framework.Managers;
using Evergine.Framework.Prefabs;
using Evergine.Framework.Services;
using Evergine.Framework.XR;
using Evergine.Framework.XR.TrackedDevices;
using Evergine.MRTK.InputSystem;
using Evergine.MRTK.Managers.Data;
using System.Collections.Generic;
using System.Linq;

namespace Evergine.MRTK.Managers
{
    /// <summary>
    /// This component encapsulates the main functionality of the MRTK.
    /// </summary>
    public partial class MRTKManager : SceneManager
    {
        [BindService(isRequired: false)]
        private XRPlatform xrPlatform = null;

        [BindSceneManager(isRequired: false)]
        private BulletPhysicManager3D bulletPhysicsManager3D = null;

        [BindSceneManager(isRequired: false)]
        private FocusProvider focusProvider = null;

        /// <inheritdoc/>
        protected override void OnLoaded()
        {
            base.OnLoaded();

            // Set default values
            this.LeftPhysicalControllerPrefab ??= this.Managers.AssetSceneManager.Load<Prefab>(MRTKResourceIDs.Prefabs.InputSystem.DefaultLeftPhysicalController_weprefab);
            this.RightPhysicalControllerPrefab ??= this.Managers.AssetSceneManager.Load<Prefab>(MRTKResourceIDs.Prefabs.InputSystem.DefaultRightPhysicalController_weprefab);
            this.LeftArticulatedHandPrefab ??= this.Managers.AssetSceneManager.Load<Prefab>(MRTKResourceIDs.Prefabs.InputSystem.DefaultLeftArticulatedHand_weprefab);
            this.RightArticulatedHandPrefab ??= this.Managers.AssetSceneManager.Load<Prefab>(MRTKResourceIDs.Prefabs.InputSystem.DefaultRightArticulatedHand_weprefab);

            this.PointerOptions ??= new List<PointerOption>(); // TODO make default pointer options list
        }

        /// <inheritdoc/>
        protected override bool OnAttached()
        {
            if (!base.OnAttached())
            {
                return false;
            }

            // Do nothing else if running on the editor
            if (Application.Current.IsEditor)
            {
                return true;
            }

            // Add the required helper managers
            this.AddManager(ref this.bulletPhysicsManager3D, new BulletPhysicManager3D()
            {
                PhysicWorldResolution = 10, // TODO check if this is needed after the change to kinematic
            });

            this.AddManager(ref this.focusProvider, new FocusProvider());

            // XR platform, like a Meta Quest device
            if (this.xrPlatform != null)
            {
                // Add MRTK Controllers for the physical controllers
                if (this.xrPlatform.InputTracking.GetDeviceByType(XRTrackedDeviceType.Controller) != null)
                {
                    this.HandleController(ControllerType.OpenXRPhysicalController, ControllerHandedness.Left, this.LeftPhysicalControllerPrefab);
                    this.HandleController(ControllerType.OpenXRPhysicalController, ControllerHandedness.Right, this.RightPhysicalControllerPrefab);
                }

                // Add MRTK Controllers for the hand tracking feature
                if (this.xrPlatform.InputTracking.GetDeviceByType(XRTrackedDeviceType.Hand) != null)
                {
                    this.HandleController(ControllerType.OpenXRArticulatedHand, ControllerHandedness.Left, this.LeftArticulatedHandPrefab);
                    this.HandleController(ControllerType.OpenXRArticulatedHand, ControllerHandedness.Right, this.RightArticulatedHandPrefab);
                }
            }

            return true;
        }

        private void AddManager<T>(ref T sceneManagerRef, T newSceneManager)
            where T : SceneManager
        {
            if (sceneManagerRef == null)
            {
                sceneManagerRef = newSceneManager;

                this.Managers.AddManager(newSceneManager);
            }
        }

        private void HandleController(ControllerType mrtkControllerType, ControllerHandedness handedness, Prefab prefab)
        {
            var controllerPrefabInstance = prefab.Instantiate();

            // Add pointers
            var pointerOptions = this.PointerOptions.Where(po => po.ControllerType == mrtkControllerType && (po.Handedness == ControllerHandedness.Any || po.Handedness == handedness));
            foreach (var pointerOption in pointerOptions)
            {
                var pointerPrefabInstance = pointerOption.Pointer.Instantiate();
                controllerPrefabInstance.AddChild(pointerPrefabInstance);
            }

            this.Managers.EntityManager.Add(controllerPrefabInstance);
        }
    }
}
