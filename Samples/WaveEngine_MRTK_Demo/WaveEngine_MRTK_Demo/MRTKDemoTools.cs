
using System;
using System.Runtime.CompilerServices;
using WaveEngine.Common.Graphics;
using WaveEngine.Common.Input.Keyboard;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Components.XR;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Physics3D;
using WaveEngine.Framework.Services;
using WaveEngine.Framework.XR;
using WaveEngine.Mathematics;
using WaveEngine.MRTK.Services.InputSystem;
using WaveEngine_MRTK_Demo.Behaviors;
using WaveEngine_MRTK_Demo.Emulation;

namespace WaveEngine_MRTK_Demo
{
    public static class MRTKDemoTools
    {
        public static unsafe Material Clone(this Material material)
        {
            Material copy = new Material(material.Effect);
            copy.ActiveDirectivesNames = (string[])material.ActiveDirectivesNames.Clone();
            copy.LayerDescription = material.LayerDescription;
            copy.OrderBias = material.OrderBias;
            copy.AllowInstancing = material.AllowInstancing;

            for (int c = 0; c < material.CBuffers.Length; c++)
            {
                void* copyData = (void*)copy.CBuffers[c].Data;
                void* data = (void*)material.CBuffers[c].Data;
                uint size = material.CBuffers[c].Size;

                Unsafe.CopyBlock(copyData, data, size);
                copy.CBuffers[c].Dirty = true;
            }

            for (int t = 0; t < material.TextureSlots.Length; t++)
            {
                copy.TextureSlots[t] = material.TextureSlots[t];
            }

            for (int s = 0; s < material.SamplerSlots.Length; s++)
            {
                copy.SamplerSlots[s] = material.SamplerSlots[s];
            }

            return copy;
        }

        
    }
}
