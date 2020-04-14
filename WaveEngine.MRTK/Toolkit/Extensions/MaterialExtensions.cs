// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System.Runtime.CompilerServices;
using WaveEngine.Framework.Graphics;

namespace WaveEngine.MRTK.Toolkit.Extensions
{
    /// <summary>
    /// A class that adds some useful extensions to WaveEngine's Material class.
    /// </summary>
    public static class MaterialExtensions
    {
        /// <summary>
        /// Clone a material.
        /// </summary>
        /// <param name="material">The material to copy.</param>
        /// <returns>The copy of the material.</returns>
        public static unsafe Material Clone(this Material material)
        {
            Material copy = new Material(material.Effect);
            copy.ActiveDirectivesNames = material.ActiveDirectivesNames;
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
