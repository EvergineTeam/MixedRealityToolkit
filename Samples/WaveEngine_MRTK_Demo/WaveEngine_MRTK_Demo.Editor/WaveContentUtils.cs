using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WaveEngine.Framework.Assets.Importers;

namespace WaveEngine_MRTK_Demo.Editor
{
    public static class WaveContentUtils
    {
        public static Dictionary<string, string> FindFonts()
        {
            return GetWaveContentTypes()
                    .SelectMany(x => GetClasses(x, string.Empty))
                    .SelectMany(x => GetFontFamilyNameFields(x.type, x.basePath))
                    .ToDictionary(x => x.name, x => x.sourcePath);
        }

        public static Dictionary<string, Guid> FindPrefabs()
        {
            return GetWaveContentTypes()
                    .SelectMany(x => GetClasses(x, filter: (t) => t.Name.ToLowerInvariant().Contains("prefab")))
                    .SelectMany(x => GetScenePrefabsFields(x.type))
                    .ToDictionary(x => x.name, x => x.id);
        }

        private static IEnumerable<TypeInfo> GetWaveContentTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                                          .SelectMany(x => x.DefinedTypes.Where(y => y.Name == "WaveContent"));
        }

        private static IEnumerable<(TypeInfo type, string basePath)> GetClasses(TypeInfo x, string basePath = null, Func<TypeInfo, bool> filter = null)
        {
            if (filter?.Invoke(x) != false)
            {
                yield return (x, basePath);
            }

            foreach (var item in x.DeclaredNestedTypes)
            {
                var subBasePath = basePath == null ? null : $"{basePath}/{item.Name}";
                foreach (var nestedType in GetClasses(item, subBasePath, filter))
                {
                    yield return nestedType;
                }
            }
        }

        private static IEnumerable<(string name, string sourcePath)> GetFontFamilyNameFields(TypeInfo type, string basePath)
        {
            return type.DeclaredFields.Where(field => field.Name.EndsWith("_ttf"))
                                      .Select(field => field.Name.Remove(field.Name.IndexOf('_')))
                                      .Distinct()
                                      .Select(name => (name, $"{basePath}/#{name}"));
        }

        private static IEnumerable<(string name, Guid id)> GetScenePrefabsFields(TypeInfo type)
        {
            var sceneExtension = SceneImporter.FileExtension.Replace('.', '_');
            return type.DeclaredFields.Where(x => x.Name.EndsWith(sceneExtension))
                .Select(x => (x.Name.Substring(0, x.Name.Length - sceneExtension.Length), (Guid)x.GetValue(null)));
        }
    }
}
