#if UNITY_EDITOR
using UnityEditor;

namespace ChoDoLa.Sprint1.Editor
{
    /// <summary>
    /// Ensures baked Blender normal maps import as Tangent-Space Normal Maps
    /// and Blender FBX models import cleanly at 1:1 scale for URP.
    /// </summary>
    public class BlenderAssetPostprocessor : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (!assetPath.Contains("Resources/Blender/Textures/")) return;

            TextureImporter importer = (TextureImporter)assetImporter;
            importer.wrapMode = UnityEngine.TextureWrapMode.Repeat;
            importer.filterMode = UnityEngine.FilterMode.Bilinear;
            importer.anisoLevel = 4;

            if (assetPath.EndsWith("_Normal.png"))
            {
                importer.textureType = TextureImporterType.NormalMap;
                importer.sRGBTexture = false;
            }
            else
            {
                importer.textureType = TextureImporterType.Default;
                importer.sRGBTexture = true;
            }
        }

        private void OnPreprocessModel()
        {
            if (!assetPath.Contains("Resources/Blender/Models/")) return;

            ModelImporter importer = (ModelImporter)assetImporter;
            importer.globalScale = 1.0f;
            importer.useFileScale = true;
            importer.importAnimation = false;
            importer.importLights = false;
            importer.importCameras = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
        }
    }
}
#endif
