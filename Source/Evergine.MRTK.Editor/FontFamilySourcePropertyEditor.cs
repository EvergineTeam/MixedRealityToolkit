using System.Collections.Generic;
using System.Linq;
using Evergine.Editor.Extension;
using Evergine.Editor.Extension.Attributes;
using Evergine.Framework;
using Evergine.MRTK.Toolkit.GUI;
using Evergine.Platform;

namespace Evergine.MRTK.Editor
{
    [CustomPropertyEditor(typeof(FontFamilySourceProperty))]
    public class FontFamilySourcePropertyEditor : PropertyEditor<FontFamilySourceProperty>
    {
        private Dictionary<string, string> fontsPathByName;

        protected override void Loaded()
        {
            base.Loaded();
            var assetsRootPath = Application.Current.Container.Resolve<AssetsDirectory>().RootPath;
            this.fontsPathByName = new Dictionary<string, string>() { { "Default", string.Empty } };
            foreach (var item in EvergineContentUtils.FindFonts(assetsRootPath))
            {
                this.fontsPathByName.Add(item.Key, item.Value);
            }

            this.FontFamilySourceSetValue(this.FontFamilySourceGetValue());
        }

        public override void GenerateUI()
        {
            this.propertyPanelContainer.AddSelector(this.Id, this.Name, this.fontsPathByName.Keys, this.FontFamilySourceGetValue, this.FontFamilySourceSetValue);
        }

        private string FontFamilySourceGetValue()
        {
            var instance = this.GetMemberValue();
            return this.fontsPathByName.FirstOrDefault(x => x.Value == instance.FontFamilySource).Key;
        }

        private void FontFamilySourceSetValue(string fontFamilySource)
        {
            if (fontFamilySource == null)
            {
                return;
            }

            this.fontsPathByName.TryGetValue(fontFamilySource, out var source);

            var instance = this.GetMemberValue();
            instance.FontFamilySource = source;
        }
    }
}
