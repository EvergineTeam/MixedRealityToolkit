// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using Evergine.Common.Attributes;
using Evergine.Framework.Graphics;
using Evergine.Framework.Physics3D;
using Evergine.Framework.Prefabs;

namespace Evergine.MRTK.SDK.Features.UX.Components.AxisManipulationHandler
{
    /// <summary>
    /// A manipulation handler that restricts movement to a combination of axes.
    /// </summary>
    public partial class AxisManipulationHandler
    {
        /// <summary>
        /// Gets or sets the scale applied to the handles.
        /// </summary>
        [RenderProperty(Tooltip = "Scale applied to the handles.")]
        public float HandleScale
        {
            get => this.handleScale;

            set
            {
                if (this.handleScale != value)
                {
                    this.handleScale = value;
                    this.CreateRig();
                }
            }
        }

        private float handleScale = 0.02f;

        /// <summary>
        /// Gets or sets the prefab used for the center handle. If not set, a sphere will be displayed instead.
        /// </summary>
        [RenderProperty(Tooltip = "The prefab used for the center handle. If not set, a sphere will be displayed instead.")]
        public Prefab CenterHandlePrefab
        {
            get => this.centerHandlePrefab;
            set
            {
                if (this.centerHandlePrefab != value)
                {
                    this.centerHandlePrefab = value;
                    this.CreateRig();
                }
            }
        }

        private Prefab centerHandlePrefab;

        /// <summary>
        /// Gets or sets the prefab used for the axis handles. If not set, a box will be displayed instead.
        /// </summary>
        [RenderProperty(Tooltip = "The prefab used for the axis handles. If not set, a box will be displayed instead.")]
        public Prefab AxisHandlePrefab
        {
            get => this.axisHandlePrefab;
            set
            {
                if (this.axisHandlePrefab != value)
                {
                    this.axisHandlePrefab = value;
                    this.CreateRig();
                }
            }
        }

        private Prefab axisHandlePrefab;

        /// <summary>
        /// Gets or sets the prefab used for the plane handles. If not set, a box will be displayed instead.
        /// </summary>
        [RenderProperty(Tooltip = "The prefab used for the plane handles. If not set, a box will be displayed instead.")]
        public Prefab PlaneHandlePrefab
        {
            get => this.planeHandlePrefab;
            set
            {
                if (this.planeHandlePrefab != value)
                {
                    this.planeHandlePrefab = value;
                    this.CreateRig();
                }
            }
        }

        private Prefab planeHandlePrefab;

        /// <summary>
        /// Gets or sets the material applied to the center handles when not grabbed.
        /// </summary>
        [RenderProperty(Tooltip = "The material applied to the center handle when not grabbed. If set to null, no center handle will be displayed.")]
        public Material CenterHandleMaterial
        {
            get => this.centerHandleMaterial;

            set
            {
                if (this.centerHandleMaterial != value)
                {
                    this.centerHandleMaterial = value;
                    this.CreateRig();
                }
            }
        }

        private Material centerHandleMaterial;

        /// <summary>
        /// Gets or sets the material applied to the center handle when focused.
        /// </summary>
        [RenderProperty(Tooltip = "The material applied to the center handle when focused. If set to null, no change will occur when focused.")]
        public Material CenterHandleFocusedMaterial
        {
            get => this.centerHandleFocusedMaterial;

            set
            {
                if (this.centerHandleFocusedMaterial != value)
                {
                    this.centerHandleFocusedMaterial = value;
                    this.CreateRig();
                }
            }
        }

        private Material centerHandleFocusedMaterial;

        /// <summary>
        /// Gets or sets the material applied to the center handle when grabbed.
        /// </summary>
        [RenderProperty(Tooltip = "The material applied to the center handle when grabbed. If set to null, no change will occur when grabbed.")]
        public Material CenterHandleGrabbedMaterial
        {
            get => this.centerHandleGrabbedMaterial;

            set
            {
                if (this.centerHandleGrabbedMaterial != value)
                {
                    this.centerHandleGrabbedMaterial = value;
                    this.CreateRig();
                }
            }
        }

        private Material centerHandleGrabbedMaterial;

        /// <summary>
        /// Gets or sets the material applied to the axis handles when not grabbed.
        /// </summary>
        [RenderProperty(Tooltip = "The material applied to the axis handles when not grabbed. If set to null, no axis handles will be displayed.")]
        public Material AxisHandleMaterial
        {
            get => this.axisHandleMaterial;

            set
            {
                if (this.axisHandleMaterial != value)
                {
                    this.axisHandleMaterial = value;
                    this.CreateRig();
                }
            }
        }

        private Material axisHandleMaterial;

        /// <summary>
        /// Gets or sets the material applied to the axis handles when focused.
        /// </summary>
        [RenderProperty(Tooltip = "The material applied to the axis handles when focused. If set to null, no change will occur when focused.")]
        public Material AxisHandleFocusedMaterial
        {
            get => this.axisHandleFocusedMaterial;

            set
            {
                if (this.axisHandleFocusedMaterial != value)
                {
                    this.axisHandleFocusedMaterial = value;
                    this.CreateRig();
                }
            }
        }

        private Material axisHandleFocusedMaterial;

        /// <summary>
        /// Gets or sets the material applied to the axis handles when grabbed.
        /// </summary>
        [RenderProperty(Tooltip = "The material applied to the axis handles when grabbed. If set to null, no change will occur when grabbed.")]
        public Material AxisHandleGrabbedMaterial
        {
            get => this.axisHandleGrabbedMaterial;

            set
            {
                if (this.axisHandleGrabbedMaterial != value)
                {
                    this.axisHandleGrabbedMaterial = value;
                    this.CreateRig();
                }
            }
        }

        private Material axisHandleGrabbedMaterial;

        /// <summary>
        /// Gets or sets the material applied to the plane handles when not grabbed.
        /// </summary>
        [RenderProperty(Tooltip = "The material applied to the plane handles when not grabbed. If set to null, no plane handles will be displayed.")]
        public Material PlaneHandleMaterial
        {
            get => this.planeHandleMaterial;

            set
            {
                if (this.planeHandleMaterial != value)
                {
                    this.planeHandleMaterial = value;
                    this.CreateRig();
                }
            }
        }

        private Material planeHandleMaterial;

        /// <summary>
        /// Gets or sets the material applied to the plane handles when focused.
        /// </summary>
        [RenderProperty(Tooltip = "The material applied to the plane handles when focused. If set to null, no change will occur when focused.")]
        public Material PlaneHandleFocusedMaterial
        {
            get => this.planeHandleFocusedMaterial;

            set
            {
                if (this.planeHandleFocusedMaterial != value)
                {
                    this.planeHandleFocusedMaterial = value;
                    this.CreateRig();
                }
            }
        }

        private Material planeHandleFocusedMaterial;

        /// <summary>
        /// Gets or sets the material applied to the plane handles when grabbed.
        /// </summary>
        [RenderProperty(Tooltip = "The material applied to the plane handles when grabbed. If set to null, no change will occur when grabbed.")]
        public Material PlaneHandleGrabbedMaterial
        {
            get => this.planeHandleGrabbedMaterial;

            set
            {
                if (this.planeHandleGrabbedMaterial != value)
                {
                    this.planeHandleGrabbedMaterial = value;
                    this.CreateRig();
                }
            }
        }

        private Material planeHandleGrabbedMaterial;

        /// <summary>
        /// Gets or sets the collision category of the handles.
        /// </summary>
        [RenderProperty(Tooltip = "The collision category of the handles")]

        public CollisionCategory3D CollisionCategory
        {
            get => this.collisionCategory;

            set
            {
                if (this.collisionCategory != value)
                {
                    this.collisionCategory = value;
                    this.CreateRig();
                }
            }
        }

        private CollisionCategory3D collisionCategory = CollisionCategory3D.Cat1;

        /// <summary>
        /// Gets or sets a value indicating whether grabbing or focusing a plane handle causes the related axis handles to change material as well.
        /// </summary>
        [RenderProperty(Tooltip = "Whether grabbing or focusing a plane handle causes the related axis handles to change material as well.")]
        public bool PlaneHandlesActivateAxisHandles { get; set; }
    }
}
