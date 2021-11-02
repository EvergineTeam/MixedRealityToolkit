using System;
using System.Linq;
using Evergine.Editor.Extension;
using Evergine.Editor.Extension.Attributes;
using Evergine.MRTK.SDK.Features.Input.Handlers;
using Evergine.MRTK.Demo.VoiceCommands;

namespace Evergine.MRTK.Demo.Editor
{
    [CustomPanelEditor(typeof(SpeechHandler))]
    public class SpeechHandlerPanel : PanelEditor
    {
        public new SpeechHandler Instance => (SpeechHandler)base.Instance;

        public override void GenerateUI()
        {
            base.GenerateUI();
            this.CreateSubPanel(nameof(SpeechHandler.SpeechKeywords), () => this.Instance.SpeechKeywords, (v) => this.Instance.SpeechKeywords = v);
        }

        private void CreateSubPanel(string name, Func<string[]> getFunc, Action<string[]> setFunc)
        {
            var contraintsSubpanel = this.propertyPanelContainer.AddSubPanel(name, name).Properties;
            foreach (var keyword in VoiceKeywords.ValidVoiceKeywords.OrderBy(word => word))
            {
                contraintsSubpanel.AddBoolean(
                    keyword,
                    keyword,
                    defaultValue: false,
                    getValue: () => getFunc()?.Contains(keyword) == true,
                    setValue: (shouldBeActive) =>
                    {
                        var array = getFunc();
                        var currentlyActive = array?.Contains(keyword) ?? false;
                        if (shouldBeActive != currentlyActive)
                        {
                            setFunc(shouldBeActive ?
                                array?.Append(keyword).ToArray() ?? new[] { keyword } :
                                array?.Except(new[] { keyword }).ToArray() ?? null);
                        }
                    });
            }
        }
    }
}
