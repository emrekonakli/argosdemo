using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Argos.UI
{
    /// <summary>
    /// MuseumSceneBootstrap ve PortalSceneBootstrap'ın paylaştığı UI panel
    /// kurucu yardımcıları. UI prefab kullanmadan procedural kurulum yapar.
    /// </summary>
    public static class UIBuilders
    {
        // ============================================================
        // InterrogationPanel — büyük overlay; sabır + chat + kanıt picker.
        // ============================================================
        public static InterrogationUI BuildInterrogationPanel(Transform parent)
        {
            var root = MakePanel(parent, "InterrogationPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(900f, 600f), new Color(0f, 0f, 0f, 0.92f));

            // Header: NPC adı + sabır barı
            var npcName = MakeText(root.transform, "NpcName", new Vector2(0, 250), new Vector2(840, 50), 28, TextAlignmentOptions.MidlineLeft);
            npcName.color = new Color(0.95f, 0.92f, 0.84f);
            var patience = MakeText(root.transform, "Patience", new Vector2(280, 250), new Vector2(280, 50), 20, TextAlignmentOptions.MidlineRight);
            patience.color = new Color(0.9f, 0.8f, 0.4f);

            // Sol: portre (200x200 placeholder)
            var portraitGo = new GameObject("Portrait");
            portraitGo.transform.SetParent(root.transform, false);
            var pRt = portraitGo.AddComponent<RectTransform>();
            pRt.anchorMin = new Vector2(0.5f, 0.5f);
            pRt.anchorMax = new Vector2(0.5f, 0.5f);
            pRt.pivot = new Vector2(0.5f, 0.5f);
            pRt.anchoredPosition = new Vector2(-330, 60);
            pRt.sizeDelta = new Vector2(200, 200);
            var portrait = portraitGo.AddComponent<Image>();
            portrait.color = new Color(0.45f, 0.35f, 0.32f);

            // Sağ: chat alanı arka plan + scrollable text
            var chatBg = MakePanel(root.transform, "ChatBg", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(120, 60), new Vector2(610, 340), new Color(0.1f, 0.1f, 0.12f, 0.85f));
            var chatText = MakeText(chatBg.transform, "ChatText", Vector2.zero, new Vector2(580, 320), 15, TextAlignmentOptions.TopLeft);

            // Input field (alt)
            var inputGo = new GameObject("Input");
            inputGo.transform.SetParent(root.transform, false);
            var iRt = inputGo.AddComponent<RectTransform>();
            iRt.anchorMin = new Vector2(0.5f, 0.5f);
            iRt.anchorMax = new Vector2(0.5f, 0.5f);
            iRt.pivot = new Vector2(0.5f, 0.5f);
            iRt.anchoredPosition = new Vector2(50, -180);
            iRt.sizeDelta = new Vector2(530, 50);
            var iImg = inputGo.AddComponent<Image>();
            iImg.color = new Color(0.95f, 0.92f, 0.85f, 0.95f);
            var input = inputGo.AddComponent<TMP_InputField>();

            var inputTextArea = new GameObject("TextArea");
            inputTextArea.transform.SetParent(inputGo.transform, false);
            var taRt = inputTextArea.AddComponent<RectTransform>();
            taRt.anchorMin = Vector2.zero;
            taRt.anchorMax = Vector2.one;
            taRt.offsetMin = new Vector2(10, 4);
            taRt.offsetMax = new Vector2(-10, -4);
            inputTextArea.AddComponent<RectMask2D>();

            var placeholder = MakeText(inputTextArea.transform, "Placeholder", Vector2.zero, Vector2.zero, 16, TextAlignmentOptions.MidlineLeft);
            placeholder.color = new Color(0.4f, 0.35f, 0.3f);
            placeholder.text = "Mesajını yaz...";
            var phRt = placeholder.rectTransform;
            phRt.anchorMin = Vector2.zero; phRt.anchorMax = Vector2.one; phRt.offsetMin = Vector2.zero; phRt.offsetMax = Vector2.zero;

            var inputText = MakeText(inputTextArea.transform, "Text", Vector2.zero, Vector2.zero, 16, TextAlignmentOptions.MidlineLeft);
            inputText.color = new Color(0.12f, 0.1f, 0.08f);
            var itRt = inputText.rectTransform;
            itRt.anchorMin = Vector2.zero; itRt.anchorMax = Vector2.one; itRt.offsetMin = Vector2.zero; itRt.offsetMax = Vector2.zero;

            input.textViewport = taRt;
            input.textComponent = inputText;
            input.placeholder = placeholder;

            var sendBtn = MakeButton(root.transform, "SendBtn", "Gönder", new Vector2(360, -180), new Vector2(110, 50));

            // Üst butonlar: Evidence + Summary + Exit
            var evidenceBtn = MakeButton(root.transform, "EvidenceBtn", "Kanıt Göster", new Vector2(120, -255), new Vector2(180, 45));
            var summaryBtn = MakeButton(root.transform, "SummaryBtn", "Özet", new Vector2(305, -255), new Vector2(100, 45));
            var exitBtn = MakeButton(root.transform, "ExitBtn", "Çık", new Vector2(390, -255), new Vector2(80, 45));
            exitBtn.image.color = new Color(0.6f, 0.2f, 0.2f, 0.9f);

            // Pre-interrogation alt panel (özet)
            var preRoot = MakePanel(root.transform, "PreInterrogation", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(600f, 450f), new Color(0.95f, 0.92f, 0.85f, 0.98f));
            var preLabel = MakeText(preRoot.transform, "Summary", new Vector2(0, 30), new Vector2(560, 340), 16, TextAlignmentOptions.TopLeft);
            preLabel.color = new Color(0.12f, 0.1f, 0.08f);
            var preContinue = MakeButton(preRoot.transform, "PreContinue", "Sorguya Başla", new Vector2(0, -180), new Vector2(220, 55));
            preRoot.SetActive(false);

            // Evidence picker alt panel
            var pickerRoot = MakePanel(root.transform, "EvidencePicker", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(500f, 400f), new Color(0.1f, 0.1f, 0.12f, 0.95f));
            var pickerHeader = MakeText(pickerRoot.transform, "Header", new Vector2(0, 165), new Vector2(460, 40), 22, TextAlignmentOptions.Center);
            pickerHeader.text = "Fotoğraf Seç";
            pickerHeader.color = new Color(0.95f, 0.92f, 0.84f);
            var pickerContentGo = new GameObject("Content");
            pickerContentGo.transform.SetParent(pickerRoot.transform, false);
            var pcRt = pickerContentGo.AddComponent<RectTransform>();
            pcRt.anchorMin = new Vector2(0.5f, 0.5f);
            pcRt.anchorMax = new Vector2(0.5f, 0.5f);
            pcRt.pivot = new Vector2(0.5f, 1f);
            pcRt.anchoredPosition = new Vector2(0, 130);
            pcRt.sizeDelta = new Vector2(460, 260);
            var vlg = pickerContentGo.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 6f;
            vlg.padding = new RectOffset(6, 6, 6, 6);
            vlg.childControlHeight = true; vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false; vlg.childForceExpandWidth = true;
            var pickerCloseBtn = MakeButton(pickerRoot.transform, "PickerClose", "Kapat", new Vector2(0, -170), new Vector2(180, 50));
            pickerRoot.SetActive(false);

            // Component + wire-up
            var interrogation = root.AddComponent<InterrogationUI>();
            interrogation.Setup(root, portrait, npcName, patience, chatText, input, sendBtn, evidenceBtn, summaryBtn, exitBtn, preRoot, preLabel, preContinue, pickerRoot, pcRt, pickerCloseBtn);
            root.SetActive(false);
            return interrogation;
        }

        // ============================================================
        // Helpers — sade panel/text/button
        // ============================================================
        static GameObject MakePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            go.AddComponent<Image>().color = color;
            return go;
        }

        static TMP_Text MakeText(Transform parent, string name, Vector2 anchoredPos, Vector2 size, float fontSize, TextAlignmentOptions align)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = fontSize;
            tmp.alignment = align;
            tmp.color = Color.white;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            return tmp;
        }

        static Button MakeButton(Transform parent, string name, string label, Vector2 anchoredPos, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            var img = go.AddComponent<Image>();
            img.color = new Color(0.3f, 0.4f, 0.7f, 0.9f);
            var btn = go.AddComponent<Button>();

            var lbl = MakeText(go.transform, "Label", Vector2.zero, Vector2.zero, 18, TextAlignmentOptions.Center);
            var lblRt = lbl.rectTransform;
            lblRt.anchorMin = Vector2.zero; lblRt.anchorMax = Vector2.one; lblRt.offsetMin = Vector2.zero; lblRt.offsetMax = Vector2.zero;
            lbl.text = label;
            return btn;
        }
    }
}
