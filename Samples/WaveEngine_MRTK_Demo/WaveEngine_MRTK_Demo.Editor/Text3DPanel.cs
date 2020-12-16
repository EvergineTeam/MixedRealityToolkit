using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using WaveEngine.Editor.Extension;
using WaveEngine.Editor.Extension.Attributes;
using WaveEngine.Framework;
using WaveEngine.Platform;
using WaveEngine_MRTK_Demo.Toolkit.Components.GUI;

namespace WaveEngine_MRTK_Demo.Editor
{
    [CustomPanelEditor(typeof(Text3D))]
    public class Text3DPanel : PanelEditor
    {
        public new Text3D Instance => (Text3D)base.Instance;

        private Dictionary<string, string> fontsPathByName;

        protected override void Loaded()
        {
            base.Loaded();
            var assetsRootPath = Application.Current.Container.Resolve<AssetsDirectory>().RootPath;
            this.fontsPathByName = new Dictionary<string, string>() { { "Default", string.Empty } };
            foreach (var item in WaveContentUtils.FindFonts(assetsRootPath))
            {
                this.fontsPathByName.Add(item.Key, item.Value);
            }
        }

        public override void GenerateUI()
        {
            base.GenerateUI();
            this.propertyPanelContainer.AddSelector(
                nameof(Text3D.FontFamilySource),
                "FontFamily",
                this.fontsPathByName.Keys,
                () => this.fontsPathByName.FirstOrDefault(x => x.Value == this.Instance.FontFamilySource).Key,
                x => this.Instance.FontFamilySource = this.fontsPathByName[x]);

            this.propertyPanelContainer.Find(nameof(Text3D.Width)).GetIsVisible = () => this.Instance.CustomWidth;
            this.propertyPanelContainer.Find(nameof(Text3D.HorizontalAlignment)).GetIsVisible = () => this.Instance.CustomWidth;
            this.propertyPanelContainer.Find(nameof(Text3D.Height)).GetIsVisible = () => this.Instance.CustomHeight;
            this.propertyPanelContainer.Find(nameof(Text3D.VerticalAlignment)).GetIsVisible = () => this.Instance.CustomHeight;
        }
    }
}
