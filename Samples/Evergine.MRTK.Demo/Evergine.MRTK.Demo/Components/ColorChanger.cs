using Evergine.Components.Graphics3D;
using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.MRTK.Toolkit.CommandService;
using Evergine.MRTK.Demo.Components.Commands;

namespace Evergine.MRTK.Demo.Components
{
    public class ColorChanger : BaseCommandRequester<DemoCommands>
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

            if (this.commandService != null)
            {
                this.commandService.CommandReceived += this.CommandService_CommandReceived;
            }
        }

        protected override void OnDeactivated()
        {
            base.OnDeactivated();

            if (this.commandService != null)
            {
                this.commandService.CommandReceived -= this.CommandService_CommandReceived;
            }
        }

        private void CommandService_CommandReceived(object sender, CommandReceivedEventArgs commandReceived)
        {
            if (commandReceived.Command is DemoCommands command && command == DemoCommands.ColorChange)
            {
                materialComponent.Material = materials[(++currentMaterialIdx) % materials.Length];
            }
        }
    }
}
