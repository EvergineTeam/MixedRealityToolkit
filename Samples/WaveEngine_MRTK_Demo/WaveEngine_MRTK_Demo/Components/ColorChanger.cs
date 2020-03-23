using System;
using System.Collections.Generic;
using System.Text;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Mathematics;
using WaveEngine.MRTK.SDK.Features.UX.Components.PressableButtons;

namespace WaveEngine_MRTK_Demo.Components
{
    public class ColorChanger : Component
    {
        [BindComponent]
        public MaterialComponent materialComponent;

        public Material material0;
        public Material material1;
        public Material material2;

        private Material[] materials;
        private int currentMaterialIdx = 0;

        protected override void Start()
        {
            if (!Application.Current.IsEditor)
            {
                materials = new Material[]{material0, material1, material2};
                materialComponent.Material = material0;

                Entity buttonsParent = this.Owner.Parent.Find("PressableButtons");
                foreach (PressableButton b in buttonsParent.FindComponentsInChildren<PressableButton>())
                {
                    b.ButtonPressed += OnButtonPressed;
                }
            }
        }

        private void OnButtonPressed(object sender, EventArgs e)
        {
            materialComponent.Material = materials[(currentMaterialIdx ++) % materials.Length];
        }
    }
}
