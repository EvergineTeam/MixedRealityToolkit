using System;
using WaveEngine.Common.Graphics;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Framework;
using WaveEngine.Mathematics;
using WaveEngine_MRTK_Demo.Effects;
using WaveEngine_MRTK_Demo.Emulation;

namespace WaveEngine_MRTK_Demo.Behaviors
{
    public class CursorPosShaderUpdater : Behavior
    {
        [BindComponent]
        protected MaterialComponent materialComponent = null;

        private HoloGraphic materialDecorator;

        protected override void Start()
        {
            this.materialDecorator = new HoloGraphic(this.materialComponent.Material);
        }

        protected override void Update(TimeSpan gameTime)
        {
            for (int i = 0; i < HoverLight.activeHoverLights.Count && i < HoverLight.MaxLights; ++i)
            {
                int accessIdx = 320 + 32 * i;

                HoverLight light = HoverLight.activeHoverLights[i];
                this.materialComponent.Material.CBuffers[1].SetBufferData<Vector3>(light.transform.Position, accessIdx);
                this.materialComponent.Material.CBuffers[1].SetBufferData<float>  (light.Radius,             accessIdx + 12);
                this.materialComponent.Material.CBuffers[1].SetBufferData<Vector4>(light.Color.ToVector4(),  accessIdx + 16);
            }

            for (int i = 0; i < ProximityLight.MaxLights; ++i)
            {
                int accessIdx = 416 + 96 * i;

                ProximityLight light = i < ProximityLight.activeProximityLights.Count ? ProximityLight.activeProximityLights[i] : null;
                if (light != null)
                {
                    this.materialComponent.Material.CBuffers[1].SetBufferData<Vector3>(light.transform.Position, accessIdx);
                    this.materialComponent.Material.CBuffers[1].SetBufferData<float>(1.0f, accessIdx + 12);

                    float pulseScaler = 1.0f;// + light.pulseTime;
                    Vector4 v4 = new Vector4(
                            light.NearRadius * pulseScaler,
                            1.0f / MathHelper.Clamp(light.FarRadius * pulseScaler, 0.001f, 1.0f),
                            1.0f / MathHelper.Clamp(light.NearDistance * pulseScaler, 0.001f, 1.0f),
                            MathHelper.Clamp(light.MinNearSizePercentage, 0.0f, 1.0f)
                        );
                    this.materialComponent.Material.CBuffers[1].SetBufferData<Vector4>(
                        v4,
                        accessIdx + 16
                    );
                    v4 = new Vector4(
                            light.NearDistance * light.pulseTime,
                            MathHelper.Clamp(1.0f - light.pulseFade, 0.0f, 1.0f),
                            0.0f,
                            0.0f);
                    this.materialComponent.Material.CBuffers[1].SetBufferData<Vector4>(
                        v4,
                        accessIdx + 32);
                    this.materialComponent.Material.CBuffers[1].SetBufferData<Vector4>(light.CenterColor.ToVector4(), accessIdx + 48);
                    this.materialComponent.Material.CBuffers[1].SetBufferData<Vector4>(light.MiddleColor.ToVector4(), accessIdx + 64);
                    this.materialComponent.Material.CBuffers[1].SetBufferData<Vector4>(light.OuterColor.ToVector4(), accessIdx + 80);
                }
                else
                {
                    this.materialComponent.Material.CBuffers[1].SetBufferData<Vector4>(Vector4.Zero, accessIdx);
                }
            }
        }
    }
}
