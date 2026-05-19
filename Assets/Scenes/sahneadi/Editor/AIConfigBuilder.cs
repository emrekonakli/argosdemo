using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Argos.AI;
using Argos.Game;

namespace Argos.EditorTools
{
    public static class AIConfigBuilder
    {
        const string MuseumScenePath = "Assets/Scenes/sahneadi/Scenes/MuseumScene.unity";
        const string ConfigFolder = "Assets/Scenes/sahneadi/ScriptableObjects";
        const string ConfigAssetPath = ConfigFolder + "/AIConfig.asset";

        [MenuItem("ARGOS/Build AI Config")]
        public static void Build()
        {
            AssetFolders.Ensure(ConfigFolder);

            var cfg = AssetDatabase.LoadAssetAtPath<AIConfig>(ConfigAssetPath);
            if (cfg == null)
            {
                cfg = ScriptableObject.CreateInstance<AIConfig>();
                AssetDatabase.CreateAsset(cfg, ConfigAssetPath);
                cfg.provider = AIProvider.OpenAI;
                cfg.apiKey = "";
                cfg.modelName = "gpt-4o-mini";
                cfg.maxTokens = 256;
                cfg.temperature = 0.7f;
                EditorUtility.SetDirty(cfg);
            }

            // MuseumScene'i aç ve Bootstrap.aiConfig'i bağla.
            var current = EditorSceneManager.GetActiveScene();
            if (current.path != MuseumScenePath)
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
                EditorSceneManager.OpenScene(MuseumScenePath, OpenSceneMode.Single);
            }

            var bootstrap = Object.FindFirstObjectByType<MuseumSceneBootstrap>();
            if (bootstrap != null)
            {
                bootstrap.aiConfig = cfg;
                EditorUtility.SetDirty(bootstrap);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            }
            else
            {
                Debug.LogWarning("[ARGOS] MuseumSceneBootstrap bulunamadı; sahne kurulu mu?");
            }

            AssetDatabase.SaveAssets();
            Debug.Log("[ARGOS] AIConfig.asset hazır. API key'i Inspector'dan doldur.");
        }
    }
}
