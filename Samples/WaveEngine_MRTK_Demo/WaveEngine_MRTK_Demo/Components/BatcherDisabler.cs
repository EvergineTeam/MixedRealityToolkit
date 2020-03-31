using System;
using System.Collections.Generic;
using System.Text;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Framework;

namespace WaveEngine_MRTK_Demo.Components
{
    public class BatcherDisabler : Component
    {
        protected override void Start()
        {
            if (!Application.Current.IsEditor)
            {
                MaterialComponent materialComponent = Owner.FindComponent<MaterialComponent>();
                if (materialComponent != null && materialComponent.Material != null)
                {
                    materialComponent.Material = materialComponent.Material.Clone();
                }
            }
        }
    }
}
