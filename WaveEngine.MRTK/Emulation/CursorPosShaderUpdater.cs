// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using System.Linq;
using WaveEngine.Common.Graphics;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Graphics.Effects;
using WaveEngine.Framework.Graphics.Effects.Analyzer;
using WaveEngine.Framework.Managers;
using WaveEngine.Mathematics;
using WaveEngine.MRTK.Effects;

namespace WaveEngine.MRTK.Emulation
{
    /// <summary>
    /// Updates the position of the cursors on all materials with shaders that required it.
    /// </summary>
    public class CursorPosShaderUpdater : UpdatableSceneManager
    {
        private struct HoverLightParam
        {
            public Vector4 Position;
            public Vector3 Color;
            public float InverseRadius;
        }

        private struct ProximityLightParam
        {
            public Vector4 Position;
            public float NearRadius;
            public float FarRadius;
            public float NearDistance;
            public float MinNearSizePercentage;
            public float PulseRadius;
            public float PulseFade;
            private float Reserved0;
            private float Reserved1;
            public GammaColor CenterColor;
            public GammaColor MiddleColor;
            public GammaColor OuterColor;
        }

        private Guid holographicEffectId;

        private ConstantBuffer perFrameCB;
        private ParameterInfo hoverLightsParamInfo;
        private ParameterInfo proximityLightsParamInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="CursorPosShaderUpdater"/> class.
        /// </summary>
        /// <param name="holographicEffectId">Id of holographic effect.</param>
        public CursorPosShaderUpdater(Guid holographicEffectId)
        {
            this.holographicEffectId = holographicEffectId;
        }

        /// <inheritdoc/>
        protected override void Start()
        {
            base.Start();

            var effect = this.Managers.AssetSceneManager.Load<Effect>(this.holographicEffectId);
            this.perFrameCB = effect.SharedCBufferBySlot.Values.FirstOrDefault(x => x.UpdatePolicy == ConstantBufferInfo.UpdatePolicies.PerFrame);
            this.hoverLightsParamInfo = this.perFrameCB.CBufferInfo.Parameters[2];
            this.proximityLightsParamInfo = this.perFrameCB.CBufferInfo.Parameters[3];

            this.DisableBatchingOnRequiredHolographicMaterials();
        }

        /// <summary>
        /// Disables batching feature on meshes that uses <see cref="HoloGraphic"/> materials that does not allows batching.
        /// </summary>
        public void DisableBatchingOnRequiredHolographicMaterials()
        {
            var holographicEffectsByOwner = this.Managers.EntityManager
                                                         .FindComponentsOfType<MaterialComponent>()
                                                         .Where(m => m.Material?.Effect?.Id == this.holographicEffectId)
                                                         .ToDictionary(m => m.Owner, m => new HoloGraphic(m.Material));

            foreach (var pair in holographicEffectsByOwner)
            {
                if (pair.Value.AllowBatching)
                {
                    continue;
                }

                // Border Light and inner glow don't work if batching is enabled
                var meshComponent = pair.Key.FindComponent<MeshComponent>(isExactType: false);
                if (meshComponent == null)
                {
                    continue;
                }

                foreach (var mesh in meshComponent.Meshes)
                {
                    mesh.AllowBatching = false;
                }
            }
        }

        /// <inheritdoc/>
        public override void Update(TimeSpan gameTime)
        {
            this.UpdateHoverLights();
            this.UpdateProximityLights();
        }

        private unsafe void UpdateHoverLights()
        {
            var hoverLightParameters = stackalloc HoverLightParam[HoverLight.MaxLights];
            int i;
            for (i = 0; i < HoverLight.activeHoverLights.Count; i++)
            {
                ref var param = ref hoverLightParameters[i];
                var light = HoverLight.activeHoverLights[i];
                param.Position = light.transform.Position.ToVector4();
                param.Color = light.Color.ToVector3();
                param.InverseRadius = 1.0f / MathHelper.Clamp(light.Radius, 0.001f, 1.0f);
            }

            for (; i < HoverLight.MaxLights; i++)
            {
                hoverLightParameters[i] = new HoverLightParam();
            }

            this.perFrameCB.SetBufferData(hoverLightParameters, (uint)(HoverLight.MaxLights * sizeof(HoverLightParam)), this.hoverLightsParamInfo.Offset);
        }

        private unsafe void UpdateProximityLights()
        {
            var proximityLightParameters = stackalloc ProximityLightParam[ProximityLight.MaxLights];
            int i;
            for (i = 0; i < ProximityLight.activeProximityLights.Count; i++)
            {
                ref var param = ref proximityLightParameters[i];
                var light = ProximityLight.activeProximityLights[i];

                float pulseScaler = 1.0f; // + light.pulseTime;
                param.Position = light.transform.Position.ToVector4();
                param.NearRadius = light.NearRadius * pulseScaler;
                param.FarRadius = 1.0f / MathHelper.Clamp(light.FarRadius * pulseScaler, 0.001f, 1.0f);
                param.NearDistance = 1.0f / MathHelper.Clamp(light.NearDistance * pulseScaler, 0.001f, 1.0f);
                param.MinNearSizePercentage = MathHelper.Clamp(light.MinNearSizePercentage, 0.0f, 1.0f);
                param.PulseRadius = light.NearDistance * light.pulseTime;
                param.PulseFade = MathHelper.Clamp(1.0f - light.pulseFade, 0.0f, 1.0f);
                param.CenterColor.AsVector4 = light.CenterColor.ToVector4();
                param.MiddleColor.AsVector4 = light.MiddleColor.ToVector4();
                param.OuterColor.AsVector4 = light.OuterColor.ToVector4();
            }

            for (; i < ProximityLight.MaxLights; i++)
            {
                proximityLightParameters[i] = new ProximityLightParam();
            }

            this.perFrameCB.SetBufferData(proximityLightParameters, (uint)(ProximityLight.MaxLights * sizeof(ProximityLightParam)), this.proximityLightsParamInfo.Offset);
        }
    }
}
