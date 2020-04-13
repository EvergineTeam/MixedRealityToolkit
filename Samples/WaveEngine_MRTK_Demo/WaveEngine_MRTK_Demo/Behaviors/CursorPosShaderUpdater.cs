using System;
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
        private CursorManager cursorManager;

        protected override void Start()
        {
            this.cursorManager = this.Owner.Scene.Managers.FindManager<CursorManager>();
            this.materialDecorator = new HoloGraphic(this.materialComponent.Material);
        }

        protected override void Update(TimeSpan gameTime)
        {
            //this.materialDecorator.Parameters_FingerPosLeft = this.cursorManager.Cursors[0].transform.Position;
            //this.materialDecorator.Parameters_FingerPosRight = this.cursorManager.Cursors[1].transform.Position;
            //Vector3 p0 = this.cursorManager.Cursors[0].transform.Position;
            //this.materialDecorator.Parameters_HoverLightData = new Vector4(p0.X, p0.Y, p0.Z, 1.0f);

            for (int i = 0; i < 2; ++i)
            {
                this.materialComponent.Material.CBuffers[1].SetBufferData<Vector3>(this.cursorManager.Cursors[i].transform.Position, 320 + 32 * i);
                this.materialComponent.Material.CBuffers[1].SetBufferData<float>(1.0f, 320 + 32 * i + 12);
            }
            //this.materialComponent.Material.CBuffers[1].SetBufferData<Vector3>(this.cursorManager.Cursors[1].transform.Position, 320 + 16);
            //this.materialComponent.Material.CBuffers[1].SetBufferData<float>(1.0f, 320 + 16 + 12);

            //this.material.CBuffers[1].GetBufferData<WaveEngine.Mathematics.Vector4>(320);
        }
    }
}
