// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using System.Linq;
using WaveEngine.Common.Graphics;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Graphics.Effects;
using WaveEngine.Framework.Graphics.Effects.Analyzer;
using WaveEngine.Mathematics;
using WaveEngine.MRTK.Effects;

namespace WaveEngine.MRTK.Emulation
{
    /// <summary>
    /// Updates the position of the cursors on all materials with shaders that required it.
    /// </summary>
    public class CursorPosShaderUpdater : Behavior
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

        private ConstantBuffer perFrameCB;
        private ParameterInfo hoverLightsParamInfo;
        private ParameterInfo proximityLightsParamInfo;

        /// <inheritdoc/>
        protected override void Start()
        {
            base.Start();

            var effect = this.Managers.AssetSceneManager.Load<Effect>(HoloGraphic.EffectId);
            this.perFrameCB = effect.SharedCBufferBySlot.Values.FirstOrDefault(x => x.UpdatePolicy == UpdatePolicy.PerFrame);
            this.hoverLightsParamInfo = this.perFrameCB.CBufferInfo.Parameters[2];
            this.proximityLightsParamInfo = this.perFrameCB.CBufferInfo.Parameters[3];
        }

        /// <inheritdoc/>
        protected override void Update(TimeSpan gameTime)
        {
            this.UpdateHoverLights();
            this.UpdateProximityLights();
        }

        private unsafe void UpdateHoverLights()
        {
            var hoverLightParameters = stackalloc HoverLightParam[HoverLight.MaxLights];
            int i;
            int count = Math.Min(HoverLight.ActiveHoverLights.Count, HoverLight.MaxLights);
            for (i = 0; i < count; i++)
            {
                ref var param = ref hoverLightParameters[i];
                var light = HoverLight.ActiveHoverLights[i];
                param.Position = light.Position.ToVector4();
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
            int count = Math.Min(ProximityLight.ActiveProximityLights.Count, ProximityLight.MaxLights);
            for (i = 0; i < count; i++)
            {
                ref var param = ref proximityLightParameters[i];
                var light = ProximityLight.ActiveProximityLights[i];

                float pulseScaler = 1.0f + light.PulseTime;
                param.Position = light.Position.ToVector4();
                param.NearRadius = light.NearRadius * pulseScaler;
                param.FarRadius = 1.0f / MathHelper.Clamp(light.FarRadius * pulseScaler, 0.001f, 1.0f);
                param.NearDistance = 1.0f / MathHelper.Clamp(light.NearDistance * pulseScaler, 0.001f, 1.0f);
                param.MinNearSizePercentage = MathHelper.Clamp(light.MinNearSizePercentage, 0.0f, 1.0f);
                param.PulseRadius = light.NearDistance * light.PulseTime;
                param.PulseFade = MathHelper.Clamp(1.0f - light.PulseFade, 0.0f, 1.0f);
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
