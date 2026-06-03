using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Argos.Game;
using Argos.NPC;
using Argos.Player;

namespace Argos.UI
{
    public class CaseBoardUI : MonoBehaviour
    {
        private GameObject root;
        private RectTransform suspectsContent;
        private RectTransform cluesContent;
        private Button mapButton;
        private GameObject mapOverlay;
        private Button mapCloseButton;
        private Button closeButton;
        private GameObject suspectsQuadrant;
        private GameObject cluesQuadrant;
        private GameObject mapQuadrant;
        private GameObject newspaperQuadrant;
        private GameObject noCaseLabel;
        private Button newspaperButton;
        private RectTransform shelvedArea;

        public bool IsOpen => root != null && root.activeSelf;

        public void Setup(GameObject rootObj, RectTransform suspects, RectTransform clues,
            Button map, GameObject overlay, Image fullImg, Button mapClose, Button close,
            GameObject suspQ, GameObject clueQ, GameObject mapQ, GameObject newsQ, GameObject noCaseLbl,
            Button newsBtn, RectTransform shelved)
        {
            root = rootObj;
            suspectsContent = suspects;
            cluesContent = clues;
            mapButton = map;
            mapOverlay = overlay;
            mapCloseButton = mapClose;
            closeButton = close;
            suspectsQuadrant = suspQ;
            cluesQuadrant = clueQ;
            mapQuadrant = mapQ;
            newspaperQuadrant = newsQ;
            noCaseLabel = noCaseLbl;
            newspaperButton = newsBtn;
            shelvedArea = shelved;

            if (closeButton != null) closeButton.onClick.AddListener(Hide);
            if (mapButton != null) mapButton.onClick.AddListener(() => { if (mapOverlay != null) mapOverlay.SetActive(true); });
            if (mapCloseButton != null) mapCloseButton.onClick.AddListener(() => { if (mapOverlay != null) mapOverlay.SetActive(false); });
            if (newspaperButton != null) newspaperButton.onClick.AddListener(() => { Hide(); UIManager.Instance?.OpenNewspaper(false, false); });
        }

        public void Show()
        {
            if (root == null) return;
            root.SetActive(true);
            if (mapOverlay != null) mapOverlay.SetActive(false);

            bool caseActive = GameManager.Instance != null && GameManager.Instance.CaseAccepted;

            if (suspectsQuadrant != null) suspectsQuadrant.SetActive(caseActive);
            if (cluesQuadrant != null) cluesQuadrant.SetActive(caseActive);
            if (mapQuadrant != null) mapQuadrant.SetActive(caseActive);
            if (newspaperQuadrant != null) newspaperQuadrant.SetActive(caseActive);
            if (noCaseLabel != null) noCaseLabel.SetActive(!caseActive);
            if (shelvedArea != null)
            {
                shelvedArea.gameObject.SetActive(!caseActive);
                if (!caseActive) PopulateShelved();
            }
            if (closeButton != null) closeButton.transform.SetAsLastSibling();

            if (caseActive)
            {
                PopulateSuspects();
                PopulateClues();
            }
        }

        public void Hide()
        {
            if (root != null) root.SetActive(false);
        }

        void PopulateSuspects()
        {
            if (suspectsContent == null) return;
            ClearChildren(suspectsContent);

            var suspects = new List<NPCData>();
            if (GameManager.Instance != null && GameManager.Instance.CurrentScenario != null)
            {
                var sc = GameManager.Instance.CurrentScenario;
                if (sc.suspects != null && sc.suspects.Count > 0)
                    suspects.AddRange(sc.suspects);
                if (sc.culprit != null && !suspects.Contains(sc.culprit))
                    suspects.Add(sc.culprit);
            }

            if (suspects.Count == 0)
            {
                MakeLabel(suspectsContent, "Şüpheli bulunamadı.");
                return;
            }

            foreach (var npc in suspects)
            {
                if (npc == null) continue;
                MakeSuspectCard(npc);
            }
        }

        void MakeSuspectCard(NPCData npc)
        {
            var card = new GameObject("Suspect_" + npc.npcName);
            card.transform.SetParent(suspectsContent, false);
            var le = card.AddComponent<LayoutElement>();
            le.minHeight = 90f;
            le.flexibleWidth = 1f;

            var hlg = card.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 8f;
            hlg.padding = new RectOffset(6, 6, 6, 6);
            hlg.childControlHeight = true;
            hlg.childControlWidth = false;
            hlg.childForceExpandHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childAlignment = TextAnchor.MiddleLeft;

            var portraitGo = new GameObject("Portrait");
            portraitGo.transform.SetParent(card.transform, false);
            var pLe = portraitGo.AddComponent<LayoutElement>();
            pLe.minWidth = 70f; pLe.minHeight = 70f; pLe.preferredWidth = 70f; pLe.preferredHeight = 70f;
            var pImg = portraitGo.AddComponent<Image>();
            if (npc.portrait != null) { pImg.sprite = npc.portrait; pImg.color = Color.white; }
            else { pImg.color = new Color(0.5f, 0.4f, 0.35f); }
            pImg.preserveAspect = true;

            var infoGo = new GameObject("Info");
            infoGo.transform.SetParent(card.transform, false);
            var iLe = infoGo.AddComponent<LayoutElement>();
            iLe.flexibleWidth = 1f; iLe.minHeight = 70f;
            var tmp = infoGo.AddComponent<TextMeshProUGUI>();
            tmp.text = $"<b>{npc.npcName}</b>\n<size=13>{npc.period}</size>";
            tmp.fontSize = 16;
            tmp.color = new Color(0.92f, 0.88f, 0.8f);
            tmp.alignment = TextAlignmentOptions.MidlineLeft;

            var bg = card.AddComponent<Image>();
            bg.color = new Color(0.22f, 0.18f, 0.16f, 0.9f);

            var btn = card.AddComponent<Button>();
            btn.targetGraphic = bg;
            var captured = npc;
            btn.onClick.AddListener(() =>
            {
                Hide();
                bool isCulprit = GameManager.Instance != null
                    && GameManager.Instance.CurrentScenario != null
                    && GameManager.Instance.CurrentScenario.culprit == captured;
                if (!isCulprit) GameManager.Instance?.RegisterWrongSuspect();
                QuestManager.Instance?.TryAdvance(3);
                UIManager.Instance?.OpenInterrogation(captured, true);
            });
        }

        void PopulateClues()
        {
            if (cluesContent == null) return;
            ClearChildren(cluesContent);

            if (PlayerInventory.Instance == null || PlayerInventory.Instance.Photos.Count == 0)
            {
                MakeLabel(cluesContent, "Henüz ipucu fotoğrafı yok.");
                return;
            }

            foreach (var photo in PlayerInventory.Instance.Photos)
            {
                if (photo == null) continue;
                MakeClueCard(photo);
            }
        }

        void MakeClueCard(Argos.Evidence.EvidencePhoto photo)
        {
            var card = new GameObject("Clue_" + photo.title);
            card.transform.SetParent(cluesContent, false);
            var le = card.AddComponent<LayoutElement>();
            le.minHeight = 80f; le.flexibleWidth = 1f;

            var hlg = card.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 8f;
            hlg.padding = new RectOffset(6, 6, 6, 6);
            hlg.childControlHeight = true; hlg.childControlWidth = false;
            hlg.childForceExpandHeight = true; hlg.childForceExpandWidth = false;
            hlg.childAlignment = TextAnchor.MiddleLeft;

            var thumbGo = new GameObject("Thumb");
            thumbGo.transform.SetParent(card.transform, false);
            var tLe = thumbGo.AddComponent<LayoutElement>();
            tLe.minWidth = 64f; tLe.minHeight = 64f; tLe.preferredWidth = 64f; tLe.preferredHeight = 64f;
            var tImg = thumbGo.AddComponent<Image>();
            Sprite sprite = photo.thumbnail;
            if (sprite == null && photo.sourceArtifact != null) sprite = photo.sourceArtifact.artifactSprite;
            if (sprite != null) { tImg.sprite = sprite; tImg.color = Color.white; }
            else { tImg.color = new Color(0.6f, 0.55f, 0.3f); }
            tImg.preserveAspect = true;

            var infoGo = new GameObject("Info");
            infoGo.transform.SetParent(card.transform, false);
            var iLe = infoGo.AddComponent<LayoutElement>();
            iLe.flexibleWidth = 1f; iLe.minHeight = 64f;
            var tmp = infoGo.AddComponent<TextMeshProUGUI>();
            tmp.text = $"<b>{photo.title}</b>\n<size=12>{photo.description}</size>";
            tmp.fontSize = 14;
            tmp.color = new Color(0.92f, 0.88f, 0.8f);
            tmp.alignment = TextAlignmentOptions.MidlineLeft;

            var bg = card.AddComponent<Image>();
            bg.color = new Color(0.18f, 0.2f, 0.22f, 0.9f);
        }

        void PopulateShelved()
        {
            if (shelvedArea == null) return;
            ClearChildren(shelvedArea);

            if (GameManager.Instance == null || GameManager.Instance.ShelvedCases.Count == 0)
                return;

            foreach (var title in GameManager.Instance.ShelvedCases)
            {
                var card = new GameObject("Shelved_" + title);
                card.transform.SetParent(shelvedArea, false);
                var le = card.AddComponent<LayoutElement>();
                le.minHeight = 70f;
                le.flexibleWidth = 1f;

                var hlg = card.AddComponent<HorizontalLayoutGroup>();
                hlg.spacing = 10f;
                hlg.padding = new RectOffset(8, 8, 6, 6);
                hlg.childControlHeight = true;
                hlg.childControlWidth = false;
                hlg.childForceExpandHeight = true;
                hlg.childForceExpandWidth = false;
                hlg.childAlignment = TextAnchor.MiddleLeft;

                var iconGo = new GameObject("Icon");
                iconGo.transform.SetParent(card.transform, false);
                var iconLe = iconGo.AddComponent<LayoutElement>();
                iconLe.minWidth = 50f;
                iconLe.minHeight = 50f;
                iconLe.preferredWidth = 50f;
                iconLe.preferredHeight = 50f;
                var iconImg = iconGo.AddComponent<Image>();
                iconImg.color = new Color(0.92f, 0.88f, 0.75f);
                iconImg.preserveAspect = true;

                var iconLabel = new GameObject("G");
                iconLabel.transform.SetParent(iconGo.transform, false);
                var ilRt = iconLabel.AddComponent<RectTransform>();
                ilRt.anchorMin = Vector2.zero;
                ilRt.anchorMax = Vector2.one;
                ilRt.offsetMin = Vector2.zero;
                ilRt.offsetMax = Vector2.zero;
                var ilTmp = iconLabel.AddComponent<TextMeshProUGUI>();
                ilTmp.text = "G";
                ilTmp.fontSize = 22;
                ilTmp.alignment = TextAlignmentOptions.Center;
                ilTmp.color = new Color(0.3f, 0.2f, 0.1f);
                ilTmp.fontStyle = FontStyles.Bold;
                ilTmp.raycastTarget = false;

                var infoGo = new GameObject("Info");
                infoGo.transform.SetParent(card.transform, false);
                var iLe = infoGo.AddComponent<LayoutElement>();
                iLe.flexibleWidth = 1f;
                iLe.minHeight = 50f;
                var tmp = infoGo.AddComponent<TextMeshProUGUI>();
                tmp.text = title;
                tmp.fontSize = 15;
                tmp.color = new Color(0.7f, 0.6f, 0.5f);
                tmp.alignment = TextAlignmentOptions.MidlineLeft;
                tmp.fontStyle = FontStyles.Italic;

                var bg = card.AddComponent<Image>();
                bg.color = new Color(0.2f, 0.18f, 0.15f, 0.85f);
            }
        }

        void MakeLabel(RectTransform parent, string text)
        {
            var go = new GameObject("Label");
            go.transform.SetParent(parent, false);
            var le = go.AddComponent<LayoutElement>();
            le.minHeight = 40f;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 15;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(0.6f, 0.6f, 0.6f);
        }

        void ClearChildren(RectTransform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
                Destroy(parent.GetChild(i).gameObject);
        }
    }
}
