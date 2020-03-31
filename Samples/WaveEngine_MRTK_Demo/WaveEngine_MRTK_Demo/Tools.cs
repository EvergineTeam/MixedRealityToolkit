using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using WaveEngine.Common.Audio;
using WaveEngine.Common.Media;
using WaveEngine.Components.Sound;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;

namespace WaveEngine_MRTK_Demo
{
    public static class Tools
    {
        public static T GetOrAddComponent<T>(this Entity Owner) where T : Component, new()
        {
            T t = Owner.FindComponent<T>();
            if (t == null)
            {
                t = new T();
                Owner.AddComponent(t);
            }
            return t;
        }

        public static T GetInChildrenOrAddComponent<T>(this Entity Owner) where T : Component, new()
        {
            T t = Owner.FindComponentInChildren<T>();
            if (t == null)
            {
                t = new T();
                Owner.AddComponent(t);
            }
            return t;
        }

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

        public static void PlaySound(SoundEmitter3D soundEmitter,  AudioBuffer sound)
        {
            if (soundEmitter != null && sound != null)
            {
                if (soundEmitter.PlayState == PlayState.Playing)
                {
                    soundEmitter.Stop();
                }

                soundEmitter.Audio = sound;

                soundEmitter.Play();
            }
        }
    }
}
