//Written by Jiayun Li
//Copyright (c) 2022

using UnityEditor;
using UnityEngine;

namespace ByteDance.Picoverse.RenderPipeline
{
    public class AssetImporter : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            SeTextureSettings(assetImporter as TextureImporter, assetPath);
        }

        private static void SeTextureSettings(TextureImporter importer, string assetPath)
        {
            if (importer == null || string.IsNullOrEmpty(assetPath))
                return;

            bool isWorldAsset = AssetUtils.IsWorldAsset(assetPath);
            bool isAvatarAsset = AssetUtils.IsAvatarAsset(assetPath);
            if (!isWorldAsset && !isAvatarAsset)
                return;

            bool isEditable = AssetUtils.IsEditable(assetPath);
            if (isEditable)
                return;
            
            bool hasAlpha = importer.DoesSourceTextureHaveAlpha();

            bool isReflectionProbe = AssetUtils.IsReflectionProbe(assetPath);
            bool isShadowMask = AssetUtils.IsShadowMask(assetPath);
            bool isLightmap = AssetUtils.IsLightmap(assetPath);
            if (isReflectionProbe || isLightmap || isShadowMask)
            {
                SetPlatformTextureSettings(importer, true, hasAlpha);
                return;
            }

            bool isAlbedoMap = AssetUtils.IsAlbedoMap(assetPath); 
            bool isEmmisionMap = AssetUtils.IsEmissionMap(assetPath);
            if (isAlbedoMap || isEmmisionMap)
            {
                importer.isReadable = false;
                importer.mipmapEnabled = true;
                importer.streamingMipmaps = true;
                
                importer.sRGBTexture = true;
                importer.textureType = TextureImporterType.Default;

                SetPlatformTextureSettings(importer, false, hasAlpha);
                return;
            }

            bool isNormalMap = AssetUtils.IsNormalMap(assetPath);
            if (isNormalMap)
            {
                importer.isReadable = false;
                importer.mipmapEnabled = true;
                importer.streamingMipmaps = true;

                importer.sRGBTexture = false;
                importer.textureType = TextureImporterType.NormalMap;

                SetPlatformTextureSettings(importer, true, hasAlpha);
                return;
            }
        }

        private static void SetPlatformTextureSettings(TextureImporter importer, bool compressedHQ, bool hasAlpha = true)
        {
            TextureImporterFormat importFormat = TextureImporterFormat.ARGB32;
            importFormat = compressedHQ ? TextureImporterFormat.ASTC_5x5 : TextureImporterFormat.ASTC_6x6;

            SetPlatformTextureSettings(importer, "Android", importFormat, 2048);
            SetPlatformTextureSettings(importer, "iOS", importFormat, 2048);
        }

        private static void SetPlatformTextureSettings(TextureImporter importer, string platfom, TextureImporterFormat format, int maxTextureSize = 2048)
        {
            TextureImporterPlatformSettings platformSetting = importer.GetPlatformTextureSettings(platfom);
            if (platformSetting != null)
            {
                var newPlatfomSetting = new TextureImporterPlatformSettings();
                platformSetting.CopyTo(newPlatfomSetting);

                newPlatfomSetting.overridden = true;
                newPlatfomSetting.format = format;

                newPlatfomSetting.maxTextureSize = platformSetting.maxTextureSize;
                if (newPlatfomSetting.maxTextureSize > maxTextureSize)
                    newPlatfomSetting.maxTextureSize = maxTextureSize;

                importer.SetPlatformTextureSettings(newPlatfomSetting);
            }
        }
    }
}