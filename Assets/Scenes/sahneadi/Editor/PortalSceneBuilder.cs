using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Argos.Game;
using Argos.NPC;

namespace Argos.EditorTools
{
    public static class PortalSceneBuilder
    {
        const string MuseumScenePath = "Assets/Scenes/sahneadi/Scenes/MuseumScene.unity";
        const string PortalScenePath = "Assets/Scenes/sahneadi/Scenes/PortalScene.unity";
        const string NpcFolder = "Assets/Scenes/sahneadi/ScriptableObjects/NPCs";
        const string PortalNpcPath = NpcFolder + "/NPC_KahinBilge.asset";

        [MenuItem("ARGOS/Build Portal Scene")]
        public static void Build()
        {
            if (EditorSceneManager.GetActiveScene().isDirty)
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            }

            AssetFolders.Ensure(NpcFolder);
            var npc = UpsertPortalNpc();

            // PortalScene'i yarat (varsa overwrite).
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var bootstrapGo = new GameObject("Bootstrap");
            var bootstrap = bootstrapGo.AddComponent<PortalSceneBootstrap>();
            bootstrap.buildOnStart = true;
            bootstrap.portalNPC = npc;

            EditorSceneManager.MarkSceneDirty(newScene);
            EditorSceneManager.SaveScene(newScene, PortalScenePath);

            UpdateBuildSettings();
            AssetDatabase.SaveAssets();

            // Kullanıcıyı MuseumScene'e geri al ki sıradaki adımda kafa karışmasın.
            EditorSceneManager.OpenScene(MuseumScenePath, OpenSceneMode.Single);

            Debug.Log("[ARGOS] Portal scene built; NPC asset created; Build Settings updated.");
        }

        static NPCData UpsertPortalNpc()
        {
            var npc = AssetDatabase.LoadAssetAtPath<NPCData>(PortalNpcPath);
            if (npc == null)
            {
                npc = ScriptableObject.CreateInstance<NPCData>();
                AssetDatabase.CreateAsset(npc, PortalNpcPath);
            }

            npc.npcName = "Kâhin Bilge";
            npc.period = "MÖ 800, Frigya";
            npc.personality = "Yaşlı, sözünü esirgemez ama doğrudan cevap vermekten kaçınan bir kâhin.";
            npc.knownFacts = new List<string>
            {
                "Tunç Kupa son kez tapınakta görüldü.",
                "Kupa sahibi yabancı bir tüccardı.",
                "O gece tapınakta üç kişi vardı.",
            };
            npc.hiddenFacts = new List<string>
            {
                "Tüccarın oğlu cinayet anında oradaydı.",
                "Kupanın kenarındaki kan tüccara aitti.",
            };
            npc.mandatoryFinalFact = "Tüccarın oğlu, babasını kupayla zehirledi.";
            npc.patienceCount = 6;
            EditorUtility.SetDirty(npc);
            return npc;
        }

        static void UpdateBuildSettings()
        {
            var scenes = new List<EditorBuildSettingsScene>();
            void AddIfMissing(string path)
            {
                if (string.IsNullOrEmpty(path)) return;
                if (scenes.Exists(s => s.path == path)) return;
                scenes.Add(new EditorBuildSettingsScene(path, true));
            }

            AddIfMissing(MuseumScenePath);
            AddIfMissing(PortalScenePath);

            // Önceki Build Settings'te kalan diğer sahneleri (örn. SampleScene) koru.
            foreach (var existing in EditorBuildSettings.scenes)
            {
                if (existing.path == MuseumScenePath || existing.path == PortalScenePath) continue;
                scenes.Add(existing);
            }

            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
