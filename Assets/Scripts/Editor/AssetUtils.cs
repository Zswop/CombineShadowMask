//Written by Jiayun Li
//Copyright (c) 2022

namespace ByteDance.Picoverse.RenderPipeline
{
    public class AssetUtils
    {
        public static bool IsWorldAsset(string name)
        {
            return name.Contains("/WorldAssets/") || name.Contains("/map_00000_newhome/");
        }

        public static bool IsAvatarAsset(string name)
        {
            return name.Contains("/Avatar/");
        }

        public static bool IsEditable(string name)
        {
            return name.Contains("/Editable/");
        }

        public  static bool IsAlbedoMap(string name)
        {
            return name.Contains("_D.");
        }

        public static bool IsNormalMap(string name)
        {
            return name.Contains("_N.");
        }

        public static bool IsMetallicGlossMap(string name)
        {
            return name.Contains("_M.");
        }

        public static bool IsEmissionMap(string name)
        {
            return name.Contains("_E.");
        }

        public static bool IsLightmap(string name)
        {
            return name.StartsWith("Lightmap-") && name.Contains("_light.");
        }

        public static bool IsShadowMask(string name)
        {
            return name.StartsWith("Lightmap-") && name.Contains("_shadowmask.");
        }

        public static bool IsReflectionProbe(string name)
        {
            return name.StartsWith("ReflectionProbe-");
        }

        public static bool IsSky(string name)
        {
            return name.Contains("_Sky_");
        }

    }
}