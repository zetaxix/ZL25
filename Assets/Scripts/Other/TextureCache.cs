using System.Collections.Generic;
using UnityEngine;

public class TextureCache
{
    // Singleton instance
    private static TextureCache _instance;
    public static TextureCache Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new TextureCache();
            }
            return _instance;
        }
    }

    // Private constructor (dýþarýdan oluþturulamaz)
    private TextureCache()
    {
        textureCache = new Dictionary<string, Texture2D>();
    }

    private Dictionary<string, Texture2D> textureCache;

    // Texture yükleme
    public Texture2D LoadTexture(string folderPath, string fileName)
    {
        string cacheKey = $"{folderPath}/{fileName}";

        // Eðer texture önceden yüklenmiþse doðrudan döndür
        if (textureCache.ContainsKey(cacheKey))
        {
            return textureCache[cacheKey];
        }

        // Texture'ý Resources'dan yükle
        string resourcePath = $"{folderPath}/{fileName}";
        Texture2D texture = Resources.Load<Texture2D>(resourcePath);

        if (texture != null)
        {
            // Cache'e ekle ve döndür
            textureCache[cacheKey] = texture;
            return texture;
        }
        else
        {
            Debug.LogError($"Texture yüklenemedi: {resourcePath}");
            return null;
        }
    }

    // Cache temizleme
    public void ClearCache()
    {
        textureCache.Clear();
    }
}
