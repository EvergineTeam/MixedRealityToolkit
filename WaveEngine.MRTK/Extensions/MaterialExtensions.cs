// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Managers;

namespace WaveEngine.MRTK.Extensions
{
    internal static class MaterialExtensions
    {
        public static Material LoadNewInstance(this Material material, AssetSceneManager assetsManager) =>
            assetsManager.Load<Material>(material.Id, true);
    }
}
