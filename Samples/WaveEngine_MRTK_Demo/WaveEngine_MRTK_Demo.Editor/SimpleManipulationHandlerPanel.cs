using System;
using System.Linq;
using WaveEngine.Editor.Extension;
using WaveEngine.Editor.Extension.Attributes;
using WaveEngine.MRTK.SDK.Features.Input.Handlers.Manipulation;
using static WaveEngine.MRTK.SDK.Features.Input.Handlers.Manipulation.SimpleManipulationHandler;

namespace WaveEngine_MRTK_Demo.Editor
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
            foreach(var constraint in contraints)
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
