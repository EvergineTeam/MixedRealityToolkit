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

                string[] nodesWithButtons = { "PressableButtons", "PressableButtonsSharedPlate", "PressableButtonsSharedPlate40x40mm" };
                foreach (string nodeName in nodesWithButtons)
                {
                    Entity buttonsParent = this.Owner.Parent.Find(nodeName);
                    foreach (PressableButton b in buttonsParent.FindComponentsInChildren<PressableButton>())
                    {
                        b.ButtonPressed += OnButtonPressed;

                        //Update text based on scale
                        Text3D text3d = b.Owner.FindComponentInChildren<Text3D>();
                        if (text3d != null)
                        {
                            text3d.Text = string.Format("{0}x{0}mm", 32.0f * b.Owner.FindComponent<Transform3D>().Scale.X);
                        }
                    }
                }
            }
        }

        private void OnButtonPressed(object sender, EventArgs e)
        {
            materialComponent.Material = materials[(currentMaterialIdx ++) % materials.Length];
        }
    }
}
