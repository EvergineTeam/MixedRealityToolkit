// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using System.Collections.Generic;
using System.Linq;
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

        // Rig
        private Entity rigRootEntity;
        private Dictionary<Entity, BoundingBoxHelper> helpers;
        private List<BoundingBoxHelper> helpersList;

        private Vector3 boundingBoxSize;

        // Interaction variables
        private Entity currentCursor;
        private BoundingBoxHelper currentHandle;

        private Vector3 initialGrabPoint;
        private Matrix4x4 transformOnGrabStart;

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
            this.AddHelpers();
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
                var rigRootPointerHandler = this.rigRootEntity.FindComponent<PointerHandler>();
                rigRootPointerHandler.OnPointerDown -= this.PointerHandler_OnPointerDown;
                rigRootPointerHandler.OnPointerDragged -= this.PointerHandler_OnPointerDragged;
                rigRootPointerHandler.OnPointerUp -= this.PointerHandler_OnPointerUp;

                this.Owner.RemoveChild(this.rigRootEntity);
                this.rigRootEntity = null;
            }
        }

        private void InitializeRigRoot()
        {
            var rigRootPointerHandler = new PointerHandler();
            rigRootPointerHandler.OnPointerDown += this.PointerHandler_OnPointerDown;
            rigRootPointerHandler.OnPointerDragged += this.PointerHandler_OnPointerDragged;
            rigRootPointerHandler.OnPointerUp += this.PointerHandler_OnPointerUp;

            this.rigRootEntity = new Entity(RIG_ROOT_NAME)
            {
                Flags = HideFlags.DontSave | HideFlags.DontShow,
            }
            .AddComponent(new Transform3D())
            .AddComponent(rigRootPointerHandler);

            this.Owner.AddChild(this.rigRootEntity);
        }

        private void InitializeDataStructures()
        {
            this.helpers = new Dictionary<Entity, BoundingBoxHelper>();
        }

        private Vector3[] GetCornerPositionsFromBounds()
        {
            var boundsCorners = new Vector3[8];

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
            for (int c = 0; c < boundsCorners.Length; c++)
            {
                boundsCorners[c] = new Vector3(
                    (c & (1 << 0)) == 0 ? minCorner[0] : maxCorner[0],
                    (c & (1 << 1)) == 0 ? minCorner[1] : maxCorner[1],
                    (c & (1 << 2)) == 0 ? minCorner[2] : maxCorner[2]);
            }

            return boundsCorners;
        }

        private Vector3 CalculateBoundingBoxSize(Vector3[] boundsCorners)
        {
            return boundsCorners[7] - boundsCorners[0];
        }

        private Vector3[] CalculateEdgeCenters(Vector3[] boundsCorners)
        {
            var edgeCenters = new Vector3[12];

            edgeCenters[0] = (boundsCorners[0] + boundsCorners[1]) * 0.5f;
            edgeCenters[1] = (boundsCorners[0] + boundsCorners[2]) * 0.5f;
            edgeCenters[2] = (boundsCorners[3] + boundsCorners[2]) * 0.5f;
            edgeCenters[3] = (boundsCorners[3] + boundsCorners[1]) * 0.5f;

            edgeCenters[4] = (boundsCorners[4] + boundsCorners[5]) * 0.5f;
            edgeCenters[5] = (boundsCorners[4] + boundsCorners[6]) * 0.5f;
            edgeCenters[6] = (boundsCorners[7] + boundsCorners[6]) * 0.5f;
            edgeCenters[7] = (boundsCorners[7] + boundsCorners[5]) * 0.5f;

            edgeCenters[8] = (boundsCorners[0] + boundsCorners[4]) * 0.5f;
            edgeCenters[9] = (boundsCorners[1] + boundsCorners[5]) * 0.5f;
            edgeCenters[10] = (boundsCorners[2] + boundsCorners[6]) * 0.5f;
            edgeCenters[11] = (boundsCorners[3] + boundsCorners[7]) * 0.5f;

            return edgeCenters;
        }

        private AxisType[] CalculateAxisTypes()
        {
            var edgeAxes = new AxisType[12];

            edgeAxes[0] = AxisType.X;
            edgeAxes[1] = AxisType.Y;
            edgeAxes[2] = AxisType.X;
            edgeAxes[3] = AxisType.Y;
            edgeAxes[4] = AxisType.X;
            edgeAxes[5] = AxisType.Y;
            edgeAxes[6] = AxisType.X;
            edgeAxes[7] = AxisType.Y;
            edgeAxes[8] = AxisType.Z;
            edgeAxes[9] = AxisType.Z;
            edgeAxes[10] = AxisType.Z;
            edgeAxes[11] = AxisType.Z;

            return edgeAxes;
        }

        private void AddHelpers()
        {
            var boundsCorners = this.GetCornerPositionsFromBounds();
            var edgeCenters = this.CalculateEdgeCenters(boundsCorners);
            var edgeAxes = this.CalculateAxisTypes();

            this.boundingBoxSize = this.CalculateBoundingBoxSize(boundsCorners);

            for (int i = 0; i < boundsCorners.Length; ++i)
            {
                // Add corners
                Transform3D cornerTransform = new Transform3D()
                {
                    LocalPosition = boundsCorners[i],
                };

                Entity corner = new Entity($"corner_{i}")
                    .AddComponent(cornerTransform)
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

                var cornerHelper = new BoundingBoxHelper()
                {
                    Type = BoundingBoxHelperType.ScaleHandle,
                    AxisType = AxisType.None,
                    Entity = corner,
                    Transform = cornerTransform,
                    OppositeHandlePosition = boundsCorners[7 - i],
                };

                this.helpers.Add(corner, cornerHelper);
            }

            // Add balls
            for (int i = 0; i < edgeCenters.Length; ++i)
            {
                Transform3D midpointTransform = new Transform3D()
                {
                    LocalPosition = edgeCenters[i],
                };

                Entity midpoint = new Entity($"midpoint_{i}")
                    .AddComponent(midpointTransform)
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

                var midpointHelper = new BoundingBoxHelper()
                {
                    Type = BoundingBoxHelperType.RotationHandle,
                    AxisType = edgeAxes[i],
                    Entity = midpoint,
                    Transform = midpointTransform,
                };

                this.helpers.Add(midpoint, midpointHelper);
            }

            for (int i = 0; i < edgeCenters.Length; ++i)
            {
                // Add links
                Vector3 rotation = Vector3.Zero;

                switch (edgeAxes[i])
                {
                    case AxisType.X:
                        rotation = new Vector3(0.0f, 0.0f, (float)Math.PI / 2);
                        break;
                    case AxisType.Y:
                        rotation = new Vector3(0.0f, (float)Math.PI / 2, 0.0f);
                        break;
                    case AxisType.Z:
                        rotation = new Vector3((float)Math.PI / 2, 0.0f, 0.0f);
                        break;
                }

                var linkTransform = new Transform3D()
                {
                    LocalPosition = edgeCenters[i],
                    Rotation = rotation,
                };

                Entity link = new Entity($"link_{i}")
                    .AddComponent(linkTransform);

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

                var linkHelper = new BoundingBoxHelper()
                {
                    Type = BoundingBoxHelperType.WireframeLink,
                    AxisType = edgeAxes[i],
                    Entity = link,
                    Transform = linkTransform,
                };

                this.helpers.Add(link, linkHelper);
            }

            this.helpersList = this.helpers.Values.ToList();
        }

        private void UpdateRigHandles()
        {
            for (int i = 0; i < this.helpersList.Count; i++)
            {
                var helper = this.helpersList[i];

                switch (helper.Type)
                {
                    case BoundingBoxHelperType.WireframeLink:
                        float scaleY = this.transform.WorldTransform.Scale.Y;

                        switch (helper.AxisType)
                        {
                            case AxisType.X:
                                scaleY *= this.boundingBoxSize.X;
                                break;
                            case AxisType.Y:
                                scaleY *= this.boundingBoxSize.Y;
                                break;
                            case AxisType.Z:
                                scaleY *= this.boundingBoxSize.Z;
                                break;
                        }

                        helper.Transform.Scale = new Vector3(this.LinkScale, scaleY, this.LinkScale);
                        break;

                    case BoundingBoxHelperType.RotationHandle:
                        helper.Transform.Scale = Vector3.One * this.RotationHandleScale;
                        break;

                    case BoundingBoxHelperType.ScaleHandle:
                        helper.Transform.Scale = Vector3.One * this.ScaleHandleScale;
                        break;
                }
            }
        }

        private void PointerHandler_OnPointerDown(object sender, MixedRealityPointerEventData eventData)
        {
            if (eventData.EventHandled)
            {
                return;
            }

            if (this.currentCursor == null)
            {
                this.currentHandle = this.helpers[eventData.CurrentTarget];

                this.currentCursor = eventData.Cursor;
                this.initialGrabPoint = eventData.Position;
                this.transformOnGrabStart = this.transform.WorldTransform;

                switch (this.currentHandle.Type)
                {
                    case BoundingBoxHelperType.RotationHandle:
                        var axis = this.currentHandle.GetRotationAxis(this.transform.WorldTransform);
                        this.currentRotationAxis = Vector3.Normalize(axis);
                        break;

                    case BoundingBoxHelperType.ScaleHandle:
                        this.grabOppositeCorner = Vector3.TransformCoordinate(this.currentHandle.OppositeHandlePosition, this.transform.WorldTransform);
                        this.grabDiagonalDirection = Vector3.Normalize(this.currentHandle.Transform.Position - this.grabOppositeCorner);
                        break;
                }

                eventData.SetHandled();
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
                var currentGrabPoint = eventData.Position;

                switch (this.currentHandle.Type)
                {
                    case BoundingBoxHelperType.RotationHandle:
                        var initialDir = this.ProjectOnPlane(this.transform.Position, this.currentRotationAxis, this.initialGrabPoint);
                        var currentDir = this.ProjectOnPlane(this.transform.Position, this.currentRotationAxis, currentGrabPoint);

                        var dir1 = Vector3.Normalize(initialDir - this.transform.Position);
                        var dir2 = Vector3.Normalize(currentDir - this.transform.Position);

                        // Check that the two vectors are not the same, workaround for a bug in Quaternion.CreateFromTwoVectors
                        if (Vector3.DistanceSquared(dir1, dir2) > MathHelper.Epsilon)
                        {
                            var rotation = Quaternion.CreateFromTwoVectors(dir1, dir2);
                            this.transform.Orientation = rotation * this.transformOnGrabStart.Orientation;
                        }

                        break;

                    case BoundingBoxHelperType.ScaleHandle:
                        float initialDist = Vector3.Dot(this.initialGrabPoint - this.grabOppositeCorner, this.grabDiagonalDirection);
                        float currentDist = Vector3.Dot(currentGrabPoint - this.grabOppositeCorner, this.grabDiagonalDirection);
                        float scaleFactor = 1 + ((currentDist - initialDist) / initialDist);

                        this.transform.Scale = this.transformOnGrabStart.Scale * scaleFactor;
                        this.transform.Position = this.grabOppositeCorner + (scaleFactor * (this.transformOnGrabStart.Translation - this.grabOppositeCorner));

                        this.UpdateRigHandles();

                        break;
                }

                eventData.SetHandled();
            }
        }

        private void PointerHandler_OnPointerUp(object sender, MixedRealityPointerEventData eventData)
        {
            if (this.currentCursor == eventData.Cursor)
            {
                this.currentCursor = null;
                this.currentHandle = null;

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
