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

    // Private constructor (d��ar�dan olu�turulamaz)
    private TextureCache()
    {
        textureCache = new Dictionary<string, Texture2D>();
    }

    private Dictionary<string, Texture2D> textureCache;

    // Texture y�kleme
    public Texture2D LoadTexture(string folderPath, string fileName)
    {
        string cacheKey = $"{folderPath}/{fileName}";

        // E�er texture �nceden y�klenmi�se do�rudan d�nd�r
        if (textureCache.ContainsKey(cacheKey))
        {
            return textureCache[cacheKey];
        }

        // Texture'� Resources'dan y�kle
        string resourcePath = $"{folderPath}/{fileName}";
        Texture2D texture = Resources.Load<Texture2D>(resourcePath);

        if (texture != null)
        {
            // Cache'e ekle ve d�nd�r
            textureCache[cacheKey] = texture;
            return texture;
        }
        else
        {
            Debug.LogError($"Texture y�klenemedi: {resourcePath}");
            return null;
        }
    }

    // Cache temizleme
    public void ClearCache()
    {
        textureCache.Clear();
    }
}
