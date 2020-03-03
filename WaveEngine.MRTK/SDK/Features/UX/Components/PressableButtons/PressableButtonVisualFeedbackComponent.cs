// Copyright © 2019 Wave Engine S.L. All rights reserved. Use is subject to license terms.

using WaveEngine.Common.Attributes;
using WaveEngine.Common.Graphics;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Graphics.Materials;
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
        public Color PressedColor { get; set; }

        /// <summary>
        /// Gets or sets the color that the optional MaterialComponent will be set to when the button is released.
        /// </summary>
        [RenderProperty(Tooltip = "The color that the optional MaterialComponent will be set to when the button is released")]
        public Color ReleasedColor { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the button visuals scale will be modified when pressed.
        /// </summary>
        [RenderProperty(Tooltip = "Set whether the button visuals scale will be modified when pressed")]
        public bool Compressable { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the button visuals color will be modified when pressed.
        /// </summary>
        [RenderProperty(Tooltip = "Set whether the button visuals color will be modified when pressed")]
        public bool ChangeColor { get; set; } = false;

        private StandardMaterial material;

        private Vector3 movingVisualsInitialLocalPosition;
        private Vector3 movingVisualsInitialLocalScale;

        /// <inheritdoc/>
        protected override bool OnAttached()
        {
            var attached = base.OnAttached();

            if (attached)
            {
                if (this.materialComponent != null)
                {
                    this.material = new StandardMaterial(this.materialComponent.Material);
                }

                this.movingVisualsInitialLocalPosition = this.transform.LocalPosition;
                this.movingVisualsInitialLocalScale = this.transform.LocalScale;
            }

            return attached;
        }

        private void SetColor(Color color)
        {
            if (this.material != null)
            {
                this.material.BaseColor = color;
            }
        }

        /// <inheritdoc/>
        void IPressableButtonFeedback.Feedback(Vector3 pushVector, Matrix4x4 colliderTransform, float pressRatio, bool pressed)
        {
            // Transform the push vector into the collider's parent's space
            Vector3 pushVectorLocal = Vector3.TransformNormal(pushVector, this.transform.WorldToLocalTransform);

            ////if (this.Compressable)
            ////{
            ////    // Move visuals half the travel distance to accomodate for the scale change
            ////    this.transform.LocalPosition = this.movingVisualsInitialLocalPosition + (pushVectorLocal / 2);

            ////    // Apply scale in press direction
            ////    var transformedOffset = Vector3.Transform(pushVector, this.transform.Orientation);
            ////    this.transform.LocalScale = this.movingVisualsInitialLocalScale * (Vector3.One - Vector3.Abs(transformedOffset));
            ////}
            ////else
            ////{
            // Move visuals
            this.transform.LocalPosition = this.movingVisualsInitialLocalPosition + pushVectorLocal;
            ////}

            if (this.ChangeColor)
            {
                // Set MaterialComponent color
                this.SetColor(pressed ? this.PressedColor : this.ReleasedColor);
            }
        }
    }
}
