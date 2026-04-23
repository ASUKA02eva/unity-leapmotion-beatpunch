using UnityEngine;

public class RandomSkybox : MonoBehaviour
{
    // 在Inspector面板中，你可以将多个天空盒材质拖放到这个数组里
    public Material[] skyboxMaterials;

    void Start()
    {
        // 每次游戏开始（场景加载）时，随机选择一个天空盒
        ChangeSkyboxRandomly();
    }

    void ChangeSkyboxRandomly()
    {
        // 防止数组为空导致报错
        if (skyboxMaterials == null || skyboxMaterials.Length == 0)
        {
            Debug.LogWarning("天空盒材质数组是空的，请先在Inspector面板中指定！");
            return;
        }

        // 随机生成一个索引
        int randomIndex = Random.Range(0, skyboxMaterials.Length);
        // 设置场景的天空盒为随机选中的材质
        RenderSettings.skybox = skyboxMaterials[randomIndex];
    }
}