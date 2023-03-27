using Evergine.Editor.Extension;
using Evergine.Editor.Extension.Attributes;
using Evergine.MRTK.Managers.Data;
using System.Collections.Generic;

namespace Evergine.MRTK.Editor
{
    [CustomPropertyEditor(typeof(List<PointerOption>))]
    public class PointerOptionListEditor : PropertyEditor<List<PointerOption>>
    {
        private List<PointerOption> property;

        protected override void Loaded()
        {
            this.property = this.GetMemberValue();
        }

        public override void GenerateUI()
        {
            if (this.property != null)
            {
                var title = "Pointer options";
                var listPanel = this.propertyPanelContainer.AddSubPanel(title, title).Properties;

                for (int i = 0; i < this.property.Count; i++)
                {
                    var index = i; // Needed for a correct callback index storage

                    var item = this.property[index];

                    listPanel.AddEnum($"ControllerType{index}", "Controller Type", getValue: () => item.ControllerType, setValue: (v) => item.ControllerType = v);
                    listPanel.AddEnum($"Handedness{index}", "Handedness", getValue: () => item.Handedness, setValue: (v) => item.Handedness = v);
                    listPanel.AddLoadable($"Pointer{index}", "Pointer", getValue: () => item.Pointer, setValue: (v) => item.Pointer = v);

                    listPanel.AddButton($"Remove{index}", "Remove",
                        () =>
                        {
                            this.property.RemoveAt(index);
                            propertyPanelContainer.InvalidateLayout();
                        });
                    listPanel.AddLabel($"Separator{index}", "");
                }

                listPanel.AddButton("Add", "Add",
                    () =>
                    {
                        this.property.Add(new PointerOption()
                        {
                            ControllerType = default,
                            Handedness = default,
                            Pointer = null,
                        });
                        propertyPanelContainer.InvalidateLayout();
                    });
            }
        }
    }
}
