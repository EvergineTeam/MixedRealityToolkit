// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using Evergine.Framework.Graphics;
using Evergine.Framework.Managers;

namespace Evergine.MRTK.Extensions
{
    internal static class MaterialExtensions
    {
        public static Material LoadNewInstance(this Material material, AssetSceneManager assetsManager) =>
            assetsManager.Load<Material>(material.Id, true);
    }
}
