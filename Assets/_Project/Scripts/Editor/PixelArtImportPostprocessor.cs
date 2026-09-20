using UnityEditor;
using UnityEngine;

namespace Rebonk.EditorTools
{
    /// <summary>
    /// Applies pixel-art import defaults to every new texture under Assets/_Project/Art.
    /// Only runs on first import so per-asset tweaks are preserved.
    /// </summary>
    public class PixelArtImportPostprocessor : AssetPostprocessor
    {
        private const string ArtRoot = "Assets/_Project/Art/";
        public const int PixelsPerUnit = 16;

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(ArtRoot) || !assetImporter.importSettingsMissing)
                return;

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = PixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Compressed;
            importer.compressionQuality = 100;
            importer.maxTextureSize = 2048;

            var android = importer.GetPlatformTextureSettings("Android");
            android.overridden = true;
            android.format = TextureImporterFormat.ASTC_4x4;
            importer.SetPlatformTextureSettings(android);

            var ios = importer.GetPlatformTextureSettings("iPhone");
            ios.overridden = true;
            ios.format = TextureImporterFormat.ASTC_4x4;
            importer.SetPlatformTextureSettings(ios);
        }
    }
}
