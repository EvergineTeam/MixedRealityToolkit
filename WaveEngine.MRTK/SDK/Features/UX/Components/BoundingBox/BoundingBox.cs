﻿// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

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

        [BindComponent(isRequired: false)]
        private BoxCollider3D boxCollider3D = null;

        [BindComponent(isExactType: false, isRequired: false, source: BindComponentSource.Children)]
        private MeshComponent meshComponent = null;

        /// <summary>
        /// Gets or sets the scale applied to the scale handles.
        /// </summary>
        [RenderProperty(Tooltip = "Scale applied to the scale handles")]
        public float ScaleHandleScale
        {
            get
            {
                return this.scaleHandleScale;
            }

            set
            {
                if (this.scaleHandleScale != value)
                {
                    this.scaleHandleScale = value;
                    this.CreateRig();
                }
            }
        }

        private float scaleHandleScale = 0.02f;

        /// <summary>
        /// Gets or sets the scale applied to the rotation handles.
        /// </summary>
        [RenderProperty(Tooltip = "Scale applied to the rotation handles")]
        public float RotationHandleScale
        {
            get
            {
                return this.rotationHandleScale;
            }

            set
            {
                if (this.rotationHandleScale != value)
                {
                    this.rotationHandleScale = value;
                    this.CreateRig();
                }
            }
        }

        private float rotationHandleScale = 0.02f;

        /// <summary>
        /// Gets or sets the scale applied to the wireframe links.
        /// </summary>
        [RenderProperty(Tooltip = "Scale applied to the wireframe links")]
        public float LinkScale
        {
            get
            {
                return this.linkScale;
            }

            set
            {
                if (this.linkScale != value)
                {
                    this.linkScale = value;
                    this.CreateRig();
                }
            }
        }

        private float linkScale = 0.005f;

        /// <summary>
        /// Gets or sets the extra padding added to the actual bounds.
        /// </summary>
        [RenderProperty(Tooltip = "Extra padding added to the actual bounds.")]
        public Vector3 BoxPadding
        {
            get
            {
                return this.boxPadding;
            }

            set
            {
                if (Vector3.Distance(this.boxPadding, value) > MathHelper.Epsilon)
                {
                    this.boxPadding = value;
                    this.CreateRig();
                }
            }
        }

        private Vector3 boxPadding = Vector3.Zero;

        /// <summary>
        /// Gets or sets the material applied to the box when not in a grabbed state.
        /// </summary>
        [RenderProperty(Tooltip = "The material applied to the box when not in a grabbed state. If set to null, no box will be displayed.")]
        public Material BoxMaterial
        {
            get
            {
                return this.boxMaterial;
            }

            set
            {
                if (this.boxMaterial != value)
                {
                    this.boxMaterial = value;
                    this.CreateRig();
                }
            }
        }

        private Material boxMaterial;

        /// <summary>
        /// Gets or sets the material applied to the box when in a grabbed state.
        /// </summary>
        [RenderProperty(Tooltip = "The material applied to the box when in a grabbed state. If set to null, no change will occur when grabbed.")]
        public Material BoxGrabbedMaterial
        {
            get
            {
                return this.boxGrabbedMaterial;
            }

            set
            {
                if (this.boxGrabbedMaterial != value)
                {
                    this.boxGrabbedMaterial = value;
                    this.CreateRig();
                }
            }
        }

        private Material boxGrabbedMaterial;

        /// <summary>
        /// Gets or sets a value indicating whether to show the wireframe links around the box or not.
        /// </summary>
        [RenderProperty(Tooltip = "Whether to show the wireframe links around the box or not")]
        public bool ShowWireframe
        {
            get
            {
                return this.showWireframe;
            }

            set
            {
                if (this.showWireframe != value)
                {
                    this.showWireframe = value;
                    this.CreateRig();
                }
            }
        }

        private bool showWireframe = true;

        /// <summary>
        /// Gets or sets a value indicating whether to show the scale handles on the corners or not.
        /// </summary>
        [RenderProperty(Tooltip = "Whether to show the scale handles on the corners or not")]
        public bool ShowScaleHandles
        {
            get
            {
                return this.showScaleHandles;
            }

            set
            {
                if (this.showScaleHandles != value)
                {
                    this.showScaleHandles = value;
                    this.CreateRig();
                }
            }
        }

        private bool showScaleHandles = true;

        /// <summary>
        /// Gets or sets a value indicating whether to show the rotation handle for axis X or not.
        /// </summary>
        [RenderProperty(Tooltip = "Whether to show the rotation handle for axis X or not")]
        public bool ShowXScaleHandle
        {
            get
            {
                return this.showXScaleHandle;
            }

            set
            {
                if (this.showXScaleHandle != value)
                {
                    this.showXScaleHandle = value;
                    this.CreateRig();
                }
            }
        }

        private bool showXScaleHandle = true;

        /// <summary>
        /// Gets or sets a value indicating whether to show the rotation handle for axis X or not.
        /// </summary>
        [RenderProperty(Tooltip = "Whether to show the rotation handle for axis X or not")]
        public bool ShowYScaleHandle
        {
            get
            {
                return this.showYScaleHandle;
            }

            set
            {
                if (this.showYScaleHandle != value)
                {
                    this.showYScaleHandle = value;
                    this.CreateRig();
                }
            }
        }

        private bool showYScaleHandle = true;

        /// <summary>
        /// Gets or sets a value indicating whether to show the rotation handle for axis X or not.
        /// </summary>
        [RenderProperty(Tooltip = "Whether to show the rotation handle for axis X or not")]
        public bool ShowZScaleHandle
        {
            get
            {
                return this.showZScaleHandle;
            }

            set
            {
                if (this.showZScaleHandle != value)
                {
                    this.showZScaleHandle = value;
                    this.CreateRig();
                }
            }
        }

        private bool showZScaleHandle = true;

        /// <summary>
        /// Gets or sets a value indicating whether to show the rotation handle for axis X or not.
        /// </summary>
        [RenderProperty(Tooltip = "Whether to show the rotation handle for axis X or not")]
        public bool ShowXRotationHandle
        {
            get
            {
                return this.showXRotationHandle;
            }

            set
            {
                if (this.showXRotationHandle != value)
                {
                    this.showXRotationHandle = value;
                    this.CreateRig();
                }
            }
        }

        private bool showXRotationHandle = true;

        /// <summary>
        /// Gets or sets a value indicating whether to show the rotation handle for axis X or not.
        /// </summary>
        [RenderProperty(Tooltip = "Whether to show the rotation handle for axis Y or not")]
        public bool ShowYRotationHandle
        {
            get
            {
                return this.showYRotationHandle;
            }

            set
            {
                if (this.showYRotationHandle != value)
                {
                    this.showYRotationHandle = value;
                    this.CreateRig();
                }
            }
        }

        private bool showYRotationHandle = true;

        /// <summary>
        /// Gets or sets a value indicating whether to show the rotation handle for axis X or not.
        /// </summary>
        [RenderProperty(Tooltip = "Whether to show the rotation handle for axis Z or not")]
        public bool ShowZRotationHandle
        {
            get
            {
                return this.showZRotationHandle;
            }

            set
            {
                if (this.showZRotationHandle != value)
                {
                    this.showZRotationHandle = value;
                    this.CreateRig();
                }
            }
        }

        private bool showZRotationHandle = true;

        /// <summary>
        /// Gets or sets the shape of the wireframe links.
        /// </summary>
        [RenderProperty(Tooltip = "The shape of the wireframe links")]
        public WireframeType WireframeShape
        {
            get
            {
                return this.wireframeShape;
            }

            set
            {
                if (this.wireframeShape != value)
                {
                    this.wireframeShape = value;
                    this.CreateRig();
                }
            }
        }

        private WireframeType wireframeShape = WireframeType.Cubic;

        /// <summary>
        /// Gets or sets the material applied to the wireframe links.
        /// </summary>
        [RenderProperty(Tooltip = "The material applied to the wireframe links")]
        public Material WireframeMaterial
        {
            get
            {
                return this.wireframeMaterial;
            }

            set
            {
                if (this.wireframeMaterial != value)
                {
                    this.wireframeMaterial = value;
                    this.CreateRig();
                }
            }
        }

        private Material wireframeMaterial;

        /// <summary>
        /// Gets or sets the material applied to the handles when not in a grabbed state.
        /// </summary>
        [RenderProperty(Tooltip = "The material applied to the handles when not in a grabbed state")]
        public Material HandleMaterial
        {
            get
            {
                return this.handleMaterial;
            }

            set
            {
                if (this.handleMaterial != value)
                {
                    this.handleMaterial = value;
                    this.CreateRig();
                }
            }
        }

        private Material handleMaterial;

        /// <summary>
        /// Gets or sets the material applied to the handles when in a grabbed state.
        /// </summary>
        [RenderProperty(Tooltip = "The material applied to the handles when in a grabbed state")]
        public Material HandleGrabbedMaterial
        {
            get
            {
                return this.handleGrabbedMaterial;
            }

            set
            {
                if (this.handleGrabbedMaterial != value)
                {
                    this.handleGrabbedMaterial = value;
                    this.CreateRig();
                }
            }
        }

        private Material handleGrabbedMaterial;

        /// <summary>
        /// Gets or sets the collision category of the handles.
        /// </summary>
        public CollisionCategory3D CollisionCategory
        {
            get
            {
                return this.collisionCategory3D;
            }

            set
            {
                if (this.collisionCategory3D != value)
                {
                    this.collisionCategory3D = value;
                    this.CreateRig();
                }
            }
        }

        private CollisionCategory3D collisionCategory3D = CollisionCategory3D.Cat1;

        /// <summary>
        /// Event fired when interaction with a rotation handle starts.
        /// </summary>
        public event EventHandler RotateStarted;

        /// <summary>
        /// Event fired when interaction with a rotation handle stops.
        /// </summary>
        public event EventHandler RotateStopped;

        /// <summary>
        /// Event fired when interaction with a scale handle starts.
        /// </summary>
        public event EventHandler ScaleStarted;

        /// <summary>
        /// Event fired when interaction with a scale handle stops.
        /// </summary>
        public event EventHandler ScaleStopped;

        // Rig
        private Entity rigRootEntity;
        private Dictionary<Entity, BoundingBoxHelper> helpers;
        private List<BoundingBoxHelper> helpersList;
        private Entity boxDisplay;

        private Vector3 boundingBoxCenter;
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

            if (!Application.Current.IsEditor)
            {
                if (this.boxCollider3D == null)
                {
                    this.boxCollider3D = new BoxCollider3D();
                    this.Owner.AddComponent(this.boxCollider3D);
                }
            }

            if (attached)
            {
                var effect = this.assetsService.Load<Effect>(DefaultResourcesIDs.StandardEffectID);
                var opaqueLayer = this.assetsService.Load<RenderLayerDescription>(DefaultResourcesIDs.OpaqueRenderLayerID);

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
            if (this.Owner != null)
            {
                this.DestroyRig();
                this.InitializeRigRoot();
                this.InitializeDataStructures();
                ////this.SetBoundingBoxCollider();
                this.AddHelpers();
                ////HandleIgnoreCollider();
                this.AddBoxDisplay();
                this.UpdateRigHandles();
                ////Flatten();
                ////ResetHandleVisibility();
                ////rigRoot.gameObject.SetActive(active);
                ////UpdateRigVisibilityInInspector();
            }
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

        private void CalculateBoundingBoxSizeAndCenter()
        {
            // Get size from child MeshComponent
            var bbox = this.meshComponent?.Model?.BoundingBox;

            if (bbox.HasValue)
            {
                this.boundingBoxCenter = bbox.Value.Center;
                this.boundingBoxSize = bbox.Value.HalfExtent * 2;
            }
            else
            {
                this.boundingBoxCenter = this.boxCollider3D.Offset;
                this.boundingBoxSize = this.boxCollider3D.Size;
            }

            this.boundingBoxSize += this.boxPadding;
        }

        private Vector3[] GetCornerPositionsFromBounds()
        {
            this.CalculateBoundingBoxSizeAndCenter();

            var boundsCorners = new Vector3[8];

            // Permutate all axes using minCorner and maxCorner.
            Vector3 minCorner = this.boundingBoxCenter - (this.boundingBoxSize / 2);
            Vector3 maxCorner = this.boundingBoxCenter + (this.boundingBoxSize / 2);
            for (int c = 0; c < boundsCorners.Length; c++)
            {
                boundsCorners[c] = new Vector3(
                    (c & (1 << 0)) == 0 ? minCorner[0] : maxCorner[0],
                    (c & (1 << 1)) == 0 ? minCorner[1] : maxCorner[1],
                    (c & (1 << 2)) == 0 ? minCorner[2] : maxCorner[2]);
            }

            return boundsCorners;
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

        private Vector3[] CalculateFaceCenters(Vector3[] boundsCorners)
        {
            var faceCenters = new Vector3[6];

            faceCenters[0] = this.boundingBoxCenter + (Vector3.Left * this.boundingBoxSize.X / 2);
            faceCenters[1] = this.boundingBoxCenter + (Vector3.Right * this.boundingBoxSize.X / 2);
            faceCenters[2] = this.boundingBoxCenter + (Vector3.Up * this.boundingBoxSize.Y / 2);
            faceCenters[3] = this.boundingBoxCenter + (Vector3.Down * this.boundingBoxSize.Y / 2);
            faceCenters[4] = this.boundingBoxCenter + (Vector3.Backward * this.boundingBoxSize.Z / 2);
            faceCenters[5] = this.boundingBoxCenter + (Vector3.Forward * this.boundingBoxSize.Z / 2);

            return faceCenters;
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
            var faceCenters = this.CalculateFaceCenters(boundsCorners);
            var edgeCenters = this.CalculateEdgeCenters(boundsCorners);
            var edgeAxes = this.CalculateAxisTypes();

            // Add corners
            if (this.showScaleHandles)
            {
                for (int i = 0; i < boundsCorners.Length; ++i)
                {
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
                        .AddComponent(new StaticBody3D() { CollisionCategories = this.CollisionCategory, IsSensor = true })
                        .AddComponent(new NearInteractionGrabbable());

                    this.rigRootEntity.AddChild(corner);

                    Entity cornerVisual = new Entity("visuals")
                        .AddComponent(new Transform3D())
                        .AddComponent(new CubeMesh())
                        .AddComponent(new MeshRenderer())
                        .AddComponent(new MaterialComponent());

                    corner.AddChild(cornerVisual);

                    this.ApplyMaterialToAllComponents(cornerVisual, this.handleMaterial);

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
            }

            // Add face balls
            bool[] showScaleHandle = { this.ShowXScaleHandle, this.ShowYScaleHandle, this.ShowZScaleHandle };
            for (int i = 0; i < faceCenters.Length; ++i)
            {
                if (!showScaleHandle[i / 2])
                {
                    continue;
                }

                Transform3D faceTransform = new Transform3D()
                {
                    LocalPosition = faceCenters[i],
                };

                Entity face = new Entity($"face_{i}")
                    .AddComponent(faceTransform)
                    .AddComponent(new BoxCollider3D()
                    {
                        Margin = 0.0001f,
                    })
                    .AddComponent(new StaticBody3D() { CollisionCategories = this.CollisionCategory, IsSensor = true })
                    .AddComponent(new NearInteractionGrabbable());

                this.rigRootEntity.AddChild(face);

                Entity faceVisual = new Entity("visuals")
                    .AddComponent(new Transform3D())
                    .AddComponent(new CubeMesh())
                    .AddComponent(new MeshRenderer())
                    .AddComponent(new MaterialComponent());

                face.AddChild(faceVisual);

                this.ApplyMaterialToAllComponents(faceVisual, this.handleMaterial);

                var cornerHelper = new BoundingBoxHelper()
                {
                    Type = BoundingBoxHelperType.NonUniformScaleHandle,
                    AxisType = AxisType.None,
                    Entity = face,
                    Transform = faceTransform,
                    OppositeHandlePosition = faceCenters[((i % 2) == 0) ? (i + 1) : (i - 1)],
                };

                this.helpers.Add(face, cornerHelper);
            }

            // Add balls
            bool[] showRotationHandle = { this.ShowXRotationHandle, this.ShowYRotationHandle, this.ShowZRotationHandle };
            for (int i = 0; i < edgeCenters.Length; ++i)
            {
                if (!showRotationHandle[(int)edgeAxes[i] - 1])
                {
                    continue;
                }

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
                    .AddComponent(new StaticBody3D() { CollisionCategories = this.CollisionCategory, IsSensor = true })
                    .AddComponent(new NearInteractionGrabbable());

                this.rigRootEntity.AddChild(midpoint);

                Entity midpointVisual = new Entity("visuals")
                    .AddComponent(new Transform3D())
                    .AddComponent(new SphereMesh())
                    .AddComponent(new MeshRenderer())
                    .AddComponent(new MaterialComponent());

                midpoint.AddChild(midpointVisual);

                this.ApplyMaterialToAllComponents(midpoint, this.handleMaterial);

                var midpointHelper = new BoundingBoxHelper()
                {
                    Type = BoundingBoxHelperType.RotationHandle,
                    AxisType = edgeAxes[i],
                    Entity = midpoint,
                    Transform = midpointTransform,
                };

                this.helpers.Add(midpoint, midpointHelper);
            }

            // Add links
            if (this.showWireframe)
            {
                for (int i = 0; i < edgeCenters.Length; ++i)
                {
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
                        .AddComponent(new Transform3D());

                    if (this.wireframeMaterial != null)
                    {
                        linkVisual
                            .AddComponent(new MeshRenderer())
                            .AddComponent(new MaterialComponent()
                            {
                                Material = this.wireframeMaterial,
                            });

                        switch (this.wireframeShape)
                        {
                            case WireframeType.Cubic:
                                linkVisual.AddComponent(new CubeMesh());
                                break;
                            case WireframeType.Cylindrical:
                                linkVisual.AddComponent(new CylinderMesh());
                                break;
                        }
                    }

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
            }

            this.helpersList = this.helpers.Values.ToList();
        }

        private void AddBoxDisplay()
        {
            if (this.boxMaterial != null)
            {
                this.boxDisplay = new Entity($"boxDisplay")
                    .AddComponent(new Transform3D()
                    {
                        LocalPosition = this.boundingBoxCenter,
                        LocalScale = this.boundingBoxSize,
                    })
                    .AddComponent(new CubeMesh())
                    .AddComponent(new MeshRenderer())
                    .AddComponent(new MaterialComponent());

                this.ApplyMaterialToAllComponents(this.boxDisplay, this.boxMaterial);

                this.rigRootEntity.AddChild(this.boxDisplay);
            }
        }

        private void ApplyMaterialToAllComponents(Entity root, Material material)
        {
            if (material != null)
            {
                MaterialComponent[] components = root.FindComponentsInChildren<MaterialComponent>().ToArray();

                for (int i = 0; i < components.Length; i++)
                {
                    components[i].Material = material;
                }
            }
        }

        private void UpdateRigHandles()
        {
            for (int i = 0; i < this.helpersList.Count; i++)
            {
                var helper = this.helpersList[i];

                switch (helper.Type)
                {
                    case BoundingBoxHelperType.WireframeLink:
                        float scaleX = this.LinkScale;
                        float scaleY = this.transform.WorldTransform.Scale.Y;
                        float scaleZ = this.LinkScale;

                        switch (helper.AxisType)
                        {
                            case AxisType.X:
                                scaleY *= this.boundingBoxSize.X;
                                scaleX *= this.transform.WorldTransform.Scale.X / this.transform.WorldTransform.Scale.Y;
                                break;
                            case AxisType.Y:
                                scaleY *= this.boundingBoxSize.Y;
                                scaleX *= this.transform.WorldTransform.Scale.X / this.transform.WorldTransform.Scale.Z;
                                scaleZ *= this.transform.WorldTransform.Scale.Z / this.transform.WorldTransform.Scale.X;
                                break;
                            case AxisType.Z:
                                scaleY *= this.boundingBoxSize.Z;
                                scaleZ *= this.transform.WorldTransform.Scale.Z / this.transform.WorldTransform.Scale.Y;
                                break;
                        }

                        helper.Transform.Scale = new Vector3(scaleX, scaleY, scaleZ);
                        break;

                    case BoundingBoxHelperType.RotationHandle:
                        helper.Transform.Scale = Vector3.One * this.RotationHandleScale;
                        break;

                    case BoundingBoxHelperType.ScaleHandle:
                    case BoundingBoxHelperType.NonUniformScaleHandle:
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

                this.ApplyMaterialToAllComponents(this.currentHandle.Entity, this.handleGrabbedMaterial);
                this.ApplyMaterialToAllComponents(this.boxDisplay, this.boxGrabbedMaterial);

                this.currentCursor = eventData.Cursor;
                this.initialGrabPoint = eventData.Position;
                this.transformOnGrabStart = this.transform.WorldTransform;

                switch (this.currentHandle.Type)
                {
                    case BoundingBoxHelperType.RotationHandle:
                        var axis = this.currentHandle.GetRotationAxis(this.transform.WorldTransform);
                        this.currentRotationAxis = Vector3.Normalize(axis);

                        this.RotateStarted?.Invoke(this, EventArgs.Empty);
                        break;

                    case BoundingBoxHelperType.ScaleHandle:
                        this.grabOppositeCorner = Vector3.TransformCoordinate(this.currentHandle.OppositeHandlePosition, this.transform.WorldTransform);
                        this.grabDiagonalDirection = Vector3.Normalize(this.currentHandle.Transform.Position - this.grabOppositeCorner);

                        this.ScaleStarted?.Invoke(this, EventArgs.Empty);
                        break;

                    case BoundingBoxHelperType.NonUniformScaleHandle:
                        this.grabOppositeCorner = Vector3.TransformCoordinate(this.currentHandle.OppositeHandlePosition, this.transform.WorldTransform);
                        this.grabDiagonalDirection = Vector3.Normalize(this.currentHandle.Transform.Position - this.grabOppositeCorner);

                        this.ScaleStarted?.Invoke(this, EventArgs.Empty);
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
                    {
                        float initialDist = Vector3.Dot(this.initialGrabPoint - this.grabOppositeCorner, this.grabDiagonalDirection);
                        float currentDist = Vector3.Dot(currentGrabPoint - this.grabOppositeCorner, this.grabDiagonalDirection);
                        float scaleFactor = 1.0f + ((currentDist - initialDist) / initialDist);

                        this.transform.Scale = this.transformOnGrabStart.Scale * scaleFactor;
                        this.transform.Position = this.grabOppositeCorner + (scaleFactor * (this.transformOnGrabStart.Translation - this.grabOppositeCorner));

                        this.UpdateRigHandles();
                        break;
                    }

                    case BoundingBoxHelperType.NonUniformScaleHandle:
                    {
                        float initialDist = Vector3.Dot(this.initialGrabPoint - this.grabOppositeCorner, this.grabDiagonalDirection);
                        float currentDist = Vector3.Dot(currentGrabPoint - this.grabOppositeCorner, this.grabDiagonalDirection);
                        float scaleFactor = 1.0f + ((currentDist - initialDist) / initialDist);

                        Vector3 localScale = this.transform.LocalScale;
                        this.transform.Scale = this.transformOnGrabStart.Scale * scaleFactor;
                        if (this.currentHandle.OppositeHandlePosition.X != 0.0f)
                        {
                            this.transform.LocalScale = new Vector3(this.transform.LocalScale.X, localScale.Y, localScale.Z);
                        }
                        else if (this.currentHandle.OppositeHandlePosition.Y != 0.0f)
                        {
                            this.transform.LocalScale = new Vector3(localScale.X, this.transform.LocalScale.Y, localScale.Z);
                        }
                        else if (this.currentHandle.OppositeHandlePosition.Z != 0.0f)
                        {
                            this.transform.LocalScale = new Vector3(localScale.X, localScale.Y, this.transform.LocalScale.Z);
                        }

                        this.transform.Position = this.grabOppositeCorner + (scaleFactor * (this.transformOnGrabStart.Translation - this.grabOppositeCorner));

                        this.UpdateRigHandles();

                        break;
                    }
                }

                eventData.SetHandled();
            }
        }

        private void PointerHandler_OnPointerUp(object sender, MixedRealityPointerEventData eventData)
        {
            if (this.currentCursor == eventData.Cursor)
            {
                this.ApplyMaterialToAllComponents(this.currentHandle.Entity, this.handleMaterial);
                this.ApplyMaterialToAllComponents(this.boxDisplay, this.boxMaterial);

                switch (this.currentHandle.Type)
                {
                    case BoundingBoxHelperType.RotationHandle:
                        this.RotateStopped?.Invoke(this, EventArgs.Empty);
                        break;
                    case BoundingBoxHelperType.ScaleHandle:
                        this.ScaleStopped?.Invoke(this, EventArgs.Empty);
                        break;
                }

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
