using System;
using Evergine.MRTK.Scenes;

namespace Evergine.MRTK.Demo.Scenes
{
    public class DemoScene : XRScene
    {
        protected override Guid CursorMatPressed => EvergineContent.MRTK.Materials.Cursor.CursorPinch;

        protected override Guid CursorMatReleased => EvergineContent.MRTK.Materials.Cursor.CursorBase;

        protected override Guid HoloHandsMat => EvergineContent.Materials.Colors.Standard_Orange;

        protected override Guid SpatialMappingMat => Guid.Empty;

        protected override Guid HandRayTexture => EvergineContent.MRTK.Textures.line_dots_png;

        protected override Guid HandRaySampler => EvergineContent.MRTK.Samplers.LinearWrapSampler;

        protected override Guid LeftControllerModelPrefab => EvergineContent.MRTK.Prefabs.InputSystem.DefaultLeftPhysicalController_weprefab;

        protected override Guid RightControllerModelPrefab => EvergineContent.MRTK.Prefabs.InputSystem.DefaultRightPhysicalController_weprefab;

        protected override float MaxFarCursorLength => 0.5f;
    }
}