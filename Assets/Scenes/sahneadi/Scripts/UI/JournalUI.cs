using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Argos.Evidence;
using Argos.Player;

namespace Argos.UI
{
    public class JournalUI : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private RectTransform contentParent;

        public bool IsOpen => root != null && root.activeSelf;

        public void Setup(GameObject rootObj, RectTransform content)
        {
            root = rootObj;
            contentParent = content;
        }

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
            if (contentParent == null) return;

            for (int i = contentParent.childCount - 1; i >= 0; i--)
                Destroy(contentParent.GetChild(i).gameObject);

            var inv = PlayerInventory.Instance;
            if (inv == null) return;

            var items = inv.GetAll();
            if (items.Count == 0)
            {
                CreateEmptyMessage();
                return;
            }

            foreach (var item in items) CreateItemRow(item);
        }

        void CreateEmptyMessage()
        {
            var go = new GameObject("Empty");
            go.transform.SetParent(contentParent, false);
            var le = go.AddComponent<LayoutElement>();
            le.minHeight = 60f;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = "Defter boş. Müzeyi keşfet ve kanıt topla.";
            tmp.fontSize = 16;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(0.3f, 0.3f, 0.3f);
        }

        void CreateItemRow(EvidenceItem item)
        {
            var go = new GameObject("Item");
            go.transform.SetParent(contentParent, false);
            var le = go.AddComponent<LayoutElement>();
            le.minHeight = 70f;

            var bg = go.AddComponent<Image>();
            bg.color = new Color(0.95f, 0.92f, 0.85f, 0.5f);

            var txt = new GameObject("Text");
            txt.transform.SetParent(go.transform, false);
            var rt = txt.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(12f, 8f);
            rt.offsetMax = new Vector2(-12f, -8f);

            var tmp = txt.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 15;
            tmp.color = new Color(0.12f, 0.12f, 0.12f);

            string prefix = item is EvidencePhoto ? "[Foto]" : "[Not]";
            string desc = string.IsNullOrEmpty(item.description)
                ? ""
                : $"\n<size=13>{item.description}</size>";
            tmp.text = $"<b>{prefix} {item.title}</b>{desc}";
        }
    }
}
