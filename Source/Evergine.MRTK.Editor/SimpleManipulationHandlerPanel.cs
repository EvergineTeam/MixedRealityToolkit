using System;
using System.Linq;
using Evergine.Editor.Extension;
using Evergine.Editor.Extension.Attributes;
using Evergine.MRTK.SDK.Features.Input.Handlers.Manipulation;
using static Evergine.MRTK.SDK.Features.Input.Handlers.Manipulation.SimpleManipulationHandler;

namespace Evergine.MRTK.Editor
{
    [CustomPanelEditor(typeof(SimpleManipulationHandler))]
    public class SimpleManipulationHandlerPanel : PanelEditor
    {
        private const string Constraints = nameof(Constraints);

        public new SimpleManipulationHandler Instance => (SimpleManipulationHandler)base.Instance;

        public override void GenerateUI()
        {
            base.GenerateUI();

            var contraintsSubpanel = this.propertyPanelContainer.AddSubPanel(Constraints, Constraints).Properties;
            var contraints = Enum.GetValues(typeof(ConstraintsEnum)).Cast<ConstraintsEnum>().ToArray();
            foreach (var constraint in contraints)
            {
                // Discard constraints with more than one 1 (...All and None)
                if ((constraint & (constraint - 1)) != 0 || constraint == 0)
                {
                    continue;
                }

                string constraintName = constraint.ToString();
                contraintsSubpanel.AddBoolean(constraintName,
                                                       constraintName,
                                                       defaultValue: false,
                                                       getValue: () => (this.Instance.Constraints & constraint) != 0,
                                                       setValue: (e) => this.Instance.Constraints = e ?
                                                                            this.Instance.Constraints | constraint :
                                                                            this.Instance.Constraints & ~constraint);
            }
        }
    }
}
