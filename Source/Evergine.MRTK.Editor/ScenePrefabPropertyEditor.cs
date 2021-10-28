using System;
using System.Collections.Generic;
using System.Linq;
using Evergine.Editor.Extension;
using Evergine.Editor.Extension.Attributes;
using Evergine.Framework;
using Evergine.Framework.Services;
using Evergine.MRTK.Toolkit.Prefabs;

namespace Evergine.MRTK.Editor
{
    [CustomPropertyEditor(typeof(ScenePrefabProperty))]
    public class ScenePrefabPropertyEditor : PropertyEditor<ScenePrefabProperty>
    {
        private Dictionary<string, Guid> prefabsIdByName;

        private AssetsService assetsService;

        private Scene prefabScene;

        protected override void Loaded()
        {
            base.Loaded();
            this.assetsService = Application.Current.Container.Resolve<AssetsService>();
            this.prefabsIdByName = EvergineContentUtils.FindPrefabs();
            this.prefabsIdByName.Add("None", Guid.Empty);
            this.PrefabSetValue(this.PrefabGetValue());
        }

        public override void GenerateUI()
        {
            this.propertyPanelContainer.AddSelector(this.Id, this.Name, prefabsIdByName.Keys, this.PrefabGetValue, this.PrefabSetValue);
        }

        private string PrefabGetValue()
        {
            var instance = this.GetMemberValue();
            return this.prefabsIdByName.FirstOrDefault(x => x.Value == instance.PrefabId).Key;
        }

        private void PrefabSetValue(string x)
        {
            if (x == null)
            {
                return;
            }

            var instance = this.GetMemberValue();

            var previousPrefabId = instance.PrefabId;
            var previousIsValid = instance.IsPrefabIdValid;

            this.prefabsIdByName.TryGetValue(x, out var id);
            instance.PrefabId = id;

            if (instance.IsPrefabIdValid)
            {
                this.UpdatePrefabScene(this.assetsService.Load<Scene>(instance.PrefabId));
            }

            if (previousPrefabId != instance.PrefabId &&
                previousIsValid)
            {
                this.assetsService.Unload(previousPrefabId);
            }
        }

        private void UpdatePrefabScene(Scene scene)
        {
            if (this.prefabScene != null)
            {
                this.prefabScene.Invalidated -= this.PrefabScene_Invalidated;
                this.prefabScene = null;
            }

            var instance = this.GetMemberValue();

            if (instance.IsPrefabIdValid)
            {
                this.prefabScene = scene;

                if (this.prefabScene != null)
                {
                    this.prefabScene.Invalidated += this.PrefabScene_Invalidated;
                }
            }
        }

        private void PrefabScene_Invalidated(object sender, Evergine.Common.ILoadable e)
        {
            var instance = this.GetMemberValue();

            instance.Refresh();
            this.UpdatePrefabScene(e as Scene);
        }
    }
}
