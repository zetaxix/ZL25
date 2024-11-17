using UnityEngine;
using UnityEditor;
using System.IO;

public class OptimizeTextures : EditorWindow
{
    private int maxFileSizeKB = 100; // Varsayýlan deðer 100 KB
    private const TextureImporterCompression compressionType = TextureImporterCompression.Compressed; // Sýkýþtýrma türü

    [MenuItem("Tools/Optimize MyRepository Textures")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(OptimizeTextures));
    }

    private void OnGUI()
    {
        GUILayout.Label("Texture Optimizer for MyRepository", EditorStyles.boldLabel);

        // Kullanýcýnýn Max File Size'ý girmesi için input alaný
        GUILayout.Label("Max File Size (KB):", EditorStyles.label);
        maxFileSizeKB = EditorGUILayout.IntField("Max File Size (KB):", maxFileSizeKB);

        if (GUILayout.Button("Optimize MyRepository Textures"))
        {
            OptimizeMyRepositoryTextures();
        }
    }

    private void OptimizeMyRepositoryTextures()
    {
        // Sadece "Assets/MyRepository" altýndaki texture dosyalarýný bul
        string[] allTexturePaths = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/MyRepository" });

        foreach (string guid in allTexturePaths)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;

            if (textureImporter != null)
            {
                OptimizeTexture(textureImporter, assetPath);
            }
        }

        AssetDatabase.Refresh();
        Debug.Log("MyRepository texture optimization completed!");
    }

    private void OptimizeTexture(TextureImporter textureImporter, string assetPath)
    {
        string fullPath = Path.Combine(Application.dataPath, assetPath.Replace("Assets/", ""));

        // Ýlk olarak sýkýþtýrmayý ve çözünürlüðü uygula
        textureImporter.textureCompression = compressionType;
        textureImporter.isReadable = true; // Okunabilir yap
        textureImporter.mipmapEnabled = false; // Mipmap'leri kapat
        textureImporter.filterMode = FilterMode.Bilinear;

        // Çözünürlük ayarlarýný düþürerek kullanýcý tarafýndan belirlenen 100 KB'nin altýna inmeye çalýþ
        int maxSize = 2048; // Baþlangýç çözünürlüðü
        bool optimized = false;

        while (maxSize > 32) // Minimum çözünürlük sýnýrý
        {
            textureImporter.maxTextureSize = maxSize;
            AssetDatabase.ImportAsset(assetPath); // Yeniden içe aktar

            FileInfo fileInfo = new FileInfo(fullPath);

            if (fileInfo.Length <= maxFileSizeKB * 1024)
            {
                optimized = true;
                Debug.Log($"Optimized {assetPath} to {maxSize}px, Size: {fileInfo.Length / 1024} KB");
                break;
            }

            maxSize /= 2; // Çözünürlüðü yarýya indir
        }

        if (!optimized)
        {
            Debug.LogWarning($"Unable to optimize {assetPath} below {maxFileSizeKB} KB. Current Size: {new FileInfo(fullPath).Length / 1024} KB");
        }

        // Asset'leri yenileyin ve deðiþiklikleri uygulayýn
        AssetDatabase.Refresh(); // Veritabanýný yenileyin
        AssetDatabase.ImportAsset(assetPath); // Asset'i yeniden içe aktarýn
    }
}