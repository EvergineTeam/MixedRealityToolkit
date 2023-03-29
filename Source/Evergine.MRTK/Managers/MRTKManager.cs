// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using Evergine.Bullet;
using Evergine.Framework;
using Evergine.Framework.Managers;
using Evergine.Framework.Prefabs;
using Evergine.Framework.Services;
using Evergine.Framework.XR.TrackedDevices;
using Evergine.MRTK.InputSystem;
using Evergine.MRTK.InputSystem.Controllers;
using System.Linq;

namespace Evergine.MRTK.Managers
{
    /// <summary>
    /// This component encapsulates the main functionality of the MRTK.
    /// </summary>
    public partial class MRTKManager : SceneManager
    {
        [BindSceneManager]
        private AssetSceneManager assetSceneManager = null;

        [BindService(isRequired: false)]
        private XRPlatform xrPlatform = null;

        [BindSceneManager(isRequired: false)]
        private BulletPhysicManager3D bulletPhysicsManager3D = null;

        [BindSceneManager(isRequired: false)]
        private FocusProvider focusProvider = null;

        /// <inheritdoc/>
        protected override bool OnAttached()
        {
            if (!base.OnAttached())
            {
                return false;
            }

            // Set default values
            this.SetDefaultValues();

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

            ////////////////////////////////////////////////
            // Spawn controllers for detected platforms
            ////////////////////////////////////////////////

            // Evergine's XR platform, used for devices like a Meta Quest
            if (this.xrPlatform != null)
            {
                // Add MRTK Controllers for the physical controllers
                if (this.xrPlatform.InputTracking.GetDeviceByType(XRTrackedDeviceType.Controller) != null)
                {
                    this.HandleController(ControllerType.XRPhysicalController, ControllerHandedness.Left, this.LeftPhysicalControllerPrefab);
                    this.HandleController(ControllerType.XRPhysicalController, ControllerHandedness.Right, this.RightPhysicalControllerPrefab);
                }

                // Add MRTK Controllers for the hand tracking feature
                if (this.xrPlatform.InputTracking.GetDeviceByType(XRTrackedDeviceType.Hand) != null)
                {
                    this.HandleController(ControllerType.XRArticulatedHand, ControllerHandedness.Left, this.LeftArticulatedHandPrefab);
                    this.HandleController(ControllerType.XRArticulatedHand, ControllerHandedness.Right, this.RightArticulatedHandPrefab);
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
            // Instantiate the controller prefab
            var controllerPrefabInstance = prefab.Instantiate();

            // Get the pointers to add to this controller
            var pointerOptions = this.PointerOptions
                .Where(po => po.ControllerType == mrtkControllerType && (po.Handedness == ControllerHandedness.Any || po.Handedness == handedness));

            // Instantiate pointer prefabs and add them to the controller
            foreach (var pointerOption in pointerOptions)
            {
                var pointerPrefabInstance = pointerOption.Pointer.Instantiate();
                controllerPrefabInstance.AddChild(pointerPrefabInstance);
            }

            // Register the controller entity
            this.Managers.EntityManager.Add(controllerPrefabInstance);
        }
    }
}
