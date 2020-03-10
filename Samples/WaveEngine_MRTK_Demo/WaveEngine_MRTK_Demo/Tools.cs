using System;
using System.Collections.Generic;
using System.Text;
using WaveEngine.Framework;

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
    }
}
