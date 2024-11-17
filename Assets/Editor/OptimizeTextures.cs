using UnityEngine;
using UnityEditor;
using System.IO;

public class OptimizeTextures : EditorWindow
{
    private int maxFileSizeKB = 100; // Varsay�lan de�er 100 KB
    private const TextureImporterCompression compressionType = TextureImporterCompression.Compressed; // S�k��t�rma t�r�

    [MenuItem("Tools/Optimize MyRepository Textures")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(OptimizeTextures));
    }

    private void OnGUI()
    {
        GUILayout.Label("Texture Optimizer for MyRepository", EditorStyles.boldLabel);

        // Kullan�c�n�n Max File Size'� girmesi i�in input alan�
        GUILayout.Label("Max File Size (KB):", EditorStyles.label);
        maxFileSizeKB = EditorGUILayout.IntField("Max File Size (KB):", maxFileSizeKB);

        if (GUILayout.Button("Optimize MyRepository Textures"))
        {
            OptimizeMyRepositoryTextures();
        }
    }

    private void OptimizeMyRepositoryTextures()
    {
        // Sadece "Assets/MyRepository" alt�ndaki texture dosyalar�n� bul
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

        // �lk olarak s�k��t�rmay� ve ��z�n�rl��� uygula
        textureImporter.textureCompression = compressionType;
        textureImporter.isReadable = true; // Okunabilir yap
        textureImporter.mipmapEnabled = false; // Mipmap'leri kapat
        textureImporter.filterMode = FilterMode.Bilinear;

        // ��z�n�rl�k ayarlar�n� d���rerek kullan�c� taraf�ndan belirlenen 100 KB'nin alt�na inmeye �al��
        int maxSize = 2048; // Ba�lang�� ��z�n�rl���
        bool optimized = false;

        while (maxSize > 32) // Minimum ��z�n�rl�k s�n�r�
        {
            textureImporter.maxTextureSize = maxSize;
            AssetDatabase.ImportAsset(assetPath); // Yeniden i�e aktar

            FileInfo fileInfo = new FileInfo(fullPath);

            if (fileInfo.Length <= maxFileSizeKB * 1024)
            {
                optimized = true;
                Debug.Log($"Optimized {assetPath} to {maxSize}px, Size: {fileInfo.Length / 1024} KB");
                break;
            }

            maxSize /= 2; // ��z�n�rl��� yar�ya indir
        }

        if (!optimized)
        {
            Debug.LogWarning($"Unable to optimize {assetPath} below {maxFileSizeKB} KB. Current Size: {new FileInfo(fullPath).Length / 1024} KB");
        }

        // Asset'leri yenileyin ve de�i�iklikleri uygulay�n
        AssetDatabase.Refresh(); // Veritaban�n� yenileyin
        AssetDatabase.ImportAsset(assetPath); // Asset'i yeniden i�e aktar�n
    }
}