// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using System;
using System.Collections.Generic;
using System.Linq;
using Evergine.Components.Graphics3D;
using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.Framework.Physics3D;
using Evergine.Framework.Prefabs;
using Evergine.Mathematics;
using Evergine.MRTK.Base.EventDatum.Input;
using Evergine.MRTK.Base.Interfaces.InputSystem.Handlers;
using Evergine.MRTK.Emulation;
using Evergine.MRTK.Services.InputSystem;

namespace Evergine.MRTK.SDK.Features.UX.Components.AxisManipulationHandler
{
    /// <summary>
    /// A manipulation handler that restricts movement to a combination of axes.
    /// </summary>
    public partial class AxisManipulationHandler : Component, IMixedRealityPointerHandler, IMixedRealityTouchHandler
    {
        [BindComponent]
        private Transform3D transform = null;

        private Entity rigRootEntity;

        private Dictionary<Entity, AxisManipulationHelper> helpers;

        private Cursor currentCursor;
        private AxisManipulationHelper currentHandle;

        private Vector3 grabbedEntityPosition;
        private Vector3 grabbedCursorPosition;

        /// <inheritdoc/>
        protected override void OnActivated()
        {
            base.OnActivated();

            this.InternalCreateRig();
        }

        /// <inheritdoc/>
        protected override void OnDeactivated()
        {
            base.OnDeactivated();

            this.DestroyRig();
        }

        private bool CreateRig()
        {
            if (!this.IsActivated)
            {
                return false;
            }

            this.InternalCreateRig();
            return true;
        }

        private void InternalCreateRig()
        {
            if (this.Owner != null)
            {
                this.DestroyRig();
                this.InitializeRigRoot();
                this.InitializeDataStructures();
                this.AddHelpers();
            }
        }

        private void DestroyRig()
        {
            if (this.rigRootEntity != null)
            {
                this.Owner.RemoveChild(this.rigRootEntity);
                this.rigRootEntity = null;
            }
        }

        private void InitializeRigRoot()
        {
            this.rigRootEntity = new Entity($"{nameof(AxisManipulationHandler)}_RigRoot")
            {
                Flags = HideFlags.DontSave | HideFlags.DontShow,
            }
            .AddComponent(new Transform3D());

            this.Owner.AddChild(this.rigRootEntity);
        }

        private void InitializeDataStructures()
        {
            this.helpers = new Dictionary<Entity, AxisManipulationHelper>();
        }

        private void AddHelpers()
        {
            // Center
            this.CreateHandle(
                AxisManipulationHelperType.Center,
                AxisType.All,
                this.CenterHandlePrefab,
                this.CenterHandleMaterial,
                this.CenterHandleGrabbedMaterial,
                this.CenterHandleFocusedMaterial,
                Quaternion.Identity,
                new AxisManipulationHelper[0]);

            // X
            var handleX = this.CreateHandle(
                AxisManipulationHelperType.Axis,
                AxisType.X,
                this.AxisHandlePrefab,
                this.AxisHandleMaterial,
                this.AxisHandleGrabbedMaterial,
                this.AxisHandleFocusedMaterial,
                Quaternion.Identity,
                new AxisManipulationHelper[0]);

            // Y
            var handleY = this.CreateHandle(
                AxisManipulationHelperType.Axis,
                AxisType.Y,
                this.AxisHandlePrefab,
                this.AxisHandleMaterial,
                this.AxisHandleGrabbedMaterial,
                this.AxisHandleFocusedMaterial,
                Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathHelper.PiOver2),
                new AxisManipulationHelper[0]);

            // Z
            var handleZ = this.CreateHandle(
                AxisManipulationHelperType.Axis,
                AxisType.Z,
                this.AxisHandlePrefab,
                this.AxisHandleMaterial,
                this.AxisHandleGrabbedMaterial,
                this.AxisHandleFocusedMaterial,
                Quaternion.CreateFromAxisAngle(Vector3.UnitY, -MathHelper.PiOver2),
                new AxisManipulationHelper[0]);

            // XY
            var rotationAxis = Vector3.Normalize(Vector3.One);

            this.CreateHandle(
                AxisManipulationHelperType.Plane,
                AxisType.XY,
                this.PlaneHandlePrefab,
                this.PlaneHandleMaterial,
                this.PlaneHandleGrabbedMaterial,
                this.PlaneHandleFocusedMaterial,
                Quaternion.CreateFromAxisAngle(rotationAxis, MathHelper.ToRadians(120f)),
                new AxisManipulationHelper[] { handleX, handleY });

            // YZ
            this.CreateHandle(
                AxisManipulationHelperType.Plane,
                AxisType.YZ,
                this.PlaneHandlePrefab,
                this.PlaneHandleMaterial,
                this.PlaneHandleGrabbedMaterial,
                this.PlaneHandleFocusedMaterial,
                Quaternion.CreateFromAxisAngle(rotationAxis, MathHelper.ToRadians(-120f)),
                new AxisManipulationHelper[] { handleY, handleZ });

            // XZ
            this.CreateHandle(
                AxisManipulationHelperType.Plane,
                AxisType.XZ,
                this.PlaneHandlePrefab,
                this.PlaneHandleMaterial,
                this.PlaneHandleGrabbedMaterial,
                this.PlaneHandleFocusedMaterial,
                Quaternion.Identity,
                new AxisManipulationHelper[] { handleX, handleZ });
        }

        private void ApplyMaterialToAllComponents(MaterialComponent[] materialComponents, Material material)
        {
            if (material != null && material.Id != Guid.Empty)
            {
                for (int i = 0; i < materialComponents.Length; i++)
                {
                    materialComponents[i].Material = material;
                }
            }
        }

        private void ApplyMaterialToHandle(AxisManipulationHelper handle, Func<AxisManipulationHelper, Material> materialGetter)
        {
            this.ApplyMaterialToAllComponents(handle.MaterialComponents, materialGetter(handle));

            if (this.PlaneHandlesActivateAxisHandles)
            {
                for (int i = 0; i < handle.RelatedHandles.Length; i++)
                {
                    var h = handle.RelatedHandles[i];
                    this.ApplyMaterialToAllComponents(h.MaterialComponents, materialGetter(h));
                }
            }
        }

        private AxisManipulationHelper CreateHandle(AxisManipulationHelperType amhType, AxisType axisType, Prefab prefab, Material idleMaterial, Material grabbedMaterial, Material focusedMaterial, Quaternion orientation, AxisManipulationHelper[] relatedHandlers)
        {
            // Entity name suffix
            var suffix = $"{amhType}_{axisType}";

            // Handle root
            var handle = new Entity($"handle_{suffix}")
                .AddComponent(new Transform3D()
                {
                    LocalScale = Vector3.One * this.HandleScale,
                });

            this.rigRootEntity.AddChild(handle);

            if (prefab != null)
            {
                // Instantiate prefab
                var prefabInstance = prefab.Instantiate();

                var prefabTransform = prefabInstance.FindComponent<Transform3D>();
                prefabTransform.LocalOrientation = orientation;

                handle.AddChild(prefabInstance);
            }
            else
            {
                // Generate default look for the handle
                Vector3 position = Vector3.Zero;
                Vector3 size = Vector3.One;
                Component mesh = null;
                Component collider = null;

                switch (amhType)
                {
                    case AxisManipulationHelperType.Center:
                        var sphereDiameter = 1f;

                        mesh = new SphereMesh()
                        {
                            Diameter = sphereDiameter,
                        };
                        collider = new SphereCollider3D()
                        {
                            Margin = 0.0001f,
                            Radius = sphereDiameter,
                        };
                        break;

                    case AxisManipulationHelperType.Axis:
                        var axisLength = 4f;
                        var axisThickness = 0.5f;

                        size = new Vector3(axisLength, axisThickness, axisThickness);

                        mesh = new CubeMesh();
                        collider = new BoxCollider3D()
                        {
                            Margin = 0.0001f,
                            Size = size + Vector3.One,
                            Offset = Vector3.UnitX,
                        };

                        position = 0.5f * Vector3.UnitX * (axisLength + 2f);
                        break;

                    case AxisManipulationHelperType.Plane:
                        var planeLength = 2f;
                        var planeThickness = 0.25f;

                        size = new Vector3(planeLength, planeThickness, planeLength);

                        mesh = new CubeMesh();
                        collider = new BoxCollider3D()
                        {
                            Margin = 0.0001f,
                            Size = size + Vector3.One,
                            Offset = Vector3.UnitX + Vector3.UnitZ,
                        };

                        position = 0.5f * Vector3.Normalize(Vector3.UnitX + Vector3.UnitZ) * (planeLength + 2f);
                        break;
                }

                // Collider entity
                var handleCollider = new Entity($"collider_{suffix}")
                    .AddComponent(new Transform3D()
                    {
                        LocalPosition = Vector3.Transform(position, orientation),
                        LocalOrientation = orientation,
                    })
                    .AddComponent(collider)
                    .AddComponent(new StaticBody3D()
                    {
                        CollisionCategories = this.CollisionCategory,
                        IsSensor = true,
                    })
                    .AddComponent(new NearInteractionGrabbable());

                // Visual entity
                var handleVisual = new Entity($"visuals_{suffix}")
                    .AddComponent(new Transform3D()
                    {
                        LocalScale = size,
                    })
                    .AddComponent(mesh)
                    .AddComponent(new MeshRenderer())
                    .AddComponent(new MaterialComponent());

                // Build hierarchy
                handle.AddChild(handleCollider);
                handleCollider.AddChild(handleVisual);
            }

            // Apply material
            var materialComponents = handle.FindComponentsInChildren<MaterialComponent>().ToArray();
            this.ApplyMaterialToAllComponents(materialComponents, idleMaterial);

            // Register helper object
            var helperTargetEntity = handle.FindComponentInChildren<NearInteractionGrabbable>()?.Owner;

            if (helperTargetEntity == null)
            {
                throw new Exception($"The handle entity needs to have a {nameof(NearInteractionGrabbable)} component.");
            }

            var handleHelper = new AxisManipulationHelper()
            {
                Type = amhType,
                AxisType = axisType,
                BaseEntity = handle,
                MaterialComponents = materialComponents,
                IdleMaterial = idleMaterial,
                GrabbedMaterial = grabbedMaterial,
                FocusedMaterial = focusedMaterial,
                RelatedHandles = relatedHandlers,
            };

            this.helpers.Add(helperTargetEntity, handleHelper);

            return handleHelper;
        }

        /// <inheritdoc/>
        public void OnPointerDown(MixedRealityPointerEventData eventData)
        {
            if (eventData.EventHandled)
            {
                return;
            }

            if (this.currentCursor == null)
            {
                if (this.helpers.TryGetValue(eventData.CurrentTarget, out var handle))
                {
                    this.currentHandle = handle;

                    this.ApplyMaterialToHandle(this.currentHandle, h => h.GrabbedMaterial);

                    this.currentCursor = eventData.Cursor;
                    this.grabbedEntityPosition = this.transform.Position;
                    this.grabbedCursorPosition = eventData.Position;

                    this.FireManipulationEvent(handle.Type, started: true);

                    eventData.SetHandled();
                }
            }
        }

        /// <inheritdoc/>
        public void OnPointerDragged(MixedRealityPointerEventData eventData)
        {
            if (eventData.EventHandled)
            {
                return;
            }

            if (this.currentCursor == eventData.Cursor)
            {
                Vector3 delta = eventData.Position - this.grabbedCursorPosition;

                var deltaX = Vector3.Zero;
                var deltaY = Vector3.Zero;
                var deltaZ = Vector3.Zero;

                if (this.currentHandle.AxisType.HasFlag(AxisType.X))
                {
                    deltaX = Vector3.Project(delta, this.transform.WorldTransform.Right);
                }

                if (this.currentHandle.AxisType.HasFlag(AxisType.Y))
                {
                    deltaY = Vector3.Project(delta, this.transform.WorldTransform.Up);
                }

                if (this.currentHandle.AxisType.HasFlag(AxisType.Z))
                {
                    deltaZ = Vector3.Project(delta, this.transform.WorldTransform.Forward);
                }

                this.transform.Position = this.grabbedEntityPosition + deltaX + deltaY + deltaZ;

                eventData.SetHandled();
            }
        }

        /// <inheritdoc/>
        public void OnPointerUp(MixedRealityPointerEventData eventData)
        {
            if (eventData.EventHandled)
            {
                return;
            }

            if (this.currentCursor == eventData.Cursor)
            {
                this.FireManipulationEvent(this.currentHandle.Type, started: false);

                this.ApplyMaterialToHandle(this.currentHandle, h => h.IdleMaterial);

                this.currentCursor = null;
                this.currentHandle = null;

                eventData.SetHandled();
            }
        }

        /// <inheritdoc/>
        public void OnPointerClicked(MixedRealityPointerEventData eventData)
        {
        }

        /// <summary>
        /// Handle the OnTouchStarted event, changing the focused handle material.
        /// </summary>
        /// <param name="eventData">The event data.</param>
        public void OnTouchStarted(HandTrackingInputEventData eventData)
        {
            if (this.currentHandle == null && this.helpers.TryGetValue(eventData.CurrentTarget, out var handle))
            {
                this.ApplyMaterialToHandle(handle, h => h.FocusedMaterial);
            }
        }

        /// <summary>
        /// Handle the OnTouchUpdated event. Unused for this component.
        /// </summary>
        /// <param name="eventData">The event data.</param>
        public void OnTouchUpdated(HandTrackingInputEventData eventData)
        {
        }

        /// <summary>
        /// Handle the OnTouchCompleted event, changing the unfocused handle material.
        /// </summary>
        /// <param name="eventData">The event data.</param>
        public void OnTouchCompleted(HandTrackingInputEventData eventData)
        {
            if (this.currentHandle == null && this.helpers.TryGetValue(eventData.CurrentTarget, out var handle))
            {
                this.ApplyMaterialToHandle(handle, h => h.IdleMaterial);
            }
        }
    }
}
