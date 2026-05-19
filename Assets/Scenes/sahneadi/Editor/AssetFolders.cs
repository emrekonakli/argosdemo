using System.IO;
using UnityEditor;

namespace Argos.EditorTools
{
    internal static class AssetFolders
    {
        /// <summary>
        /// Verilen 'Assets/...' yolundaki klasörü garanti eder, eksik üst klasörler
        /// recursive olarak yaratılır.
        /// </summary>
        public static void Ensure(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath)) return;
            string parent = Path.GetDirectoryName(assetPath).Replace('\\', '/');
            if (!AssetDatabase.IsValidFolder(parent)) Ensure(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(assetPath));
        }
    }
}
