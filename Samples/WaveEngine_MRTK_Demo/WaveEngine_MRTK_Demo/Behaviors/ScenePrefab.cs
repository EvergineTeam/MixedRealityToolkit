using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WaveEngine.Framework;
using WaveEngine.Framework.Assets;
using WaveEngine.Framework.Assets.Importers;
using WaveEngine.Framework.Services;
using WaveEngine.Platform;

namespace WaveEngine_MRTK_Demo.Behaviors
{
    public class ScenePrefab : Component
    {
        private AssetsDirectory assetsDirectory;

        public enum PrefabTypes
        {
            Button,
            Slider,
        }

        public System.Guid[] TypeToId =
        {
            WaveContent.Scenes.Prefabs.RoundButton_wescene
        };


        public PrefabTypes prefabType;

        protected override bool OnAttached()
        {
            //if (!Application.Current.IsEditor)
            {
                //Remove all children first
                while (this.Owner.NumChildren > 0)
                {
                    this.Owner.RemoveChild(this.Owner.ChildEntities.First());
                }

                this.assetsDirectory = Application.Current.Container.Resolve<AssetsDirectory>();
                WaveSceneImporter importer = new WaveSceneImporter();
                SceneSource source = new SceneSource();

                string path = this.GetAssetPath(TypeToId[(int)prefabType]);
                using (var stream = assetsDirectory.Open(path))
                {
                    importer.ImportHeader(stream, out source);
                    importer.ImportData(stream, source, false);
                }

                Entity root = source.SceneData.Items.First().Entity;
                this.Owner.AddChild(root);
            }

            return true;
        }

        private string GetAssetPath(Guid id)
        {
            return this.assetsDirectory.EnumerateFiles(string.Empty, $"{id}.*", SearchOption.AllDirectories).FirstOrDefault();
        }
    }
}
