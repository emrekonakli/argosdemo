using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Argos.Game;
using Argos.NPC;
using Argos.Scenarios;

namespace Argos.EditorTools
{
    public static class ScenarioBuilder
    {
        const string MuseumScenePath = "Assets/Scenes/sahneadi/Scenes/MuseumScene.unity";
        const string NpcFolder = "Assets/Scenes/sahneadi/ScriptableObjects/NPCs";
        const string ScenarioFolder = "Assets/Scenes/sahneadi/ScriptableObjects/Scenarios";

        [MenuItem("ARGOS/Build Scenario")]
        public static void Build()
        {
            AssetFolders.Ensure(NpcFolder);
            AssetFolders.Ensure(ScenarioFolder);

            var kahin = UpsertKahin();
            var tuccarOglu = UpsertTuccarOglu();
            var bekci = UpsertBekci();

            var scenario = UpsertScenario(tuccarOglu);

            // MuseumScene'i aç ve Bootstrap'a bağla.
            var current = EditorSceneManager.GetActiveScene();
            if (current.path != MuseumScenePath)
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
                EditorSceneManager.OpenScene(MuseumScenePath, OpenSceneMode.Single);
            }

            var bootstrap = Object.FindFirstObjectByType<MuseumSceneBootstrap>();
            if (bootstrap != null)
            {
                bootstrap.scenario = scenario;
                bootstrap.suspectsForCaseBoard = new List<NPCData> { kahin, tuccarOglu, bekci };
                EditorUtility.SetDirty(bootstrap);
                var active = EditorSceneManager.GetActiveScene();
                EditorSceneManager.MarkSceneDirty(active);
                EditorSceneManager.SaveScene(active);
            }
            else
            {
                Debug.LogWarning("[ARGOS] MuseumSceneBootstrap bulunamadı.");
            }

            AssetDatabase.SaveAssets();
            Debug.Log("[ARGOS] Scenario + 3 NPCData + Bootstrap bağlantısı hazır.");
        }

        static NPCData UpsertKahin()
        {
            // Faz 7'de yaratıldı; kontrol et + alanları güncelle.
            string path = NpcFolder + "/NPC_KahinBilge.asset";
            var npc = LoadOrCreate(path);
            npc.npcName = "Köylü Çocuk";
            npc.period = "MÖ 800, Frigya";
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

        static NPCData UpsertTuccarOglu()
        {
            string path = NpcFolder + "/NPC_TuccarOglu.asset";
            var npc = LoadOrCreate(path);
            npc.npcName = "Tüccar Oğlu Lyrios";
            npc.period = "MÖ 800, Frigya";
            npc.personality = "Genç, sinirli, savunmacı. Babasının ölümünden sonra mirasa konmaya yeltenmiş.";
            npc.knownFacts = new List<string>
            {
                "Babam o gece tapınaktaydı.",
                "Kupayı ben de görmüştüm ama ona ben vermedim.",
            };
            npc.hiddenFacts = new List<string>
            {
                "Kupanın içine zehir koyan bendim.",
                "Babamla miras yüzünden tartışmıştık.",
                "Cinayet sonrası kupayı tapınağa geri bıraktım.",
            };
            npc.mandatoryFinalFact = "Babamı ben öldürdüm. Kupayı zehirledim.";
            npc.patienceCount = 8; // suçlu daha dirençli
            EditorUtility.SetDirty(npc);
            return npc;
        }

        static NPCData UpsertBekci()
        {
            string path = NpcFolder + "/NPC_TapinakBekcisi.asset";
            var npc = LoadOrCreate(path);
            npc.npcName = "Tapınak Bekçisi Aramis";
            npc.period = "MÖ 800, Frigya";
            npc.personality = "Görevine sadık, ama o gece nöbette uyukladığı için suçluluk duyan bir orta yaşlı adam.";
            npc.knownFacts = new List<string>
            {
                "Tüccar tapınağa kupasını bırakmak için geldi.",
                "Geç saatte tapınağı kilitledim.",
            };
            npc.hiddenFacts = new List<string>
            {
                "Tüccarın oğlunu da gece içeri aldım, kimseye söylemedim.",
                "Nöbette uyukladım, gerçek olayı görmedim.",
            };
            npc.mandatoryFinalFact = "Tüccarın oğlunu o gece içeri ben aldım.";
            npc.patienceCount = 5;
            EditorUtility.SetDirty(npc);
            return npc;
        }

        static ScenarioData UpsertScenario(NPCData culprit)
        {
            string path = ScenarioFolder + "/Senaryo_FrigyaCinayeti.asset";
            var s = AssetDatabase.LoadAssetAtPath<ScenarioData>(path);
            if (s == null)
            {
                s = ScriptableObject.CreateInstance<ScenarioData>();
                AssetDatabase.CreateAsset(s, path);
            }
            s.scenarioTitle = "Frigya Tüccarının Şüpheli Ölümü";
            s.newspaperHeadline = "FRİGYA'DA SARAY TÜCCARI ESRARENGİZ ŞEKİLDE ÖLDÜ — TAPINAKTA BULUNDU";
            s.scenarioDate = "MÖ 800";
            s.culprit = culprit;
            EditorUtility.SetDirty(s);
            return s;
        }

        static NPCData LoadOrCreate(string path)
        {
            var npc = AssetDatabase.LoadAssetAtPath<NPCData>(path);
            if (npc == null)
            {
                npc = ScriptableObject.CreateInstance<NPCData>();
                AssetDatabase.CreateAsset(npc, path);
            }
            return npc;
        }
    }
}
