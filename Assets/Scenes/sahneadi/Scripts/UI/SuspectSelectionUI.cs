using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Argos.Game;
using Argos.NPC;

namespace Argos.UI
{
    public class SuspectSelectionUI : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private RectTransform listParent;
        [SerializeField] private Button closeButton;
        [SerializeField] private List<NPCData> suspects = new List<NPCData>();

        public void Setup(GameObject rootObj, RectTransform list, Button close)
        {
            root = rootObj;
            listParent = list;
            closeButton = close;
            if (closeButton != null) closeButton.onClick.AddListener(Hide);
        }

        public void SetSuspects(List<NPCData> list) => suspects = list;

        public void Show()
        {
            if (root != null) root.SetActive(true);
            Populate();
        }

        public void Hide()
        {
            if (root != null) root.SetActive(false);
        }

        void Populate()
        {
            if (listParent == null) return;

            for (int i = listParent.childCount - 1; i >= 0; i--)
                Destroy(listParent.GetChild(i).gameObject);

            // Senaryo'dan culprit dahil tüm şüphelileri otomatik topla; manuel liste boşsa.
            var working = new List<NPCData>(suspects);
            if (working.Count == 0 && GameManager.Instance != null && GameManager.Instance.CurrentScenario != null)
            {
                if (GameManager.Instance.CurrentScenario.culprit != null)
                    working.Add(GameManager.Instance.CurrentScenario.culprit);
            }

            if (working.Count == 0)
            {
                CreateLabel("Şu an dosyada şüpheli yok.");
                return;
            }

            foreach (var npc in working)
            {
                if (npc == null) continue;
                CreateSuspectButton(npc);
            }
        }

        void CreateLabel(string text)
        {
            var go = new GameObject("EmptyLabel");
            go.transform.SetParent(listParent, false);
            var le = go.AddComponent<LayoutElement>();
            le.minHeight = 50f;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 16;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(0.5f, 0.5f, 0.5f);
        }

        void CreateSuspectButton(NPCData npc)
        {
            var go = new GameObject("Suspect_" + npc.npcName);
            go.transform.SetParent(listParent, false);
            var le = go.AddComponent<LayoutElement>();
            le.minHeight = 70f;
            var img = go.AddComponent<Image>();
            img.color = new Color(0.45f, 0.35f, 0.32f, 0.92f);
            var btn = go.AddComponent<Button>();

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(go.transform, false);
            var rt = labelGo.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(12f, 8f);
            rt.offsetMax = new Vector2(-12f, -8f);
            var tmp = labelGo.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 17;
            tmp.color = new Color(0.95f, 0.92f, 0.85f);
            tmp.text = $"<b>{npc.npcName}</b>\n<size=13>{npc.period}</size>";

            btn.onClick.AddListener(() => OnSuspectChosen(npc));
        }

        public void OnSuspectChosen(NPCData chosen)
        {
            if (chosen == null) return;
            bool isCulprit = GameManager.Instance != null
                && GameManager.Instance.CurrentScenario != null
                && GameManager.Instance.CurrentScenario.culprit == chosen;
            if (!isCulprit) GameManager.Instance?.RegisterWrongSuspect();
            QuestManager.Instance?.TryAdvance(3);
            Hide();
            UIManager.Instance?.OpenInterrogation(chosen, true);
        }
    }
}
