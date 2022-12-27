//Written by Jiayun Li
//Copyright (c) 2022

using UnityEngine;
using UnityEditor;

namespace ByteDance.Picoverse.RenderPipeline
{
    public class CombineShadowMaskHelper
    {
        [InitializeOnLoadMethod]
        private static void InitializeOnLoad()
        {
            Lightmapping.bakeCompleted -= OnBakeCompleted;
            Lightmapping.bakeCompleted += OnBakeCompleted;
        }

        private static void OnBakeCompleted()
        {
            Debug.Log($"============================================================= Combine ShadowMask To Lightmap");
            CombineShadowMaskToLightmap();
        }

        [MenuItem("Tools/Scene/CombineShadowMask")]
        public static bool CombineShadowMaskToLightmap()
        {
            var lightmapDatas = LightmapSettings.lightmaps;
            if (lightmapDatas != null)
            {
                var newData = new LightmapData[lightmapDatas.Length];
                for (int i = 0; i < lightmapDatas.Length; ++i)
                {
                    var data = lightmapDatas[i];

                    if (data.lightmapColor == null || data.shadowMask == null)
                    {
                        Debug.LogError("The lightMap or shadowMask should not be null. Perhaps LightingMode setting is not correct");
                        return false;
                    }

                    var path = AssetDatabase.GetAssetPath(data.lightmapColor);
                    if (string.IsNullOrEmpty(path))
                    {
                        Debug.LogError($"{ data.lightmapColor } asset doest not extist!");
                        return false;
                    }

                    var combinedTexture = ReplaceAlphaWithR(data.lightmapColor, data.shadowMask);
                    combinedTexture = ReplaceAsset(path, combinedTexture);
                    newData[i] = new LightmapData()
                    {
                        lightmapColor = combinedTexture,
                        lightmapDir = data.lightmapDir,
                        shadowMask = data.shadowMask
                    };
                }

                LightmapSettings.lightmaps = newData;
                return true;
            }
            return false;
        }

        private static Texture2D ReplaceAlphaWithR(Texture2D t1, Texture2D t2)
        {
            if (t1.width != t2.width || t1.height != t2.height)
            {
                Debug.LogError("The two textues should have same size.");
                return null;
            }

            Debug.Log($"============================================================= { t1.name }, { t2.name }!");

            var dt1 = Duplicate(t1, RenderTextureReadWrite.sRGB);
            var dt2 = Duplicate(t2, RenderTextureReadWrite.Linear);

            for (int i = 0; i < dt1.width; ++i)
            {
                for (int j = 0; j < dt1.height; ++j)
                {
                    var p1 = GetLightmapColor(dt1, i, j);
                    var p2 = dt2.GetPixel(i, j);
                    dt1.SetPixel(i, j, new Color(p1.r, p1.g, p1.b, p2.r));
                }
            }
            dt1.Apply();

            return dt1;
        }

        private static Color GetLightmapColor(Texture2D t, int i, int j)
        {
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows ||
                EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows64)
            {
                Color p = t.GetPixel(i, j);

                Vector3 dLDR;
                bool ligthmapGamma = true;
                if (ligthmapGamma)
                {
                    Vector3 unpack = p.a * 5 * new Vector3(p.r, p.g, p.b); // unpack RGBM encoding
                    dLDR = unpack / 2f;  // pack dLDR encoding
                }
                else
                {
                    Vector3 unpack = Mathf.Pow(p.a, 2.2f) * 34.493242f * new Vector3(p.r, p.g, p.b); //unpack RGBM encoding
                    dLDR = unpack / 4.59f;  // pack dLDR encoding
                }

                return new Color(dLDR.x, dLDR.y, dLDR.z);
            }

            return t.GetPixel(i, j);
        }

        private static void SetTextureSettings(string path)
        {
            TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(path);

            importer.textureType = TextureImporterType.Default;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;

            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
        }

        private static Texture2D ReplaceAsset(string path, Texture2D dst)
        {
            var dir = System.IO.Path.GetDirectoryName(path);
            var extension = System.IO.Path.GetExtension(path);
            var fileName = System.IO.Path.GetFileNameWithoutExtension(path);

            var bkpPath = System.IO.Path.Combine(dir, fileName + "_bkp" + extension);
            AssetDatabase.CopyAsset(path, bkpPath);

            //Texture2D test = new Texture2D(dst.width, dst.height);
            //for (int i = 0; i < dst.width; ++i)
            //{
            //    for (int j = 0; j < dst.height; ++j)
            //    {
            //        var p = dst.GetPixel(i, j);
            //        test.SetPixel(i, j, new Color(p.a, 0, 0, 0));
            //    }
            //}
            //test.Apply();
            //var testPath = System.IO.Path.Combine(dir, fileName + "_test.png");
            //System.IO.File.WriteAllBytes(testPath, test.EncodeToPNG());

            System.IO.File.WriteAllBytes(path, dst.EncodeToTGA());
            AssetDatabase.Refresh();

            SetTextureSettings(path);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static Texture2D Duplicate(Texture2D source, RenderTextureReadWrite readWrite)
        {
            RenderTexture renderTex = RenderTexture.GetTemporary(
                        source.width,
                        source.height,
                        0,
                        RenderTextureFormat.ARGB32,
                        readWrite);

            Graphics.Blit(source, renderTex);

            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;

            Texture2D readableText = new Texture2D(source.width, source.height);
            readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            readableText.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);

            return readableText;
        }

        private static void SetTextureReadable(Texture2D t)
        {
            var path = AssetDatabase.GetAssetPath(t);

            TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(path);
            importer.isReadable = true;
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
        }
    }
}