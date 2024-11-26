using UnityEditor;
using UnityEngine;

public class ConvertMaterialsToStandard : MonoBehaviour
{
    [MenuItem("Tools/Convert Materials to Standard Shader")]
    public static void ConvertMaterials()
    {
        string[] guids = AssetDatabase.FindAssets("t:Material");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
            {
                material.shader = Shader.Find("Standard");
            }
        }
        Debug.Log("All materials have been converted to Standard Shader.");
    }
}
