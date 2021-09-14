using System;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
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
        public Material material3;

        private Material[] materials;
        private int currentMaterialIdx = 0;

        protected override void OnActivated()
        {
            base.OnActivated();

            if (Application.Current.IsEditor)
            {
                return;
            }

            this.materials = new Material[] { material0, material1, material2, material3 };
            this.materialComponent.Material = material0;

            this.SubscribeToButtonPresses(true);
        }

        protected override void OnDeactivated()
        {
            base.OnDeactivated();

            this.SubscribeToButtonPresses(false);
        }

        private void SubscribeToButtonPresses(bool subscribe)
        {
            foreach (var child in this.Owner.Parent.ChildEntities)
            {
                if (child.Name.ToLowerInvariant().StartsWith("pressablebuttons"))
                {
                    foreach (var button in child.FindComponentsInChildren<PressableButton>())
                    {
                        if (subscribe)
                        {
                            button.ButtonPressed += this.OnButtonPressed;
                        }
                        else
                        {
                            button.ButtonPressed -= this.OnButtonPressed;
                        }
                    }
                }
            }
        }

        private void OnButtonPressed(object sender, EventArgs e)
        {
            materialComponent.Material = materials[(++currentMaterialIdx) % materials.Length];
        }
    }
}
