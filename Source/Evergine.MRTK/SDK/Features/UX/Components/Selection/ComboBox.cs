// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using Evergine.Common.Attributes;
using Evergine.Components.Fonts;
using Evergine.Components.Graphics3D;
using Evergine.Components.WorkActions;
using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.Framework.Physics3D;
using Evergine.Framework.Services;
using Evergine.Mathematics;
using Evergine.MRTK.SDK.Features.UX.Components.Lists;
using Evergine.MRTK.SDK.Features.UX.Components.PressableButtons;
using System;

namespace Evergine.MRTK.SDK.Features.UX.Components.Selection
{
    /// <summary>
    /// Component to handle comboboxes.
    /// </summary>
    public class ComboBox : Component
    {
        private const float ArrowButtonSize = 0.032f;
        private const float TextPadding = 0.016f;

        [BindComponent(source: BindComponentSource.Children, tag: "PART_ComboBox_Root")]
        private PressableButton rootButton = null;

        [BindComponent(source: BindComponentSource.Children, tag: "PART_ComboBox_Root")]
        private BoxCollider3D rootCollider = null;

        [BindComponent(source: BindComponentSource.Children, tag: "PART_ComboBox_BackPlate")]
        private Transform3D backPlateTransform = null;

        [BindComponent(source: BindComponentSource.Children, tag: "PART_ComboBox_Arrow_Holder")]
        private Transform3D arrowHolderTransform = null;

        [BindComponent(source: BindComponentSource.Children, tag: "PART_ComboBox_Arrow_BackPlate")]
        private Transform3D arrowBackPlateTransform = null;

        [BindComponent(source: BindComponentSource.Children, tag: "PART_ComboBox_Arrow_Icon")]
        private Transform3D arrowIconTransform = null;

        [BindComponent(source: BindComponentSource.Children, tag: "PART_ComboBox_Arrow_Icon")]
        private MaterialComponent arrowMaterialComponent = null;

        [BindComponent(source: BindComponentSource.Children, tag: "PART_ComboBox_FrontPlate")]
        private Transform3D frontPlateTransform = null;

        [BindEntity(source: BindEntitySource.Children, tag: "PART_ComboBox_ItemsContainer")]
        private Entity itemsContainerEntity = null;

        [BindComponent(source: BindComponentSource.Children, tag: "PART_ComboBox_ItemsContainer")]
        private Transform3D itemsContainerTransform = null;

        [BindComponent(source: BindComponentSource.Children, tag: "PART_ComboBox_Text")]
        private Text3DMesh selectedItemTextMesh = null;

        [BindComponent(source: BindComponentSource.Children, tag: "PART_ComboBox_Items_ListView")]
        private ListView itemsListView = null;

        private DataAdapter itemsAdapter = null;
        private object selectedItem = null;
        private IWorkAction arrowAnimation;
        private bool pendingRefresh;
        private string placeholderText;
        private Material arrowMaterial = null;
        private Vector2 size = new Vector2(0.096f, ArrowButtonSize);
        private float maxItemsHeight = 0.06f;

        /// <summary>
        /// Raised when selected item changes.
        /// </summary>
        public event EventHandler SelectedItemChanged;

        /// <summary>
        /// Gets a value indicating whether popup is open.
        /// </summary>
        [IgnoreEvergine]
        [DontRenderProperty]
        public bool IsPopupOpen
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets items collection.
        /// </summary>
        [IgnoreEvergine]
        [DontRenderProperty]
        public DataAdapter DataSource
        {
            get => this.itemsAdapter;
            set
            {
                if (this.itemsAdapter != value)
                {
                    this.itemsAdapter = value;

                    if (this.IsAttached)
                    {
                        this.OnItemsUpdated();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets selected item.
        /// </summary>
        [IgnoreEvergine]
        [DontRenderProperty]
        public object SelectedItem
        {
            get => this.itemsListView.SelectedItem;
            set
            {
                if (this.selectedItem != value)
                {
                    this.selectedItem = value;
                    this.OnSelectedItemUpdated();
                }
            }
        }

        /// <summary>
        /// Gets or sets placeholder text. This will be displayed if no item
        /// is selected.
        /// </summary>
        public string PlaceholderText
        {
            get => this.placeholderText;
            set
            {
                if (this.placeholderText != value)
                {
                    this.placeholderText = value;
                    this.OnPlaceholderTextUpdated();
                }
            }
        }

        /// <summary>
        /// Gets or sets arrow entity.
        /// </summary>
        public Material ArrowMaterial
        {
            get => this.arrowMaterial;
            set
            {
                if (this.arrowMaterial != value)
                {
                    this.arrowMaterial = value;
                    this.OnArrowMaterialUpdated();
                }
            }
        }

        /// <summary>
        /// Gets or sets ComboBox size.
        /// </summary>
        public Vector2 Size
        {
            get => this.size;
            set
            {
                this.size = value;
                if (this.IsAttached)
                {
                    this.UpdateSize();
                }
            }
        }

        /// <summary>
        /// Gets or sets maximum items area height.
        /// </summary>
        public float MaxItemsHeight
        {
            get => this.maxItemsHeight;
            set
            {
                if (this.maxItemsHeight != value)
                {
                    this.maxItemsHeight = value;
                    this.OnMaxItemsHeightUpdated();
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

            this.itemsListView.HeaderEnabled = false;
            this.itemsListView.Columns = new ColumnDefinition[] { new ColumnDefinition { PercentageSize = 1.0f } };
            this.itemsListView.SelectedItemChanged += this.ItemsListView_SelectedChanged;
            this.OnItemsUpdated();

            return true;
        }

        /// <inheritdoc/>
        protected override void OnDetach()
        {
            base.OnDetach();
            this.itemsListView.SelectedItemChanged -= this.ItemsListView_SelectedChanged;
        }

        /// <inheritdoc/>
        protected override void OnActivated()
        {
            base.OnActivated();

            this.itemsContainerEntity.IsEnabled = false;
            this.rootButton.ButtonReleased += this.RootButton_ButtonReleased;

            if (this.pendingRefresh)
            {
                this.itemsListView.Refresh();
                this.pendingRefresh = false;
            }

            this.OnSelectedItemUpdated();
            this.OnArrowMaterialUpdated();
            this.UpdateSize();
        }

        /// <inheritdoc/>
        protected override void OnDeactivated()
        {
            base.OnDeactivated();

            this.arrowAnimation?.Cancel();
            this.rootButton.ButtonReleased -= this.RootButton_ButtonReleased;
        }

        private void RootButton_ButtonReleased(object sender, EventArgs args)
        {
            this.IsPopupOpen = !this.itemsContainerEntity.IsEnabled;
            this.itemsContainerEntity.IsEnabled = this.IsPopupOpen;

            this.arrowAnimation?.Cancel();
            this.arrowAnimation = new RotateTo3DWorkAction(
                this.arrowIconTransform.Owner,
                new Vector3(0, 0, MathHelper.ToRadians(this.IsPopupOpen ? 180 : 0)),
                TimeSpan.FromSeconds(0.3),
                EaseFunction.SineOutEase);
            this.arrowAnimation.Run();
        }

        private void OnItemsUpdated()
        {
            this.itemsListView.DataSource = this.itemsAdapter;

            if (this.itemsListView.IsAttached)
            {
                this.itemsListView.Refresh();
            }
            else
            {
                this.pendingRefresh = true;
            }
        }

        private void OnPlaceholderTextUpdated()
        {
            if (!this.IsAttached)
            {
                return;
            }

            this.selectedItemTextMesh.Text = this.SelectedItem?.ToString() ?? this.placeholderText;
        }

        private void OnSelectedItemUpdated()
        {
            if (this.itemsListView?.IsAttached == true && this.itemsListView.SelectedItem != this.selectedItem)
            {
                this.itemsListView.SelectedItem = this.selectedItem;
            }

            this.OnPlaceholderTextUpdated();
            this.SelectedItemChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ItemsListView_SelectedChanged(object sender, EventArgs args)
        {
            this.selectedItem = this.itemsListView.SelectedItem;
            this.OnSelectedItemUpdated();
        }

        private void OnArrowMaterialUpdated()
        {
            if (!this.IsAttached)
            {
                return;
            }

            this.arrowMaterialComponent.Material = this.arrowMaterial;
        }

        private void UpdateSize()
        {
            var backPlateScale = this.backPlateTransform.LocalScale;
            backPlateScale.X = this.size.X;
            backPlateScale.Y = this.size.Y;
            this.backPlateTransform.LocalScale = backPlateScale;

            var frontPlateScale = this.frontPlateTransform.LocalScale;
            frontPlateScale.X = this.size.X;
            frontPlateScale.Y = this.size.Y;
            this.frontPlateTransform.LocalScale = frontPlateScale;

            var rootColliderSize = this.rootCollider.Size;
            rootColliderSize.X = this.size.X;
            rootColliderSize.Y = this.size.Y;
            this.rootCollider.Size = rootColliderSize;

            var arrowHolderLocalPos = this.arrowHolderTransform.LocalPosition;
            arrowHolderLocalPos.X = (this.size.X / 2) - (ArrowButtonSize / 2);
            this.arrowHolderTransform.LocalPosition = arrowHolderLocalPos;

            var arrowBackPlateScale = this.arrowBackPlateTransform.LocalScale;
            arrowBackPlateScale.Y = this.size.Y - 0.002f; // padding
            this.arrowBackPlateTransform.LocalScale = arrowBackPlateScale;

            var selectedItemTextSize = this.selectedItemTextMesh.Size;
            selectedItemTextSize.X = this.size.X - ArrowButtonSize - TextPadding;
            this.selectedItemTextMesh.Size = selectedItemTextSize;

            var itemsListViewSize = this.itemsListView.Size;
            itemsListViewSize.X = this.size.X;
            this.itemsListView.Size = itemsListViewSize;

            this.UpdateItemsListViewHeight();
        }

        private void OnMaxItemsHeightUpdated() => this.UpdateItemsListViewHeight();

        private void UpdateItemsListViewHeight()
        {
            if (!this.IsAttached)
            {
                return;
            }

            var itemsListViewSize = this.itemsListView.Size;
            if (this.DataSource != null)
            {
                itemsListViewSize.Y = Math.Min(
                    this.itemsListView.RowHeight * this.DataSource.Count,
                    this.maxItemsHeight);
            }
            else
            {
                itemsListViewSize.Y = this.maxItemsHeight;
            }

            this.itemsListView.Size = itemsListViewSize;

            var containerPos = this.itemsContainerTransform.LocalPosition;
            containerPos.Y = -(itemsListViewSize.Y / 2) - (this.size.Y / 2);
            this.itemsContainerTransform.LocalPosition = containerPos;
        }
    }
}
