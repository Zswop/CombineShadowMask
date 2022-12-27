using UnityEngine;

public class CombineShadowMaskTest : MonoBehaviour
{
    public LightmapData[] lightmapDatas;

    [ContextMenu("ClearLightmaps")]
    void ClearLightmaps()
    {
        lightmapDatas = LightmapSettings.lightmaps;
        LightmapSettings.lightmaps = null;
    }

    [ContextMenu("RevertLightmaps")]

    void RevertLightmaps()
    {
        LightmapSettings.lightmaps = lightmapDatas;
    }

    [ContextMenu("CombineLightmaps")]

    void CombineLightmaps()
    {
        ByteDance.Picoverse.RenderPipeline.CombineShadowMaskHelper.CombineShadowMaskToLightmap();
    }
}
