using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Framework;
using WaveEngine.Framework.Assets;
using WaveEngine.Framework.Assets.Importers;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Services;
using WaveEngine.Platform;

namespace WaveEngine_MRTK_Demo.Components
{
    public class ScenePrefab : Component
    {
        public bool duplicateMaterials = false;

        private AssetsDirectory assetsDirectory;

        public enum PrefabTypes
        {
            PressableRoundButton,
            Slider,
            PressableButtonPlated32x32mm,
            PressableButtonUnplated
        }

        public static System.Guid[] TypeToId =
        {
            WaveContent.Scenes.Prefabs.PressableRoundButton_wescene,
            WaveContent.Scenes.Prefabs.slider_wescene,
            WaveContent.Scenes.Prefabs.PressableButtonPlated32x32mm_wescene,
            WaveContent.Scenes.Prefabs.PressableButtonHoloLens2Unplated_wescene
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
                root.Flags = HideFlags.DontSave | HideFlags.DontShow;
                
                if (duplicateMaterials)
                {
                    foreach (MaterialComponent m in root.FindComponentsInChildren<MaterialComponent>())
                    {
                        m.Material = m.Material.Clone();
                    }
                }

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
