using Noesis;
using System;
using System.Collections.Generic;
using System.Text;
using WaveEngine.Common.Graphics;
using Color = WaveEngine.Common.Graphics.Color;

namespace WaveEngine_MRTK_Demo.Effects
{
    [WaveEngine.Framework.Graphics.MaterialDecoratorAttribute("4f7e4c24-e83c-4350-9cd4-511fb2199cf4")]
    public partial class HoloGraphic : WaveEngine.Framework.Graphics.MaterialDecorator
    {
        public Color Albedo
        {
            get
            {
                return new Color(Parameters_Color.X, Parameters_Color.Y, Parameters_Color.Z, Parameters_Alpha);
            }
            set 
            {
                Parameters_Color = value.ToVector3();
                Parameters_Alpha = value.A;
            }
        }

        public Color LightColor
        {
            get
            {
                return new Color(Parameters_LightColor0.X, Parameters_LightColor0.Y, Parameters_LightColor0.Z);
            }
            set
            {
                Parameters_LightColor0 = value.ToVector4();
            }
        }
    }
}
