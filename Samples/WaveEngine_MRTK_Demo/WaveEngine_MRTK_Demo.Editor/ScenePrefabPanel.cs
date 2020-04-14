using System;
using System.Collections.Generic;
using System.Linq;
using WaveEngine.Editor.Extension;
using WaveEngine.Editor.Extension.Attributes;
using WaveEngine.Framework;
using WaveEngine.Framework.Services;
using WaveEngine.MRTK.Toolkit.Prefabs;

namespace WaveEngine_MRTK_Demo.Editor
{
    [CustomPanelEditor(typeof(ScenePrefab))]
    public class ScenePrefabPanel : PanelEditor
    {
        public new ScenePrefab Instance => (ScenePrefab)base.Instance;

        private Dictionary<string, Guid> prefabsIdByName;

        private AssetsService assetsService;

        private Scene prefabScene;

        protected override void Loaded()
        {
            base.Loaded();
            this.assetsService = this.Instance.AssetsService;
            this.prefabsIdByName = WaveContentUtils.FindPrefabs();
            this.PrefabSetValue(this.PrefabGetValue());
        }

        public override void GenerateUI()
        {
            base.GenerateUI();
            this.propertyPanelContainer.AddSelector(nameof(ScenePrefab.PrefabId), "Prefab", prefabsIdByName.Keys, this.PrefabGetValue, this.PrefabSetValue);
        }

        private string PrefabGetValue()
        {
            return this.prefabsIdByName.FirstOrDefault(x => x.Value == Instance.PrefabId).Key;
        }

        private void PrefabSetValue(string x)
        {
            if (x == null)
            {
                return;
            }

            var previousPrefabId = this.Instance.PrefabId;
            this.Instance.PrefabId = this.prefabsIdByName[x];

            this.UpdatePrefabScene(this.assetsService.Load<Scene>(this.Instance.PrefabId));

            if (previousPrefabId != this.Instance.PrefabId)
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

            if (this.Instance.PrefabId != Guid.Empty)
            {
                this.prefabScene = scene;

                if (this.prefabScene != null)
                {
                    this.prefabScene.Invalidated += this.PrefabScene_Invalidated;
                }
            }
        }

        private void PrefabScene_Invalidated(object sender, WaveEngine.Common.ILoadable e)
        {
            this.Instance.RefreshEntity();
            this.UpdatePrefabScene(e as Scene);
        }
    }
}
