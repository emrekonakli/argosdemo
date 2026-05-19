using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Argos.Artifacts;
using Argos.Game;

namespace Argos.EditorTools
{
    public static class SampleArtifactBuilder
    {
        const string ScenePath = "Assets/Scenes/sahneadi/Scenes/MuseumScene.unity";
        const string ArtifactFolder = "Assets/Scenes/sahneadi/ScriptableObjects/Artifacts";

        [MenuItem("ARGOS/Build Sample Artifacts")]
        public static void Build()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.path != ScenePath)
            {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                else return;
            }

            AssetFolders.Ensure(ArtifactFolder);

            var a = UpsertArtifact("Eser_A_HititAslan", a =>
            {
                a.artifactName = "Hitit Aslan Heykeli";
                a.artifactDescription = "Bronz alaşımdan dökülmüş, tabanında küçük bir kazıma izi bulunan aslan figürü. Hitit dönemi koruyucu sembollerinden. Müzenin envanterinde 1958'den beri kayıtlı.";
                a.artifactPeriod = "MÖ 1400";
                a.artifactOrigin = "Alacahöyük";
                a.isEvidenceTarget = false;
                a.isPortalTrigger = false;
                a.internalVoiceLine = "";
            });

            var b = UpsertArtifact("Eser_B_MektupTableti", a =>
            {
                a.artifactName = "Antik Mektup Tableti";
                a.artifactDescription = "Çivi yazısıyla yazılmış bir mektup parçası. Kenarı kırık, ama mühür izi hâlâ okunabiliyor. Üzerinde belirgin bir kan lekesi var — son sahibinden kalmış olabilir.";
                a.artifactPeriod = "MÖ 1200";
                a.artifactOrigin = "Boğazköy";
                a.isEvidenceTarget = true;
                a.isPortalTrigger = false;
                a.internalVoiceLine = "";
            });

            var c = UpsertArtifact("Eser_C_TuncKupa", a =>
            {
                a.artifactName = "Tunç Kupa";
                a.artifactDescription = "Ağız kenarında belirgin bir aşınma var. Kupanın yüzeyinde dinmeyen, hafif bir titreme hissediliyor — bilim adamlarının açıklayamadığı bir anomali.";
                a.artifactPeriod = "MÖ 800";
                a.artifactOrigin = "Frigya";
                a.isEvidenceTarget = false;
                a.isPortalTrigger = true;
                a.internalVoiceLine = "Bu kupa... sanki bana bakıyor. Tanıdık bir koku var, ama hatırlamıyorum. Burada bir şey olmuş.";
            });

            var bootstrap = Object.FindFirstObjectByType<MuseumSceneBootstrap>();
            if (bootstrap != null)
            {
                bootstrap.sampleArtifacts = new[] { a, b, c };
                EditorUtility.SetDirty(bootstrap);
            }
            else
            {
                Debug.LogWarning("[ARGOS] MuseumSceneBootstrap bulunamadı — sahnede Bootstrap GameObject var mı kontrol et.");
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[ARGOS] Sample artifacts built; scene updated.");
        }

        static ArtifactData UpsertArtifact(string fileName, System.Action<ArtifactData> setup)
        {
            string path = $"{ArtifactFolder}/{fileName}.asset";
            var data = AssetDatabase.LoadAssetAtPath<ArtifactData>(path);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<ArtifactData>();
                AssetDatabase.CreateAsset(data, path);
            }
            setup(data);
            EditorUtility.SetDirty(data);
            return data;
        }
    }
}
