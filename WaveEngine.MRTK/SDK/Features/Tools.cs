// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using WaveEngine.Common.Audio;
using WaveEngine.Common.Media;
using WaveEngine.Components.Sound;
using WaveEngine.Framework;

namespace WaveEngine.MRTK.SDK.Features
{
    /// <summary>
    /// Some shared Tools.
    /// </summary>
    public static class Tools
    {
        /// <summary>
        /// Gets a Component or adds it if doesn't exist.
        /// </summary>
        /// <typeparam name="T">The component type.</typeparam>
        /// <param name="owner">The entity to add the component to.</param>
        /// <returns>The requested component.</returns>
        public static T GetOrAddComponent<T>(this Entity owner)
            where T : Component, new()
        {
            T t = owner.FindComponent<T>();
            if (t == null)
            {
                t = new T();
                owner.AddComponent(t);
            }

            return t;
        }

        /// <summary>
        /// /// Gets a Component in this entity or any of its children or adds it if doesn't exist.
        /// </summary>
        /// <typeparam name="T">The component type.</typeparam>
        /// <param name="owner">The entity to add the component to.</param>
        /// <returns>The requested component.</returns>
        public static T GetInChildrenOrAddComponent<T>(this Entity owner)
            where T : Component, new()
        {
            T t = owner.FindComponentInChildren<T>();
            if (t == null)
            {
                t = new T();
                owner.AddComponent(t);
            }

            return t;
        }

        /// <summary>
        /// Plays a sound using the passed soundEmitter.
        /// </summary>
        /// <param name="soundEmitter">The soundEmitter.</param>
        /// <param name="sound">The sound to play.</param>
        /// <param name="pitch">The pitch.</param>
        public static void PlaySound(SoundEmitter3D soundEmitter, AudioBuffer sound, float pitch = 1.0f)
        {
            if (soundEmitter != null && sound != null)
            {
                if (soundEmitter.PlayState == PlayState.Playing)
                {
                    soundEmitter.Stop();
                }

                soundEmitter.Audio = sound;
                soundEmitter.Pitch = pitch;

                soundEmitter.Play();
            }
        }
    }
}
