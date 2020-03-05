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

namespace WaveEngine_MRTK_Demo.Behaviors
{
    public class ScenePrefab : Component
    {
        public bool duplicateMaterials = false;

        private AssetsDirectory assetsDirectory;

        public enum PrefabTypes
        {
            PressableRoundButton,
            Slider
        }

        public static System.Guid[] TypeToId =
        {
            WaveContent.Scenes.Prefabs.PressableRoundButton_wescene,
            WaveContent.Scenes.Prefabs.slider_wescene
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
                
                if (duplicateMaterials)
                {
                    foreach (MaterialComponent m in root.FindComponentsInChildren<MaterialComponent>())
                    {
                        m.Material = this.CopyMaterial(m.Material);
                    }
                }

                this.Owner.AddChild(root);
            }

            return true;
        }

        private unsafe Material CopyMaterial(Material material)
        {
            Material copy = new Material(material.Effect);
            copy.ActiveDirectivesNames = material.ActiveDirectivesNames;
            copy.LayerDescription = material.LayerDescription;
            copy.OrderBias = material.OrderBias;
            copy.AllowInstancing = material.AllowInstancing;

            for (int c = 0; c < material.CBuffers.Length; c++)
            {                
                void* copyData = (void*)copy.CBuffers[c].Data;
                void* data = (void*)material.CBuffers[c].Data;
                uint size = material.CBuffers[c].Size;

                Unsafe.CopyBlock(copyData, data, size);
                copy.CBuffers[c].Dirty = true;
            }

            for (int t = 0; t < material.TextureSlots.Length; t++)
            {
                copy.TextureSlots[t] = material.TextureSlots[t];
            }

            for (int s = 0; s < material.SamplerSlots.Length; s++)
            {
                copy.SamplerSlots[s] = material.SamplerSlots[s];
            }            

            return copy;
        }

        private string GetAssetPath(Guid id)
        {
            return this.assetsDirectory.EnumerateFiles(string.Empty, $"{id}.*", SearchOption.AllDirectories).FirstOrDefault();
        }
    }
}
