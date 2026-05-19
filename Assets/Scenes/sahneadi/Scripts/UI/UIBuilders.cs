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
        // SuspectSelectionPanel — Dava Panosu'na tıklayınca açılan şüpheli listesi.
        // ============================================================
        public static SuspectSelectionUI BuildSuspectSelectionPanel(Transform parent)
        {
            var root = MakePanel(parent, "SuspectSelectionPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(620f, 540f), new Color(0.1f, 0.08f, 0.08f, 0.96f));

            var header = MakeText(root.transform, "Header", new Vector2(0, 230), new Vector2(580, 50), 24, TextAlignmentOptions.Center);
            header.text = "Dava Panosu — Şüpheliler";
            header.color = new Color(0.95f, 0.92f, 0.85f);

            var listGo = new GameObject("List");
            listGo.transform.SetParent(root.transform, false);
            var listRt = listGo.AddComponent<RectTransform>();
            listRt.anchorMin = new Vector2(0.5f, 0.5f);
            listRt.anchorMax = new Vector2(0.5f, 0.5f);
            listRt.pivot = new Vector2(0.5f, 1f);
            listRt.anchoredPosition = new Vector2(0, 195);
            listRt.sizeDelta = new Vector2(560, 360);
            var vlg = listGo.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 8f;
            vlg.padding = new RectOffset(8, 8, 8, 8);
            vlg.childControlHeight = true; vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false; vlg.childForceExpandWidth = true;

            var closeBtn = MakeButton(root.transform, "CloseBtn", "Kapat", new Vector2(0, -230), new Vector2(200, 50));

            var sus = root.AddComponent<SuspectSelectionUI>();
            sus.Setup(root, listRt, closeBtn);
            root.SetActive(false);
            return sus;
        }

        // ============================================================
        // NewspaperPanel — tam ekran gazete giriş/çıkış görseli.
        // ============================================================
        public static NewspaperUI BuildNewspaperPanel(Transform parent)
        {
            var root = new GameObject("NewspaperPanel");
            root.transform.SetParent(parent, false);
            var rt = root.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var bg = root.AddComponent<Image>();
            bg.color = new Color(0.94f, 0.9f, 0.78f, 1f); // bej eski kağıt rengi

            // İçerik konteyneri — orta 80% alan.
            var content = new GameObject("Content");
            content.transform.SetParent(root.transform, false);
            var cRt = content.AddComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0.1f, 0.05f);
            cRt.anchorMax = new Vector2(0.9f, 0.95f);
            cRt.offsetMin = Vector2.zero;
            cRt.offsetMax = Vector2.zero;

            // Tarih (üst sağ küçük)
            var dateLbl = MakeAnchoredText(content.transform, "Date", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-10f, -10f), new Vector2(220f, 28f), 14, TextAlignmentOptions.TopRight);
            dateLbl.color = new Color(0.3f, 0.2f, 0.1f);

            // Başlık (üstte büyük)
            var headlineLbl = MakeAnchoredText(content.transform, "Headline", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -60f), new Vector2(900f, 90f), 36, TextAlignmentOptions.Center);
            headlineLbl.color = new Color(0.12f, 0.08f, 0.05f);
            headlineLbl.fontStyle = FontStyles.Bold;

            // Gövde
            var bodyLbl = MakeAnchoredText(content.transform, "Body", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 30f), new Vector2(900f, 250f), 18, TextAlignmentOptions.TopLeft);
            bodyLbl.color = new Color(0.2f, 0.15f, 0.1f);

            // Skor (alt orta)
            var scoreLbl = MakeAnchoredText(content.transform, "Score", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -160f), new Vector2(400f, 40f), 22, TextAlignmentOptions.Center);
            scoreLbl.color = new Color(0.5f, 0.1f, 0.1f);
            scoreLbl.fontStyle = FontStyles.Bold;

            // Primary button (alt orta)
            var priBtn = MakeButton(content.transform, "Primary", "", new Vector2(-130, -240), new Vector2(240, 60));
            var priLbl = priBtn.GetComponentInChildren<TMP_Text>(true);

            // Secondary button
            var secBtn = MakeButton(content.transform, "Secondary", "", new Vector2(130, -240), new Vector2(240, 60));
            secBtn.image.color = new Color(0.4f, 0.4f, 0.4f, 0.9f);
            var secLbl = secBtn.GetComponentInChildren<TMP_Text>(true);
            secBtn.gameObject.SetActive(false);

            var paper = root.AddComponent<NewspaperUI>();
            paper.Setup(root, headlineLbl, dateLbl, bodyLbl, scoreLbl, priBtn, priLbl, secBtn, secLbl);
            // Sahnede yaratıldığında pasif başlat — Show çağrılınca aktif olur.
            // NewspaperScene'de Bootstrap aktif edecek; diğer sahnelerde overlay olarak gizli kalır.
            root.SetActive(false);
            return paper;
        }

        // Yardımcı: custom anchor'lı text — köşeye konumlanma için.
        static TMP_Text MakeAnchoredText(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size, float fontSize, TextAlignmentOptions align)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
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
