using System;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Framework;
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
            this.materialDecorator.Parameters_FingerPosLeft = this.cursorManager.Cursors[0].transform.Position;
            this.materialDecorator.Parameters_FingerPosRight = this.cursorManager.Cursors[1].transform.Position;
        }
    }
}
