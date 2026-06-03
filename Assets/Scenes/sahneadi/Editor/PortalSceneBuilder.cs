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
        const string AIConfigPath = "Assets/Scenes/sahneadi/ScriptableObjects/AIConfig.asset";

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
            bootstrap.aiConfig = AssetDatabase.LoadAssetAtPath<Argos.AI.AIConfig>(AIConfigPath);

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

            npc.npcName = "Köylü Çocuk";
            npc.period = "MÖ 800, Frigya";
            npc.portrait = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/KahinPortrait.png");
            npc.portraitMouthOpen = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/KahinPortraitOpen.png");
            npc.personality = "MÖ 800 Frigya'sında yaşayan, yırtık giysili, yoksul bir köy çocuğu. Korkmuş ve "
                + "çekingendir. Bir gece köyün yakınındaki yolda bir tüccarın at arabasının pusuya düşürülüp "
                + "devrildiğine şahit oldu ve o günden beri dehşet içinde. Çocuksu, kısa ve sade cümlelerle konuşur, "
                + "bazen kekeler. Gördüklerini anlatmaya korkar çünkü saldırganın kendisine zarar vermesinden çekinir; "
                + "güven kazanılırsa yavaş yavaş açılır. Soylulardan, büyüklerin işlerinden ve tapınaktan ürker. "
                + "Asla yalan uydurmaz; korktuğunda susar ya da 'bilmiyorum' der. Dönemine ait olmayan (modern) "
                + "şeyleri anlamaz.";
            npc.knownFacts = new List<string>
            {
                "O gece köyün yakınındaki yolda bir at arabasının devrildiğini gördüm.",
                "Arabanın atları ürkmüş, çığlık çığlığa her yere kaçışıyordu.",
                "Arabadaki yaşlı adam yere yığılmıştı, hiç kıpırdamıyordu.",
            };
            npc.hiddenFacts = new List<string>
            {
                "Devrilen arabanın yanında soylu giysili genç bir adam vardı.",
                "O genç adam arabadan bir şey alıp tapınağa doğru koştu.",
                "Çok korktuğum için bugüne dek kimseye anlatmadım.",
            };
            npc.mandatoryFinalFact = "O genç adamı tanıdım; tüccarın oğluydu. Babasının arabasını yolda devirip onu orada ölüme terk etti.";
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
