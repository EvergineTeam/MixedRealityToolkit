// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using System.Collections.Generic;
using WaveEngine.Common.Attributes;
using WaveEngine.Common.Graphics;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Graphics.Effects;
using WaveEngine.Framework.Graphics.Materials;
using WaveEngine.Framework.Physics3D;
using WaveEngine.Framework.Services;
using WaveEngine.Mathematics;
using WaveEngine.MRTK.Base.EventDatum.Input;
using WaveEngine.MRTK.SDK.Features.Input.Handlers;
using WaveEngine.MRTK.Services.InputSystem;

namespace WaveEngine.MRTK.SDK.Features.UX.Components.BoundingBox
{
    /// <summary>
    /// BoundingBox allows to transform objects (rotate and scale) and draws a cube around the object to visualize
    /// the possibility of user triggered transform manipulation.
    /// </summary>
    public class BoundingBox : Component
    {
        private static readonly string RIG_ROOT_NAME = "rigRoot";

        [BindService]
        private AssetsService assetsService = null;

        [BindComponent]
        private Transform3D transform = null;

        [BindComponent]
        private BoxCollider3D boxCollider3D = null;

        [BindComponent(isExactType: false, isRequired: false, source: BindComponentSource.Children)]
        private MeshComponent meshComponent = null;

        /// <summary>
        /// Gets or sets the scale applied to the scale handles.
        /// </summary>
        [RenderProperty(Tooltip = "Scale applied to the scale handles")]
        public float ScaleHandleScale { get; set; } = 0.02f;

        /// <summary>
        /// Gets or sets the scale applied to the rotation handles.
        /// </summary>
        [RenderProperty(Tooltip = "Scale applied to the rotation handles")]
        public float RotationHandleScale { get; set; } = 0.02f;

        /// <summary>
        /// Gets or sets the scale applied to the wireframe links.
        /// </summary>
        [RenderProperty(Tooltip = "Scale applied to the wireframe links")]
        public float LinkScale { get; set; } = 0.005f;

        private StandardMaterial handleMaterial;

        private Entity currentCursor;
        private Transform3D rigRootTransform;
        private PointerHandler rigRootPointerHandler;
        private Entity rigRootEntity;

        private Vector3[] boundsCorners;
        private Vector3[] edgeCenters;
        private CardinalAxisType[] edgeAxes;

        private List<Entity> corners;
        private List<Entity> balls;
        private List<Entity> links;

        private Vector3 boundsSize;

        private HandleType currentHandleType;
        private Vector3 initialGrabPoint;
        private Vector3 initialScaleOnGrabStart;
        private Vector3 initialPositionOnGrabStart;
        private Quaternion initialOrientationOnGrabStart;

        private Vector3 grabOppositeCorner;
        private Vector3 grabDiagonalDirection;
        private Vector3 currentRotationAxis;

        /// <inheritdoc/>
        protected override bool OnAttached()
        {
            var attached = base.OnAttached();

            if (attached)
            {
                var effect = this.assetsService.Load<Effect>(DefaultResourcesIDs.StandardEffectID);
                var opaqueLayer = this.assetsService.Load<RenderLayerDescription>(DefaultResourcesIDs.OpaqueRenderLayerID);

                this.handleMaterial = new StandardMaterial(effect)
                {
                    LightingEnabled = false,
                    IBLEnabled = true,
                    LayerDescription = opaqueLayer,
                };

                this.transform.ScaleChanged += this.Transform_ScaleChanged;
                this.transform.LocalScaleChanged += this.Transform_ScaleChanged;
            }

            return attached;
        }

        /// <inheritdoc/>
        protected override void OnDetach()
        {
            base.OnDetach();

            this.transform.ScaleChanged -= this.Transform_ScaleChanged;
            this.transform.LocalScaleChanged -= this.Transform_ScaleChanged;
        }

        private void Transform_ScaleChanged(object sender, EventArgs e)
        {
            this.UpdateRigHandles();
        }

        /// <inheritdoc/>
        protected override void OnActivated()
        {
            base.OnActivated();

            this.CreateRig();
        }

        /// <inheritdoc/>
        protected override void OnDeactivated()
        {
            base.OnDeactivated();

            this.DestroyRig();
        }

        /// <summary>
        /// Destroys and re-creates the rig around the bounding box.
        /// </summary>
        public void CreateRig()
        {
            this.DestroyRig();
            ////SetMaterials();
            this.InitializeRigRoot();
            this.InitializeDataStructures();
            ////this.SetBoundingBoxCollider();
            this.UpdateBounds();
            this.AddCorners();
            this.AddLinks();
            ////HandleIgnoreCollider();
            ////AddBoxDisplay();
            this.UpdateRigHandles();
            ////Flatten();
            ////ResetHandleVisibility();
            ////rigRoot.gameObject.SetActive(active);
            ////UpdateRigVisibilityInInspector();
        }

        private void DestroyRig()
        {
            if (this.rigRootEntity != null)
            {
                this.rigRootPointerHandler.OnPointerDown -= this.PointerHandler_OnPointerDown;
                this.rigRootPointerHandler.OnPointerDragged -= this.PointerHandler_OnPointerDragged;
                this.rigRootPointerHandler.OnPointerUp -= this.PointerHandler_OnPointerUp;

                this.Owner.RemoveChild(this.rigRootEntity);
                this.rigRootEntity = null;
            }
        }

        private void InitializeRigRoot()
        {
            this.rigRootTransform = new Transform3D();

            this.rigRootPointerHandler = new PointerHandler();
            this.rigRootPointerHandler.OnPointerDown += this.PointerHandler_OnPointerDown;
            this.rigRootPointerHandler.OnPointerDragged += this.PointerHandler_OnPointerDragged;
            this.rigRootPointerHandler.OnPointerUp += this.PointerHandler_OnPointerUp;

            this.rigRootEntity = new Entity(RIG_ROOT_NAME)
            {
                Flags = HideFlags.DontSave | HideFlags.DontShow,
            }
            .AddComponent(this.rigRootTransform)
            .AddComponent(this.rigRootPointerHandler);

            this.Owner.AddChild(this.rigRootEntity);
        }

        private void InitializeDataStructures()
        {
            this.boundsCorners = new Vector3[8];
            this.edgeCenters = new Vector3[12];
            this.edgeAxes = new CardinalAxisType[12];

            this.corners = new List<Entity>();
            this.balls = new List<Entity>();
            this.links = new List<Entity>();
        }

        private void UpdateBounds()
        {
            this.GetCornerPositionsFromBounds();
            this.CalculateEdgeCenters();
        }

        private void GetCornerPositionsFromBounds()
        {
            var center = this.boxCollider3D.Offset;
            var extent = this.boxCollider3D.Size / 2;

            // Get size from child MeshComponent
            var bbox = this.meshComponent?.Model?.BoundingBox;

            if (bbox.HasValue)
            {
                center = bbox.Value.Center;
                extent = bbox.Value.HalfExtent;
            }

            // Permutate all axes using minCorner and maxCorner.
            Vector3 minCorner = center - extent;
            Vector3 maxCorner = center + extent;
            for (int c = 0; c < this.boundsCorners.Length; c++)
            {
                this.boundsCorners[c] = new Vector3(
                    (c & (1 << 0)) == 0 ? minCorner[0] : maxCorner[0],
                    (c & (1 << 1)) == 0 ? minCorner[1] : maxCorner[1],
                    (c & (1 << 2)) == 0 ? minCorner[2] : maxCorner[2]);
            }

            this.boundsSize = maxCorner - minCorner;
        }

        private void CalculateEdgeCenters()
        {
            if (this.boundsCorners != null)
            {
                this.edgeCenters[0] = (this.boundsCorners[0] + this.boundsCorners[1]) * 0.5f;
                this.edgeCenters[1] = (this.boundsCorners[0] + this.boundsCorners[2]) * 0.5f;
                this.edgeCenters[2] = (this.boundsCorners[3] + this.boundsCorners[2]) * 0.5f;
                this.edgeCenters[3] = (this.boundsCorners[3] + this.boundsCorners[1]) * 0.5f;

                this.edgeCenters[4] = (this.boundsCorners[4] + this.boundsCorners[5]) * 0.5f;
                this.edgeCenters[5] = (this.boundsCorners[4] + this.boundsCorners[6]) * 0.5f;
                this.edgeCenters[6] = (this.boundsCorners[7] + this.boundsCorners[6]) * 0.5f;
                this.edgeCenters[7] = (this.boundsCorners[7] + this.boundsCorners[5]) * 0.5f;

                this.edgeCenters[8] = (this.boundsCorners[0] + this.boundsCorners[4]) * 0.5f;
                this.edgeCenters[9] = (this.boundsCorners[1] + this.boundsCorners[5]) * 0.5f;
                this.edgeCenters[10] = (this.boundsCorners[2] + this.boundsCorners[6]) * 0.5f;
                this.edgeCenters[11] = (this.boundsCorners[3] + this.boundsCorners[7]) * 0.5f;
            }
        }

        private void AddCorners()
        {
            for (int i = 0; i < this.boundsCorners.Length; ++i)
            {
                // Add corners
                Entity corner = new Entity($"corner_{i}")
                    .AddComponent(new Transform3D()
                    {
                        LocalPosition = this.boundsCorners[i],
                    })
                    .AddComponent(new BoxCollider3D()
                     {
                         Margin = 0.0001f,
                     })
                    .AddComponent(new StaticBody3D() { IsSensor = true })
                    .AddComponent(new NearInteractionGrabbable());

                this.rigRootEntity.AddChild(corner);

                Entity visual = new Entity("visuals")
                    .AddComponent(new Transform3D())
                    .AddComponent(new CubeMesh())
                    .AddComponent(new MeshRenderer())
                    .AddComponent(new MaterialComponent()
                    {
                        Material = this.handleMaterial.Material,
                    });

                corner.AddChild(visual);

                this.corners.Add(corner);
            }
        }

        private void AddLinks()
        {
            this.edgeAxes[0] = CardinalAxisType.X;
            this.edgeAxes[1] = CardinalAxisType.Y;
            this.edgeAxes[2] = CardinalAxisType.X;
            this.edgeAxes[3] = CardinalAxisType.Y;
            this.edgeAxes[4] = CardinalAxisType.X;
            this.edgeAxes[5] = CardinalAxisType.Y;
            this.edgeAxes[6] = CardinalAxisType.X;
            this.edgeAxes[7] = CardinalAxisType.Y;
            this.edgeAxes[8] = CardinalAxisType.Z;
            this.edgeAxes[9] = CardinalAxisType.Z;
            this.edgeAxes[10] = CardinalAxisType.Z;
            this.edgeAxes[11] = CardinalAxisType.Z;

            for (int i = 0; i < this.edgeCenters.Length; ++i)
            {
                // Add balls
                Entity midpoint = new Entity($"midpoint_{i}")
                    .AddComponent(new Transform3D()
                    {
                        LocalPosition = this.edgeCenters[i],
                    })
                    .AddComponent(new SphereCollider3D()
                    {
                        Margin = 0.0001f,
                    })
                    .AddComponent(new StaticBody3D() { IsSensor = true })
                    .AddComponent(new NearInteractionGrabbable());

                this.rigRootEntity.AddChild(midpoint);

                Entity midpointVisual = new Entity("visuals")
                    .AddComponent(new Transform3D())
                    .AddComponent(new SphereMesh())
                    .AddComponent(new MeshRenderer())
                    .AddComponent(new MaterialComponent()
                    {
                        Material = this.handleMaterial.Material,
                    });

                midpoint.AddChild(midpointVisual);

                this.balls.Add(midpoint);

                // Add links
                Vector3 rotation;
                if (this.edgeAxes[i] == CardinalAxisType.Y)
                {
                    rotation = new Vector3(0.0f, (float)Math.PI / 2, 0.0f);
                }
                else if (this.edgeAxes[i] == CardinalAxisType.Z)
                {
                    rotation = new Vector3((float)Math.PI / 2, 0.0f, 0.0f);
                }
                else //// X
                {
                    rotation = new Vector3(0.0f, 0.0f, (float)Math.PI / 2);
                }

                Entity link = new Entity($"link_{i}")
                    .AddComponent(new Transform3D()
                    {
                        LocalPosition = this.edgeCenters[i],
                        Rotation = rotation,
                    });

                this.rigRootEntity.AddChild(link);

                Entity linkVisual = new Entity("visuals")
                    .AddComponent(new Transform3D())
                    .AddComponent(new CylinderMesh())
                    .AddComponent(new MeshRenderer())
                    .AddComponent(new MaterialComponent()
                    {
                        Material = this.handleMaterial.Material,
                    });

                link.AddChild(linkVisual);

                this.links.Add(link);
            }
        }

        private Vector3 GetRotationAxis(Entity handle)
        {
            for (int i = 0; i < this.balls.Count; ++i)
            {
                if (handle == this.balls[i])
                {
                    System.Diagnostics.Trace.WriteLine(this.edgeAxes[i]);
                    if (this.edgeAxes[i] == CardinalAxisType.X)
                    {
                        return this.transform.WorldTransform.Right;
                    }
                    else if (this.edgeAxes[i] == CardinalAxisType.Y)
                    {
                        return this.transform.WorldTransform.Up;
                    }
                    else
                    {
                        return this.transform.WorldTransform.Forward;
                    }
                }
            }

            return Vector3.Zero;
        }

        private void UpdateRigHandles()
        {
            for (int i = 0; i < this.corners.Count; i++)
            {
                var cornerTransform = this.corners[i].FindComponent<Transform3D>();
                cornerTransform.Scale = Vector3.One * this.ScaleHandleScale;
            }

            for (int i = 0; i < this.balls.Count; i++)
            {
                var ballTransform = this.balls[i].FindComponent<Transform3D>();
                ballTransform.Scale = Vector3.One * this.RotationHandleScale;
            }

            for (int i = 0; i < this.links.Count; i++)
            {
                float scaleY = this.transform.WorldTransform.Scale.Y;

                if (this.edgeAxes[i] == CardinalAxisType.X)
                {
                    scaleY *= this.boundsSize.X;
                }
                else if (this.edgeAxes[i] == CardinalAxisType.Y)
                {
                    scaleY *= this.boundsSize.Y;
                }
                else //// Z
                {
                    scaleY *= this.boundsSize.Z;
                }

                var linkTransform = this.links[i].FindComponent<Transform3D>();
                linkTransform.Scale = new Vector3(this.LinkScale, scaleY, this.LinkScale);
            }
        }

        private HandleType GetHandleType(Entity handle)
        {
            for (int i = 0; i < this.corners.Count; ++i)
            {
                if (handle == this.corners[i])
                {
                    return HandleType.Scale;
                }
            }

            for (int i = 0; i < this.balls.Count; ++i)
            {
                if (handle == this.balls[i])
                {
                    return HandleType.Rotation;
                }
            }

            return HandleType.None;
        }

        private void PointerHandler_OnPointerDown(object sender, MixedRealityPointerEventData eventData)
        {
            if (eventData.EventHandled)
            {
                return;
            }

            if (this.currentCursor == null)
            {
                var grabbedHandle = eventData.CurrentTarget;
                this.currentHandleType = this.GetHandleType(grabbedHandle);

                if (this.currentHandleType != HandleType.None)
                {
                    this.currentCursor = eventData.Cursor;
                    this.initialGrabPoint = eventData.Position;
                    this.initialScaleOnGrabStart = this.transform.LocalScale;
                    this.initialPositionOnGrabStart = this.transform.Position;
                    this.initialOrientationOnGrabStart = this.transform.Orientation;

                    if (this.currentHandleType == HandleType.Scale)
                    {
                        var grabbedHandleTransform = grabbedHandle.FindComponent<Transform3D>();

                        //// grabOppositeCorner seems to be miscalculated
                        this.grabOppositeCorner = Vector3.TransformCoordinate(-grabbedHandleTransform.LocalPosition, this.rigRootTransform.WorldTransform);
                        this.grabDiagonalDirection = Vector3.Normalize(grabbedHandleTransform.Position - this.grabOppositeCorner);
                    }
                    else if (this.currentHandleType == HandleType.Rotation)
                    {
                        this.currentRotationAxis = Vector3.Normalize(this.GetRotationAxis(grabbedHandle));
                        System.Diagnostics.Trace.WriteLine(this.currentRotationAxis);
                    }

                    eventData.SetHandled();
                }
            }
        }

        private void PointerHandler_OnPointerDragged(object sender, MixedRealityPointerEventData eventData)
        {
            if (eventData.EventHandled)
            {
                return;
            }

            if (this.currentCursor == eventData.Cursor)
            {
                if (this.currentHandleType != HandleType.None)
                {
                    var currentGrabPoint = eventData.Position;

                    if (this.currentHandleType == HandleType.Scale)
                    {
                        float initialDist = Vector3.Dot(this.initialGrabPoint - this.grabOppositeCorner, this.grabDiagonalDirection);
                        float currentDist = Vector3.Dot(currentGrabPoint - this.grabOppositeCorner, this.grabDiagonalDirection);
                        float scaleFactor = 1 + ((currentDist - initialDist) / initialDist);

                        this.transform.LocalScale = this.initialScaleOnGrabStart * scaleFactor;
                        this.transform.Position = this.grabOppositeCorner + (scaleFactor * (this.initialPositionOnGrabStart - this.grabOppositeCorner));
                    }
                    else if (this.currentHandleType == HandleType.Rotation)
                    {
                        var initialDir = this.ProjectOnPlane(this.transform.Position, this.currentRotationAxis, this.initialGrabPoint);
                        var currentDir = this.ProjectOnPlane(this.transform.Position, this.currentRotationAxis, currentGrabPoint);

                        var dir1 = Vector3.Normalize(initialDir - this.transform.Position);
                        var dir2 = Vector3.Normalize(currentDir - this.transform.Position);

                        // Check that the two vectors are not the same, workaround for a bug in Quaternion.CreateFromTwoVectors
                        if (Vector3.DistanceSquared(dir1, dir2) > MathHelper.Epsilon)
                        {
                            var rotation = Quaternion.CreateFromTwoVectors(dir1, dir2);
                            this.transform.Orientation = rotation * this.initialOrientationOnGrabStart;
                        }
                    }
                }

                eventData.SetHandled();
            }
        }

        private void PointerHandler_OnPointerUp(object sender, MixedRealityPointerEventData eventData)
        {
            ////if (eventData.EventHandled)
            ////{
            ////    return;
            ////}

            if (this.currentCursor == eventData.Cursor)
            {
                ////DropController();

                this.currentCursor = null;
                this.currentHandleType = HandleType.None;

                eventData.SetHandled();
            }
        }

        private Vector3 ProjectOnPlane(Vector3 planePoint, Vector3 planeNormal, Vector3 point)
        {
            var diff = point - planePoint;
            float dot = Vector3.Dot(diff, planeNormal);
            return point - (dot * planeNormal);
        }
    }
}
