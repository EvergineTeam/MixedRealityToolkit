// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using System;
using System.Collections.Generic;
using System.Linq;
using Evergine.Common.Attributes;
using Evergine.Components.Graphics3D;
using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.Framework.Physics3D;
using Evergine.Framework.Prefabs;
using Evergine.Mathematics;
using Evergine.MRTK.Base.EventDatum.Input;
using Evergine.MRTK.Emulation;
using Evergine.MRTK.SDK.Features.Input.Handlers;
using Evergine.MRTK.Services.InputSystem;

namespace Evergine.MRTK.SDK.Features.UX.Components.BoundingBox
{
    /// <summary>
    /// BoundingBox allows to transform objects (rotate and scale) and draws a cube around the object to visualize
    /// the possibility of user triggered transform manipulation.
    /// </summary>
    public class BoundingBox : Behavior
    {
        private static readonly string RIG_ROOT_NAME = "rigRoot";

        [BindComponent]
        private Transform3D transform = null;

        [BindComponent(isRequired: false)]
        private BoxCollider3D boxCollider3D = null;

        /// <summary>
        /// Gets or sets a value indicating whether the bounding collider should be calculated automatically
        /// based on <see cref="MeshComponent"/> components found in from owner's hierarchy.
        /// </summary>
        [RenderProperty(Tooltip = "Whether the bounding collider should be calculated automatically based on MeshComponent components found in from owner's hierarchy.")]
        public bool AutoCalculate { get; set; } = false;

        /// <summary>
        /// Gets or sets the scale applied to the scale handles.
        /// </summary>
        [RenderProperty(Tooltip = "Scale applied to the scale handles.")]
        public float ScaleHandleScale
        {
            get => this.scaleHandleScale;

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
        [RenderProperty(Tooltip = "Scale applied to the rotation handles.")]
        public float RotationHandleScale
        {
            get => this.rotationHandleScale;

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
        [RenderProperty(Tooltip = "Scale applied to the wireframe links.")]
        public float LinkScale
        {
            get => this.linkScale;

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
            get => this.boxPadding;

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
            get => this.boxMaterial;

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
            get => this.boxGrabbedMaterial;

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
        [RenderProperty(Tooltip = "Whether to show the wireframe links around the box or not.")]
        public bool ShowWireframe
        {
            get => this.showWireframe;

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
        /// Gets or sets a value indicating whether to show the uniform scale handles on the corners or not.
        /// </summary>
        [RenderProperty(Tooltip = "Whether to show the uniform scale handles on the corners or not.")]
        public bool ShowScaleHandles
        {
            get => this.showScaleHandles;

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
        /// Gets or sets a value indicating whether to show the scale handles on the faces for axis X or not.
        /// </summary>
        [RenderProperty(Tooltip = "Whether to show the scale handles on the faces for axis X or not.")]
        public bool ShowXScaleHandle
        {
            get => this.showXScaleHandle;

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
        /// Gets or sets a value indicating whether to show the scale handles on the faces for axis Y or not.
        /// </summary>
        [RenderProperty(Tooltip = "Whether to show the scale handles on the faces for axis Y or not.")]
        public bool ShowYScaleHandle
        {
            get => this.showYScaleHandle;

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
        /// Gets or sets a value indicating whether to show the scale handles on the faces for axis Z or not.
        /// </summary>
        [RenderProperty(Tooltip = "Whether to show the scale handles on the faces for axis Z or not.")]
        public bool ShowZScaleHandle
        {
            get => this.showZScaleHandle;

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
        /// Gets or sets a value indicating whether to show the rotation handle on the edges for axis X or not.
        /// </summary>
        [RenderProperty(Tooltip = "Whether to show the rotation handle on the edges for axis X or not.")]
        public bool ShowXRotationHandle
        {
            get => this.showXRotationHandle;

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
        /// Gets or sets a value indicating whether to show the rotation handle on the edges for axis X or not.
        /// </summary>
        [RenderProperty(Tooltip = "Whether to show the rotation handle on the edges for axis Y or not.")]
        public bool ShowYRotationHandle
        {
            get => this.showYRotationHandle;

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
        /// Gets or sets a value indicating whether to show the rotation handle on the edges for axis Z or not.
        /// </summary>
        [RenderProperty(Tooltip = "Whether to show the rotation handle on the edges for axis Z or not.")]
        public bool ShowZRotationHandle
        {
            get => this.showZRotationHandle;

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
        [RenderProperty(Tooltip = "The shape of the wireframe links.")]
        public WireframeType WireframeShape
        {
            get => this.wireframeShape;

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
        [RenderProperty(Tooltip = "The material applied to the wireframe links.")]
        public Material WireframeMaterial
        {
            get => this.wireframeMaterial;

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
        [RenderProperty(Tooltip = "The material applied to the handles when not in a grabbed state.")]
        public Material HandleMaterial
        {
            get => this.handleMaterial;

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
        [RenderProperty(Tooltip = "The material applied to the handles when in a grabbed state.")]
        public Material HandleGrabbedMaterial
        {
            get => this.handleGrabbedMaterial;

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
        /// Gets or sets the prefab used to display scale handles in corners. If not set, boxes will be displayed instead.
        /// </summary>
        [RenderProperty(Tooltip = "The prefab used to display scale handles in corners. If not set, boxes will be displayed instead.")]
        public Prefab ScaleHandlePrefab
        {
            get => this.scaleHandlePrefab;
            set
            {
                if (this.scaleHandlePrefab != value)
                {
                    this.scaleHandlePrefab = value;
                    this.CreateRig();
                }
            }
        }

        private Prefab scaleHandlePrefab;

        /// <summary>
        /// Gets or sets the prefab used to display rotation handles in edges. If not set, spheres will be displayed instead.
        /// </summary>
        [RenderProperty(Tooltip = "The prefab used to display rotation handles in edges. If not set, spheres will be displayed instead.")]
        public Prefab RotationHandlePrefab
        {
            get => this.rotationHandlePrefab;
            set
            {
                if (this.rotationHandlePrefab != value)
                {
                    this.rotationHandlePrefab = value;
                    this.CreateRig();
                }
            }
        }

        private Prefab rotationHandlePrefab;

        /// <summary>
        /// Gets or sets the prefab used to display scale handles in faces. If not set, spheres will be displayed instead.
        /// </summary>
        [RenderProperty(Tooltip = "The prefab used to display rotation handles in edges. If not set, spheres will be displayed instead.")]
        public Prefab FaceScaleHandlePrefab
        {
            get => this.faceScaleHandlePrefab;
            set
            {
                if (this.faceScaleHandlePrefab != value)
                {
                    this.faceScaleHandlePrefab = value;
                    this.CreateRig();
                }
            }
        }

        private Prefab faceScaleHandlePrefab;

        /// <summary>
        /// Gets or sets the collision category of the handles.
        /// </summary>
        [RenderProperty(Tooltip = "The collision category of the handles")]

        public CollisionCategory3D CollisionCategory
        {
            get => this.collisionCategory3D;

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
        /// Gets or sets a value indicating whether the handles should keep their apparent size with respect to the camera.
        /// </summary>
        [RenderProperty(Tooltip = "Whether the handles should keep their apparent size with respect to the camera.")]
        public bool MaintainHandlesApparentSize
        {
            get => this.maintainHandlesApparentSize;

            set
            {
                if (this.maintainHandlesApparentSize != value)
                {
                    this.maintainHandlesApparentSize = value;
                    this.CreateRig();
                }
            }
        }

        private bool maintainHandlesApparentSize = false;

        /// <summary>
        /// Event fired when interaction with a rotation handle starts.
        /// </summary>
        public event EventHandler<BoundingBoxManipulationEventArgs> RotateStarted;

        /// <summary>
        /// Event fired when interaction with a rotation handle is updated.
        /// </summary>
        public event EventHandler<BoundingBoxManipulationEventArgs> RotateUpdated;

        /// <summary>
        /// Event fired when interaction with a rotation handle stops.
        /// </summary>
        public event EventHandler<BoundingBoxManipulationEventArgs> RotateStopped;

        /// <summary>
        /// Event fired when interaction with a scale handle starts.
        /// </summary>
        public event EventHandler<BoundingBoxManipulationEventArgs> ScaleStarted;

        /// <summary>
        /// Event fired when interaction with a scale handle is updated.
        /// </summary>
        public event EventHandler<BoundingBoxManipulationEventArgs> ScaleUpdated;

        /// <summary>
        /// Event fired when interaction with a scale handle stops.
        /// </summary>
        public event EventHandler<BoundingBoxManipulationEventArgs> ScaleStopped;

        private Entity rigRootEntity;

        private Dictionary<Entity, BoundingBoxHelper> helpers;
        private List<BoundingBoxHelper> helpersList;
        private Entity boxDisplay;

        private Vector3 boundingBoxCenter;
        private Vector3 boundingBoxSize;

        // Interaction variables
        private Cursor currentCursor;
        private BoundingBoxHelper currentHandle;

        private Vector3 initialGrabPoint;
        private Matrix4x4 transformOnGrabStart;

        private Vector3 grabOppositeCorner;
        private Vector3 grabDiagonalDirection;
        private Vector3 currentRotationAxis;

        private void AdjustBoundingToChildren()
        {
            Mathematics.BoundingBox? boundingBox = null;

            MeshComponent[] children = this.Owner.FindComponentsInChildren<MeshComponent>(isExactType: false).ToArray();
            foreach (MeshComponent meshComponent in children)
            {
                var bbox = meshComponent?.BoundingBox;
                if (bbox != null)
                {
                    var meshTransform = meshComponent.Owner.FindComponent<Transform3D>();
                    var localToThis = meshTransform.WorldTransform * this.transform.WorldInverseTransform;
                    Mathematics.BoundingBox b = bbox.Value;
                    b.Transform(localToThis);
                    bbox = b;

                    if (boundingBox == null)
                    {
                        // Assign this bounding box
                        boundingBox = bbox;
                    }
                    else
                    {
                        // Grow the boundingbox
                        boundingBox = new Evergine.Mathematics.BoundingBox(Vector3.Min(boundingBox.Value.Min, bbox.Value.Min), Vector3.Max(boundingBox.Value.Max, bbox.Value.Max));
                    }
                }
            }

            if (boundingBox != null)
            {
                this.boxCollider3D.Size = boundingBox.Value.HalfExtent * 2;
                this.boxCollider3D.Offset = boundingBox.Value.Center;
                var bounding = this.Owner.FindComponent<MeshComponent>(isExactType: false)?.BoundingBox;
                if (bounding != null)
                {
                    this.boxCollider3D.Size /= bounding.Value.HalfExtent * 2;
                    this.boxCollider3D.Offset -= bounding.Value.Center;
                }
            }
        }

        /// <inheritdoc/>
        protected override bool OnAttached()
        {
            if (!base.OnAttached())
            {
                return false;
            }

            if (this.boxCollider3D == null)
            {
                this.boxCollider3D = new BoxCollider3D();
                this.Owner.AddComponent(this.boxCollider3D);
            }

            return true;
        }

        /// <inheritdoc/>
        protected override void OnActivated()
        {
            base.OnActivated();

            this.transform.ScaleChanged += this.Transform_ScaleChanged;
            this.transform.LocalScaleChanged += this.Transform_ScaleChanged;

            this.InternalCreateRig();
        }

        /// <inheritdoc/>
        protected override void OnDeactivated()
        {
            base.OnDeactivated();

            this.transform.ScaleChanged -= this.Transform_ScaleChanged;
            this.transform.LocalScaleChanged -= this.Transform_ScaleChanged;

            this.DestroyRig();
        }

        private void Transform_ScaleChanged(object sender, EventArgs e)
        {
            this.UpdateRigHandles();
        }

        /// <inheritdoc/>
        protected override void Update(TimeSpan gameTime)
        {
            if (this.maintainHandlesApparentSize)
            {
                this.UpdateRigHandles();
            }
        }

        /// <summary>
        /// Destroys and re-creates the rig around the bounding box.
        /// </summary>
        /// <returns><see langword="true"/> if the rig can be re-created at that moment.</returns>
        public bool CreateRig()
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
                this.AddBoxDisplay();
                this.UpdateRigHandles();
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
            if (this.AutoCalculate)
            {
                this.AdjustBoundingToChildren();
            }

            this.boundingBoxCenter = this.boxCollider3D.Offset;
            this.boundingBoxSize = this.boxCollider3D.Size;

            MeshComponent meshComponent = this.Owner.FindComponent<MeshComponent>(isExactType: false);
            var bounding = meshComponent?.BoundingBox;
            if (bounding != null)
            {
                this.boundingBoxSize *= bounding.Value.HalfExtent * 2;
                this.boundingBoxCenter += bounding.Value.Center;
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

        private Vector3[] CalculateCornerRotations()
        {
            var cornerRotations = new Vector3[8];

            cornerRotations[0] = new Vector3(0, 0, 0);
            cornerRotations[1] = new Vector3(0, 0, 1);
            cornerRotations[2] = new Vector3(1, 0, 0);
            cornerRotations[3] = new Vector3(1, 0, 1);
            cornerRotations[4] = new Vector3(0, 1, 0);
            cornerRotations[5] = new Vector3(0, 3, 1);
            cornerRotations[6] = new Vector3(1, 1, 0);
            cornerRotations[7] = new Vector3(1, 3, 1);

            for (int i = 0; i < cornerRotations.Length; i++)
            {
                cornerRotations[i] *= (float)Math.PI / 2;
            }

            return cornerRotations;
        }

        private Vector3[] CalculateFaceCenters()
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

        private Vector3[] CalculateFaceRotations()
        {
            var faceRotations = new Vector3[6];

            faceRotations[0] = new Vector3(0, 0, 1);
            faceRotations[1] = new Vector3(0, 0, -1);
            faceRotations[2] = new Vector3(0, 1, 0);
            faceRotations[3] = new Vector3(0, -1, 0);
            faceRotations[4] = new Vector3(1, 0, 0);
            faceRotations[5] = new Vector3(-1, 0, 0);

            for (int i = 0; i < faceRotations.Length; i++)
            {
                faceRotations[i] *= (float)Math.PI / 2;
            }

            return faceRotations;
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

        private Vector3[] CalculateEdgeRotations()
        {
            var edgeRotations = new Vector3[12];

            edgeRotations[0] = new Vector3(0, 0, 1);
            edgeRotations[1] = new Vector3(0, 1, 2);
            edgeRotations[2] = new Vector3(0, 0, 3);
            edgeRotations[3] = new Vector3(0, 3, 0);

            edgeRotations[4] = new Vector3(3, 1, 0);
            edgeRotations[5] = new Vector3(0, 1, 0);
            edgeRotations[6] = new Vector3(1, 1, 0);
            edgeRotations[7] = new Vector3(2, 1, 0);

            edgeRotations[8] = new Vector3(0, 1, 1);
            edgeRotations[9] = new Vector3(3, 1, 1);
            edgeRotations[10] = new Vector3(1, 1, 1);
            edgeRotations[11] = new Vector3(2, 1, 1);

            for (int i = 0; i < edgeRotations.Length; i++)
            {
                edgeRotations[i] *= (float)Math.PI / 2;
            }

            return edgeRotations;
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
            // Add corners
            var cornerCenters = this.GetCornerPositionsFromBounds();
            var cornerRotations = this.CalculateCornerRotations();

            if (this.showScaleHandles)
            {
                for (int i = 0; i < cornerCenters.Length; ++i)
                {
                    this.CreateHandle(
                        cornerCenters[i],
                        cornerCenters[7 - i],
                        cornerRotations[i],
                        this.ScaleHandlePrefab,
                        BoundingBoxHelperType.ScaleHandle,
                        AxisType.None);
                }
            }

            // Add face balls
            var faceCenters = this.CalculateFaceCenters();
            var faceRotations = this.CalculateFaceRotations();

            bool[] showScaleHandle = { this.ShowXScaleHandle, this.ShowYScaleHandle, this.ShowZScaleHandle };
            for (int i = 0; i < faceCenters.Length; ++i)
            {
                if (!showScaleHandle[i / 2])
                {
                    continue;
                }

                this.CreateHandle(
                    faceCenters[i],
                    faceCenters[((i % 2) == 0) ? (i + 1) : (i - 1)],
                    faceRotations[i],
                    this.FaceScaleHandlePrefab,
                    BoundingBoxHelperType.NonUniformScaleHandle,
                    (AxisType)((i / 2) + 1));
            }

            // Add midpoints
            var edgeCenters = this.CalculateEdgeCenters(cornerCenters);
            var edgeRotations = this.CalculateEdgeRotations();
            var edgeAxes = this.CalculateAxisTypes();

            bool[] showRotationHandle = { this.ShowXRotationHandle, this.ShowYRotationHandle, this.ShowZRotationHandle };
            for (int i = 0; i < edgeCenters.Length; ++i)
            {
                if (!showRotationHandle[(int)edgeAxes[i] - 1])
                {
                    continue;
                }

                this.CreateHandle(
                    edgeCenters[i],
                    Vector3.Zero,
                    edgeRotations[i],
                    this.RotationHandlePrefab,
                    BoundingBoxHelperType.RotationHandle,
                    edgeAxes[i]);
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

                    var link = new Entity($"link_{i}")
                        .AddComponent(linkTransform);

                    this.rigRootEntity.AddChild(link);

                    var linkVisual = new Entity("visuals")
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
                        BaseEntity = link,
                        Transform = linkTransform,
                    };

                    this.helpers.Add(link, linkHelper);
                }
            }

            this.helpersList = this.helpers.Values.ToList();
        }

        private void CreateHandle(Vector3 position, Vector3 oppositePosition, Vector3 visualRotation, Prefab prefab, BoundingBoxHelperType bbhType, AxisType axisType)
        {
            var handleTransform = new Transform3D()
            {
                LocalPosition = position,
            };

            var handle = new Entity()
                .AddComponent(handleTransform);

            this.rigRootEntity.AddChild(handle);

            if (prefab != null)
            {
                // Instantiate prefab
                var prefabInstance = prefab.Instantiate();

                handle.AddChild(prefabInstance);
            }
            else
            {
                Component collider;
                Component mesh;

                if (bbhType == BoundingBoxHelperType.RotationHandle)
                {
                    collider = new SphereCollider3D()
                    {
                        Margin = 0.0001f,
                    };

                    mesh = new SphereMesh();
                }
                else
                {
                    collider = new BoxCollider3D()
                    {
                        Margin = 0.0001f,
                    };

                    mesh = new CubeMesh();
                }

                handle
                    .AddComponent(collider)
                    .AddComponent(new StaticBody3D()
                    {
                        CollisionCategories = this.CollisionCategory,
                        IsSensor = true,
                    })
                    .AddComponent(new NearInteractionGrabbable());

                var handleVisual = new Entity("visuals")
                    .AddComponent(new Transform3D())
                    .AddComponent(mesh)
                    .AddComponent(new MeshRenderer())
                    .AddComponent(new MaterialComponent());

                handle.AddChild(handleVisual);
            }

            // Apply material
            this.ApplyMaterialToAllComponents(handle, this.handleMaterial);

            // Set rotation
            var childTransform = handle.ChildEntities.First().FindComponent<Transform3D>();
            if (childTransform != null)
            {
                childTransform.LocalRotation = visualRotation;
            }

            // Register helper object
            var helperTargetEntity = handle.FindComponentInChildren<NearInteractionGrabbable>()?.Owner;
            if (helperTargetEntity != null)
            {
                var handleHelper = new BoundingBoxHelper()
                {
                    Type = bbhType,
                    AxisType = axisType,
                    BaseEntity = handle,
                    Transform = handleTransform,
                    OppositeHandlePosition = oppositePosition,
                };

                this.helpers.Add(helperTargetEntity, handleHelper);
            }
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
            if (material != null && root != null)
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
            var activeCamera = this.Managers.RenderManager.ActiveCamera3D;
            for (int i = 0; i < this.helpersList.Count; i++)
            {
                var helper = this.helpersList[i];

                var scaleFactorDueToCameraPosition = 1.0f;
                if (this.maintainHandlesApparentSize && activeCamera != null)
                {
                    scaleFactorDueToCameraPosition = (activeCamera.Position - helper.Transform.Position).Length() * activeCamera.FieldOfView;
                }

                scaleFactorDueToCameraPosition = MathHelper.Max(scaleFactorDueToCameraPosition, 0.01f);

                switch (helper.Type)
                {
                    case BoundingBoxHelperType.WireframeLink:
                        float scaleX = this.LinkScale * scaleFactorDueToCameraPosition;
                        float scaleY = this.transform.WorldTransform.Scale.Y;
                        float scaleZ = this.LinkScale * scaleFactorDueToCameraPosition;

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
                        helper.Transform.Scale = Vector3.One * this.RotationHandleScale * scaleFactorDueToCameraPosition;
                        break;

                    case BoundingBoxHelperType.ScaleHandle:
                    case BoundingBoxHelperType.NonUniformScaleHandle:
                        helper.Transform.Scale = Vector3.One * this.ScaleHandleScale * scaleFactorDueToCameraPosition;
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
                if (this.helpers.TryGetValue(eventData.CurrentTarget, out var handle))
                {
                    this.currentHandle = handle;

                    this.ApplyMaterialToAllComponents(this.currentHandle.BaseEntity, this.handleGrabbedMaterial);
                    this.ApplyMaterialToAllComponents(this.boxDisplay, this.boxGrabbedMaterial);

                    this.currentCursor = eventData.Cursor;
                    this.initialGrabPoint = eventData.Position;
                    this.transformOnGrabStart = this.transform.WorldTransform;

                    switch (this.currentHandle.Type)
                    {
                        case BoundingBoxHelperType.RotationHandle:
                            var axis = this.currentHandle.GetRotationAxis(this.transform.WorldTransform);
                            this.currentRotationAxis = Vector3.Normalize(axis);

                            this.RotateStarted?.Invoke(this, new BoundingBoxManipulationEventArgs()
                            {
                                Handle = this.currentHandle,
                            });
                            break;

                        case BoundingBoxHelperType.ScaleHandle:
                            this.grabOppositeCorner = Vector3.TransformCoordinate(this.currentHandle.OppositeHandlePosition, this.transform.WorldTransform);
                            this.grabDiagonalDirection = Vector3.Normalize(this.currentHandle.Transform.Position - this.grabOppositeCorner);

                            this.ScaleStarted?.Invoke(this, new BoundingBoxManipulationEventArgs()
                            {
                                Handle = this.currentHandle,
                            });
                            break;

                        case BoundingBoxHelperType.NonUniformScaleHandle:
                            this.grabOppositeCorner = Vector3.TransformCoordinate(this.currentHandle.OppositeHandlePosition, this.transform.WorldTransform);
                            this.grabDiagonalDirection = Vector3.Normalize(this.currentHandle.Transform.Position - this.grabOppositeCorner);

                            this.ScaleStarted?.Invoke(this, new BoundingBoxManipulationEventArgs()
                            {
                                Handle = this.currentHandle,
                            });
                            break;
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
                var currentGrabPoint = eventData.Position;

                switch (this.currentHandle.Type)
                {
                    case BoundingBoxHelperType.RotationHandle:
                        {
                            var initialDir = this.ProjectOnPlane(this.transform.Position, this.currentRotationAxis, this.initialGrabPoint);
                            var currentDir = this.ProjectOnPlane(this.transform.Position, this.currentRotationAxis, currentGrabPoint);

                            var dir1 = Vector3.Normalize(initialDir - this.transform.Position);
                            var dir2 = Vector3.Normalize(currentDir - this.transform.Position);

                            // Check that the two vectors are not the same, workaround for a bug in Quaternion.CreateFromTwoVectors
                            if (Vector3.DistanceSquared(dir1, dir2) > MathHelper.Epsilon)
                            {
                                var rotation = Quaternion.CreateFromTwoVectors(dir1, dir2);
                                this.transform.Orientation = rotation * this.transformOnGrabStart.Orientation;

                                var cross = Vector3.Cross(dir1, dir2);
                                var dir = Vector3.Dot(cross, this.currentRotationAxis);

                                this.RotateUpdated?.Invoke(this, new BoundingBoxManipulationEventArgs()
                                {
                                    Handle = this.currentHandle,
                                    Value = Vector3.Angle(dir1, dir2) * Math.Sign(dir),
                                });
                            }

                            break;
                        }

                    case BoundingBoxHelperType.ScaleHandle:
                    case BoundingBoxHelperType.NonUniformScaleHandle:
                        {
                            float initialDist = Vector3.Dot(this.initialGrabPoint - this.grabOppositeCorner, this.grabDiagonalDirection);
                            float currentDist = Vector3.Dot(currentGrabPoint - this.grabOppositeCorner, this.grabDiagonalDirection);
                            float scaleFactor = 1.0f + ((currentDist - initialDist) / initialDist);

                            Vector3 localScale = this.transform.LocalScale;

                            this.transform.Scale = this.transformOnGrabStart.Scale * scaleFactor;

                            if (this.currentHandle.Type == BoundingBoxHelperType.ScaleHandle)
                            {
                                this.transform.Position = this.grabOppositeCorner + (scaleFactor * (this.transformOnGrabStart.Translation - this.grabOppositeCorner));
                            }
                            else
                            {
                                if (this.currentHandle.AxisType == AxisType.X)
                                {
                                    this.transform.LocalScale = new Vector3(this.transform.LocalScale.X, localScale.Y, localScale.Z);
                                }
                                else if (this.currentHandle.AxisType == AxisType.Y)
                                {
                                    this.transform.LocalScale = new Vector3(localScale.X, this.transform.LocalScale.Y, localScale.Z);
                                }
                                else if (this.currentHandle.AxisType == AxisType.Z)
                                {
                                    this.transform.LocalScale = new Vector3(localScale.X, localScale.Y, this.transform.LocalScale.Z);
                                }

                                Vector3 oppositeCornerPos = Vector3.TransformCoordinate(this.currentHandle.OppositeHandlePosition, this.transform.WorldTransform);
                                this.transform.Position += this.grabOppositeCorner - oppositeCornerPos;
                            }

                            this.UpdateRigHandles();

                            this.ScaleUpdated?.Invoke(this, new BoundingBoxManipulationEventArgs()
                            {
                                Handle = this.currentHandle,
                                Value = scaleFactor,
                            });

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
                this.ApplyMaterialToAllComponents(this.currentHandle.BaseEntity, this.handleMaterial);
                this.ApplyMaterialToAllComponents(this.boxDisplay, this.boxMaterial);

                switch (this.currentHandle.Type)
                {
                    case BoundingBoxHelperType.RotationHandle:
                        this.RotateStopped?.Invoke(this, new BoundingBoxManipulationEventArgs()
                        {
                            Handle = this.currentHandle,
                        });
                        break;
                    case BoundingBoxHelperType.ScaleHandle:
                    case BoundingBoxHelperType.NonUniformScaleHandle:
                        this.ScaleStopped?.Invoke(this, new BoundingBoxManipulationEventArgs()
                        {
                            Handle = this.currentHandle,
                        });
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
