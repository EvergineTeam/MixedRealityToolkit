// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using System.Collections;
using System.Collections.Generic;
using WaveEngine.Common.Attributes;
using WaveEngine.Common.Graphics;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Mathematics;
using WaveEngine.MRTK.Effects;

namespace WaveEngine.MRTK.Emulation
{
    /// <summary>
    /// Utility component to animate and visualize a light that can be used with
    /// the <see cref="HoloGraphic"/> shader <see cref="HoloGraphic.ProximityLightDirective"/> feature.
    /// </summary>
    public class ProximityLight : Behavior
    {
        // Two proximity lights are supported at this time.
        internal const int MaxLights = 2;

        internal static List<ProximityLight> ActiveProximityLights = new List<ProximityLight>(MaxLights);

        /// <summary>
        /// The <see cref="Transform3D"/> component dependency.
        /// </summary>
        [BindComponent]
        protected Transform3D transform;

        /// <summary>
        /// Gets the position of the <see cref="ProximityLight"/>.
        /// </summary>
        public Vector3 Position => this.transform.Position;

        /// <summary>
        /// Gets or sets the radius of the <see cref="ProximityLight"/> effect when near to a surface.
        /// </summary>
        [RenderPropertyAsFInput(minLimit: 0, maxLimit: 1, Tooltip = "Specifies the radius of the ProximityLight effect when near to a surface.")]
        public float NearRadius { get; set; } = 0.05f;

        /// <summary>
        /// Gets or sets the radius of the <see cref="ProximityLight"/> effect when far from a surface.
        /// </summary>
        [RenderPropertyAsFInput(minLimit: 0, maxLimit: 1, Tooltip = "Specifies the radius of the ProximityLight effect when far from a surface.")]
        public float FarRadius { get; set; } = 0.2f;

        /// <summary>
        /// Gets or sets the distance a <see cref="ProximityLight"/> must be from a surface to be considered near.
        /// </summary>
        [RenderPropertyAsFInput(minLimit: 0, maxLimit: 1, Tooltip = "Specifies the distance a ProximityLight must be from a surface to be considered near.")]
        public float NearDistance { get; set; } = 0.02f;

        /// <summary>
        /// Gets or sets a value that, when a <see cref="ProximityLight"/> is near, indicates the smallest size percentage
        /// from the far size it can shrink to.
        /// </summary>
        [RenderPropertyAsFInput(minLimit: 0, maxLimit: 1, Tooltip = "When a ProximityLight is near, the smallest size percentage from the far size it can shrink to.")]
        public float MinNearSizePercentage { get; set; } = 0.35f;

        /// <summary>
        /// Gets or sets the color of the <see cref="ProximityLight"/> gradient at the center (RGB) and (A) is gradient extent.
        /// </summary>
        [RenderProperty(Tooltip = "The color of the ProximityLight gradient at the center (RGB) and (A) is gradient extent.")]
        public Color CenterColor { get; set; } = new Color(54, 142, 250, 0);

        /// <summary>
        /// Gets or sets the color of the <see cref="ProximityLight"/> gradient at the middle (RGB) and (A) is gradient extent.
        /// </summary>
        [RenderProperty(Tooltip = "The color of the ProximityLight gradient at the middle (RGB) and (A) is gradient extent.")]
        public Color MiddleColor { get; set; } = new Color(47, 132, 255, 51);

        /// <summary>
        /// Gets or sets the color of the <see cref="ProximityLight"/> gradient at the outer (RGB) and (A) is gradient extent.
        /// </summary>
        [RenderProperty(Tooltip = "The color of the ProximityLight gradient at the outer (RGB) and (A) is gradient extent.")]
        public Color OuterColor { get; set; } = new Color(82, 31, 191, 255);

        internal float PulseFade { get; private set; } = 0.0f;

        internal float PulseTime { get; private set; } = 0.0f;

        private IEnumerator routine;

        /// <inheritdoc />
        protected override void OnActivated()
        {
            base.OnActivated();
            ActiveProximityLights.Add(this);
        }

        /// <inheritdoc />
        protected override void OnDeactivated()
        {
            base.OnDeactivated();
            ActiveProximityLights.Remove(this);
        }

        /// <inheritdoc />
        protected override void Update(TimeSpan gameTime)
        {
            if (this.routine == null)
            {
                return;
            }

            this.deltaTime = (float)gameTime.TotalSeconds;

            if (this.routine.MoveNext() == false)
            {
                this.routine = null;
            }
        }

        /// <summary>
        /// Initiates a pulse, if one is not already occurring, which simulates a user touching a surface.
        /// </summary>
        /// <param name="pulseDuration">How long in seconds should the pulse animate over.</param>
        /// <param name="fadeBegin">At what point during the pulseDuration should the pulse begin to fade out as a percentage. Range should be [0, 1].</param>
        /// <param name="fadeSpeed">The speed to fade in and out.</param>
        public void Pulse(float pulseDuration = 0.2f, float fadeBegin = 0.8f, float fadeSpeed = 10.0f)
        {
            if (this.PulseTime <= 0)
            {
                this.routine = this.PulseRoutine(pulseDuration, fadeBegin, fadeSpeed);
            }
        }

        private float deltaTime;

        private IEnumerator PulseRoutine(float pulseDuration, float fadeBegin, float fadeSpeed)
        {
            float pulseTimer = 0.0f;

            while (pulseTimer < pulseDuration)
            {
                pulseTimer += this.deltaTime;
                this.PulseTime = pulseTimer / pulseDuration;

                if (this.PulseTime > fadeBegin)
                {
                    this.PulseFade += this.deltaTime;
                }

                yield return null;
            }

            while (this.PulseFade < 1.0f)
            {
                this.PulseFade += this.deltaTime * fadeSpeed;

                yield return null;
            }

            this.PulseTime = 0.0f;

            while (this.PulseFade > 0.0f)
            {
                this.PulseFade -= this.deltaTime * fadeSpeed;

                yield return null;
            }

            this.PulseFade = 0.0f;
        }
    }
}
