// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.
using Evergine.Common.Attributes;
using Evergine.Common.Graphics;
using Evergine.Common.Input;
using Evergine.Components.Graphics3D;
using Evergine.Components.Primitives;
using Evergine.Components.XR;
using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.Framework.Physics3D;
using Evergine.Framework.Prefabs;
using Evergine.Framework.Threading;
using Evergine.Mathematics;
using System;
using System.Collections.Generic;

namespace Evergine.MRTK.Emulation
{
    /// <summary>
    /// Ray for selecting where to teleport.
    /// </summary>
    public class TeleportRay : Behavior
    {
        //// Teleport Effect Area
        ////private Entity teleportAreaEntity = null;
        ////private StaticBody3D teleportAreaStaticBody = null;
        ////private SphereCollider3D sphereCollider = null;
        ////private Transform3D teleportAreaTransform = null;

        /// <summary>
        /// Prefab for teleportation area.
        /// </summary>
        [RenderProperty(CustomPropertyName = "Teleport Area", Tooltip = "The teleport area prefab to be instantiated")]
        public Prefab TeleportationAreaPrefab = null;

        /// <summary>
        /// Prefab for teleportation line.
        /// </summary>
        [RenderProperty(CustomPropertyName = "Teleport Area", Tooltip = "The teleport line prefab to be instantiated")]
        public Prefab TeleportationLinePrefab = null;

        /// <summary>
        /// The tint color for the pointer when pointing to a allowed area.
        /// </summary>
        [RenderProperty(CustomPropertyName = "Allowed Area Color", Tooltip = "The tint color for the pointer when pointing to a allowed area")]
        public Color AllowedAreaColor = new Color(114.0f / 255.0f, 186.0f / 255.0f, 128.0f / 255.0f);

        /// <summary>
        /// The tint color for the pointer when pointing to a non-allowed area.
        /// </summary>
        [RenderProperty(CustomPropertyName = "Non Allowed Area Color", Tooltip = "The tint color for the pointer when pointing to a non-permitted area")]
        public Color NonAllowedAreaColor = new Color(188.0f / 255.0f, 74.0f / 255.0f, 74.0f / 255.0f);

        /// <summary>
        /// The tint color for the area pointer when pointing to a allowed area.
        /// </summary>
        [RenderProperty(CustomPropertyName = "Allowed Area Material", Tooltip = "The tint color for the area pointer when pointing to a allowed area")]
        public Material AllowedAreaMaterial = null;

        /// <summary>
        /// The tint color for the area pointer when pointing to a non-permitted area.
        /// </summary>
        [RenderProperty(CustomPropertyName = "Non Allowed Area Material", Tooltip = "The tint color for the area pointer when pointing to a non-permitted area")]
        public Material NonAllowedAreaMaterial = null;

        /// <summary>
        /// The thickness of the mesh line of the pointer.
        /// </summary>
        [RenderProperty(CustomPropertyName = "Pointer Thickness", Tooltip = "The thickness of the mesh line of the pointer")]
        public float Thickness = 0.03f;

        /// <summary>
        /// Gets or sets the strenght with which the arc of the pointer will be drawn.
        /// </summary>
        [RenderProperty(CustomPropertyName = "Pointer Strength", Tooltip = "The strength with which the arc of the pointer will be drawn")]
        public float Strength { get; set; } = 5f;

        /// <summary>
        /// Gets or sets the gravity to be applied to the pointer arc trace.
        /// </summary>
        [RenderProperty(CustomPropertyName = "Pointer Gravity", Tooltip = "The gravity to be applied to the pointer arc trace")]
        public float Gravity { get; set; } = -9.8f;

        /// <summary>
        /// Gets or sets the maximum number of vertices the line mesh of the pointer will contain.
        /// </summary>
        [RenderProperty(CustomPropertyName = "Max Vertex Line", Tooltip = "The maximum number of vertices the line mesh of the pointer can contain")]
        public int MaxVertexLine { get; set; } = 100;

        /// <summary>
        /// Gets or sets the size of the pointer line mesh segments.
        /// </summary>
        [RenderProperty(CustomPropertyName = "Ray Step", Tooltip = "The size of the pointer line mesh segments")]
        public float RayStepSize { get; set; } = 0.03f;

        /// <summary>
        /// Collision category that will be detected as ground. The ground is a place where the user can teleport.
        /// </summary>
        public CollisionCategory3D GroundCollisionCategory = CollisionCategory3D.Cat4;

        /// <summary>
        /// Collision category that will be detected as an obstacle. User can not teleport to obstacles.
        /// </summary>
        public CollisionCategory3D ObstacleCollisionCategory = CollisionCategory3D.Cat1;

        /// <summary>
        /// Controller to track with ray.
        /// </summary>
        public TrackXRController controller = null;

        /// <summary>
        /// Origin of the teleport ray.
        /// </summary>
        public Transform3D transform = null;

        //// Prefab entities
        private Entity teleportationAreaEntity = null;
        private Entity teleportationLineEntity = null;

        //// Teleportation Components
        private MaterialComponent teleportationAreaMaterialComponent;
        private Transform3D teleportationAreaTransform;
        private LineMeshRenderer3D teleportationLineMeshRenderer;
        private LineMesh teleportationLineMesh;

        //// Line mesh
        private List<Vector3> vertexList = new List<Vector3>();
        private Vector3 gravityVector;
        private Ray stepRay;
        private Vector3 groundPosition;
        private Vector3 normalPosition;

        //// Collisions
        private bool detectedCollisionArea = false;
        private bool groundDetected = false;
        private bool rayCollisionDetected = false;

        private bool isManipulationActive = false;

        /// <inheritdoc/>
        protected override async void Start()
        {
            base.Start();
            if (Application.Current.IsEditor)
            {
                return;
            }

            await EvergineForegroundTask.Run(() =>
            {
                this.teleportationAreaEntity = this.TeleportationAreaPrefab.Instantiate();
                if (this.teleportationAreaEntity != null)
                {
                    this.teleportationAreaEntity.IsEnabled = false;
                    this.Managers.EntityManager.Add(this.teleportationAreaEntity);
                }

                this.teleportationLineEntity = this.TeleportationLinePrefab.Instantiate();
                if (this.teleportationLineEntity != null)
                {
                    this.Managers.EntityManager.Add(this.teleportationAreaEntity);
                }

                var teleportationAreaStaticBody = this.teleportationAreaEntity.FindComponent<StaticBody3D>();
                if (teleportationAreaStaticBody != null)
                {
                    teleportationAreaStaticBody.BeginCollision += this.TeleportationAreaStaticBodyBeginCollision;
                    teleportationAreaStaticBody.EndCollision += this.TeleportationAreaStaticBodyEndCollision;
                }

                this.teleportationAreaTransform = this.teleportationAreaEntity.FindComponent<Transform3D>();
                this.teleportationAreaMaterialComponent = this.teleportationAreaEntity.FindComponentInChildren<MaterialComponent>();
                this.teleportationLineMeshRenderer = this.teleportationLineEntity.FindComponent<LineMeshRenderer3D>();
                this.teleportationLineMesh = this.teleportationLineEntity.FindComponent<LineMesh>();

                this.gravityVector = new Vector3(0.0f, this.Gravity, 0.0f);
            });
        }

        private void TeleportationAreaStaticBodyEndCollision(object sender, CollisionInfo3D e)
        {
            this.detectedCollisionArea = false;
        }

        private void TeleportationAreaStaticBodyBeginCollision(object sender, CollisionInfo3D e)
        {
            this.detectedCollisionArea = true;
        }

        /// <inheritdoc/>
        protected override void Update(TimeSpan gameTime)
        {
            if (this.controller == null)
            {
                return;
            }

            if (!this.isManipulationActive && !this.IsCursorCollision())
            {
                var btnState = this.controller.ControllerState.TriggerButton;
                if (btnState == ButtonState.Pressed)
                {
                    this.PressedBehavior();
                }
                else if (btnState == ButtonState.Releasing)
                {
                    this.ReleasingBehavior();
                }
            }
            else
            {
                this.teleportationAreaEntity.IsEnabled = false;
                this.DeleteLineMesh();
            }
        }

        private void DeleteLineMesh()
        {
            throw new NotImplementedException();
        }

        private void ReleasingBehavior()
        {
            throw new NotImplementedException();
        }

        private void PressedBehavior()
        {
            this.UpdateLinePath();

            this.CreateLineMesh();

            if (this.teleportationAreaEntity != null)
            {
                this.teleportationAreaMaterialComponent.Material = (this.rayCollisionDetected || this.detectedCollisionArea) ? this.NonAllowedAreaMaterial : this.AllowedAreaMaterial;
                this.teleportationAreaEntity.IsEnabled = this.groundDetected;
                this.UpdateTeleportationAreaPosition();
            }
        }

        private void UpdateTeleportationAreaPosition()
        {
            if (this.groundDetected)
            {
                this.teleportationAreaTransform.Position = this.groundPosition + (this.normalPosition * 0.1f);
                this.teleportationAreaTransform.Orientation = Quaternion.Identity;
            }
        }

        private void CreateLineMesh()
        {
            this.teleportationLineMeshRenderer.IsEnabled = true;
            var linePoints = this.teleportationLineMesh.LinePoints;
            linePoints.Clear();

            for (int i = 0; i < this.vertexList.Count - 1; i++)
            {
                linePoints.Add(new LinePointInfo()
                {
                    Position = this.vertexList[i],
                    Thickness = this.Thickness,
                    Color = (this.rayCollisionDetected || this.detectedCollisionArea) ? this.NonAllowedAreaColor : this.AllowedAreaColor,
                });
            }

            var tiling = this.teleportationLineMesh.TextureTiling;
            tiling.X = this.vertexList.Count;

            this.teleportationLineMesh.TextureTiling = tiling;
            this.teleportationLineMesh.LinePoints = linePoints;
        }

        private void UpdateLinePath()
        {
            this.rayCollisionDetected = false;
            this.groundDetected = false;

            Vector3 startingDirection = this.controller.Pointer.Direction * this.Strength;
            Vector3 originPosition = this.transform.Position;

            this.vertexList.Clear();
            this.vertexList.Add(originPosition);

            //// Until ground or max of vertex
            while (!this.groundDetected && this.vertexList.Count < this.MaxVertexLine)
            {
                ////????
                Vector3 newVertex = originPosition + (startingDirection * this.RayStepSize) + (0.5f * this.gravityVector * this.RayStepSize * this.RayStepSize);
                startingDirection += this.gravityVector * this.RayStepSize;

                this.vertexList.Add(newVertex);

                Vector3 currentDirection = Vector3.Normalize(newVertex - originPosition);
                this.stepRay.Position = originPosition;
                this.stepRay.Direction = currentDirection;

                float distance = Vector3.Distance(originPosition, newVertex);

                var collisionResult = this.Managers.PhysicManager3D.RayCast(ref this.stepRay, distance, this.ObstacleCollisionCategory);
                if (collisionResult.Succeeded)
                {
                    this.rayCollisionDetected = true;
                }

                var groundResult = this.Managers.PhysicManager3D.RayCast(ref this.stepRay, distance, CollisionCategory3D.Cat4);
                this.groundDetected = groundResult.Succeeded;
                if (this.groundDetected)
                {
                    this.groundPosition = groundResult.Point;
                    this.normalPosition = groundResult.Normal;
                }

                originPosition = newVertex;
            }
        }

        /// <summary>
        /// Checks if there is something too close to the cursor (the user could be trying to grab something).
        /// </summary>
        /// <returns>true if the user hand cursor is close to something.</returns>
        private bool IsCursorCollision()
        {
            Ray ray = this.controller.Pointer;
            var collisionResult = this.Managers.PhysicManager3D.RayCast(ref ray, 0.2f, CollisionCategory3D.All);

            return collisionResult.Succeeded;
        }
    }
}
