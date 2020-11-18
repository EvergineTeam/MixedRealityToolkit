// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using WaveEngine.Common.Attributes;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Mathematics;

namespace WaveEngine.MRTK.SDK.Features.UX.Components.PressableButtons
{
    /// <summary>
    /// Visual feedback of a pressable button.
    /// </summary>
    public class PressableButtonVisualFeedbackComponent : Component, IPressableButtonFeedback
    {
        /// <summary>
        /// The transform.
        /// </summary>
        [BindComponent]
        protected Transform3D transform;

        /// <summary>
        /// The material component.
        /// </summary>
        [BindComponent(isRequired: false)]
        protected MaterialComponent materialComponent;

        /// <summary>
        /// Gets or sets the color that the optional MaterialComponent will be set to when the button is pressed.
        /// </summary>
        [RenderProperty(Tooltip = "The color that the optional MaterialComponent will be set to when the button is pressed")]
        public Material PressedMaterial { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the button visuals scale will be modified when pressed.
        /// </summary>
        [RenderProperty(Tooltip = "Set whether the button visuals scale will be modified when pressed")]
        public bool Compressable { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating the minimum ratio of the original scale the visual feedback entity can be compressed to.
        /// </summary>
        [RenderPropertyAsFInput(Tooltip = "The minimum ratio of the original scale the visual feedback entity can be compressed to.", MinLimit = 0.0f, MaxLimit = 1.0f)]
        public float MinCompressRatio { get; set; } = 0.25f;

        /// <summary>
        /// Gets or sets a value indicating the scale to apply to the movement of the entity.
        /// </summary>
        [RenderPropertyAsFInput(Tooltip = "The scale to apply to the movement of the entity.", MinLimit = 0.0f, MaxLimit = 1.0f)]
        public float MovementScale { get; set; } = 1f;

        /// <summary>
        /// Gets or sets a value indicating whether the button visuals color will be modified when pressed.
        /// </summary>
        [RenderProperty(Tooltip = "Set whether the button visuals color will be modified when pressed")]
        public bool ChangeColor { get; set; } = false;

        private Vector3 movingVisualsInitialLocalPosition;
        private Vector3 movingVisualsInitialLocalScale;

        private Material releasedMaterial;

        /// <inheritdoc/>
        protected override bool OnAttached()
        {
            var attached = base.OnAttached();

            if (attached)
            {
                if (this.materialComponent?.Material != null)
                {
                    // Cache current material as released
                    this.releasedMaterial = this.materialComponent.Material;
                }

                this.movingVisualsInitialLocalPosition = this.transform.LocalPosition;
                this.movingVisualsInitialLocalScale = this.transform.LocalScale;
            }

            return attached;
        }

        /// <inheritdoc/>
        void IPressableButtonFeedback.Feedback(Vector3 pushVector, float pressRatio, bool pressed)
        {
            if (this.Compressable)
            {
                // Compress the button visuals by the push amount.
                float press = Math.Max(this.MinCompressRatio, 1.0f - pressRatio);

                Vector3 scale = this.transform.LocalScale;
                scale.Z = this.movingVisualsInitialLocalScale.Z * press;
                this.transform.LocalScale = scale;
            }
            else
            {
                // Transform the push vector into the collider's parent's space
                Vector3 pushVectorLocal = Vector3.TransformNormal(pushVector, this.transform.WorldToLocalTransform);

                // Move visuals
                this.transform.LocalPosition = this.movingVisualsInitialLocalPosition + (pushVectorLocal * this.MovementScale);
            }

            if (this.ChangeColor)
            {
                // Set MaterialComponent color
                this.materialComponent.Material = pressed ? this.PressedMaterial : this.releasedMaterial;
            }
        }
    }
}
