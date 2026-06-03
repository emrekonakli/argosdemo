using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Argos.AI;
using Argos.Game;
using Argos.Scenarios;

namespace Argos.EditorTools
{
    public static class NewspaperSceneBuilder
    {
        const string NewspaperScenePath = "Assets/Scenes/sahneadi/Scenes/NewspaperScene.unity";
        const string OfficeScenePath = "Assets/Scenes/sahneadi/Scenes/OfficeScene.unity";
        const string MuseumScenePath = "Assets/Scenes/sahneadi/Scenes/MuseumScene.unity";
        const string PortalScenePath = "Assets/Scenes/sahneadi/Scenes/PortalScene.unity";
        const string ScenarioPath = "Assets/Scenes/sahneadi/ScriptableObjects/Scenarios/Senaryo_FrigyaCinayeti.asset";
        const string AIConfigPath = "Assets/Scenes/sahneadi/ScriptableObjects/AIConfig.asset";

        [MenuItem("ARGOS/Build Newspaper Scene")]
        public static void Build()
        {
            if (EditorSceneManager.GetActiveScene().isDirty)
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            }

            var scenario = AssetDatabase.LoadAssetAtPath<ScenarioData>(ScenarioPath);
            var aiCfg = AssetDatabase.LoadAssetAtPath<AIConfig>(AIConfigPath);

            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var bootstrapGo = new GameObject("Bootstrap");
            var bootstrap = bootstrapGo.AddComponent<NewspaperSceneBootstrap>();
            bootstrap.buildOnStart = true;
            bootstrap.isEndingScene = false;
            bootstrap.isCorrect = false;
            bootstrap.aiConfig = aiCfg;
            bootstrap.scenario = scenario;

            EditorSceneManager.MarkSceneDirty(newScene);
            EditorSceneManager.SaveScene(newScene, NewspaperScenePath);

            UpdateBuildSettings();
            SetPlayModeStartScene();
            AssetDatabase.SaveAssets();

            EditorSceneManager.OpenScene(MuseumScenePath, OpenSceneMode.Single);
            Debug.Log("[ARGOS] NewspaperScene built; Build Settings + playModeStartScene güncellendi (Editor'de Play her zaman OfficeScene'den başlar).");
        }

        static void SetPlayModeStartScene()
        {
            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(OfficeScenePath);
            if (sceneAsset != null) EditorSceneManager.playModeStartScene = sceneAsset;
        }

        /// <summary>
        /// Unity yeniden başladığında playModeStartScene'i OfficeScene'e bağlar.
        /// Eskiden NewspaperScene'i zorluyordu; artık giriş sahnesi Office.
        /// </summary>
        [InitializeOnLoadMethod]
        static void EnsurePlayModeStartScene()
        {
            var officeAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(OfficeScenePath);
            if (officeAsset == null) return;
            if (EditorSceneManager.playModeStartScene == officeAsset) return;
            EditorSceneManager.playModeStartScene = officeAsset;
        }

        static void UpdateBuildSettings()
        {
            // OfficeScene ilk index — oyun açılınca o yüklenecek.
            var ordered = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene(OfficeScenePath, true),
                new EditorBuildSettingsScene(NewspaperScenePath, true),
                new EditorBuildSettingsScene(MuseumScenePath, true),
                new EditorBuildSettingsScene(PortalScenePath, true),
            };
            // Diğer korumalı sahneleri ekle.
            foreach (var existing in EditorBuildSettings.scenes)
            {
                if (ordered.Exists(s => s.path == existing.path)) continue;
                ordered.Add(existing);
            }
            EditorBuildSettings.scenes = ordered.ToArray();
        }
    }
}
