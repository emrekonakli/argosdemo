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
        // CaseBoardPanel — 4 bölmeli dava panosu ekranı.
        // ============================================================
        public static CaseBoardUI BuildCaseBoardPanel(Transform parent)
        {
            // Ana panel — tam ekrana yakın
            var root = MakePanel(parent, "CaseBoardPanel",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(960f, 620f),
                new Color(0.08f, 0.07f, 0.06f, 0.96f));

            // Başlık
            var header = MakeText(root.transform, "Header",
                new Vector2(0, 280), new Vector2(900, 40), 26, TextAlignmentOptions.Center);
            header.text = "DAVA PANOSU";
            header.color = new Color(0.95f, 0.85f, 0.6f);
            header.fontStyle = FontStyles.Bold;

            float qW = 440f, qH = 240f;
            float gap = 10f;

            // ── SOL ÜST: Şüpheliler ──
            var suspectsBg = MakePanel(root.transform, "SuspectsQuadrant",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-(qW + gap) / 2f, (qH + gap) / 2f - 20f), new Vector2(qW, qH),
                new Color(0.14f, 0.12f, 0.1f, 0.95f));
            var suspLabel = MakeText(suspectsBg.transform, "Label",
                new Vector2(0, 105), new Vector2(qW - 20, 30), 18, TextAlignmentOptions.Center);
            suspLabel.text = "ŞÜPHELİLER";
            suspLabel.color = new Color(0.9f, 0.75f, 0.4f);
            suspLabel.fontStyle = FontStyles.Bold;

            var suspectsScroll = MakeScrollRect(suspectsBg.transform, "SuspectsScroll",
                new Vector2(0, -10), new Vector2(qW - 16, qH - 45));
            var suspectsContent = suspectsScroll.GetComponent<ScrollRect>().content;

            // ── SAĞ ÜST: İpuçları ──
            var cluesBg = MakePanel(root.transform, "CluesQuadrant",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2((qW + gap) / 2f, (qH + gap) / 2f - 20f), new Vector2(qW, qH),
                new Color(0.12f, 0.14f, 0.12f, 0.95f));
            var clueLabel = MakeText(cluesBg.transform, "Label",
                new Vector2(0, 105), new Vector2(qW - 20, 30), 18, TextAlignmentOptions.Center);
            clueLabel.text = "İPUÇLARI";
            clueLabel.color = new Color(0.5f, 0.85f, 0.5f);
            clueLabel.fontStyle = FontStyles.Bold;

            var cluesScroll = MakeScrollRect(cluesBg.transform, "CluesScroll",
                new Vector2(0, -10), new Vector2(qW - 16, qH - 45));
            var cluesContent = cluesScroll.GetComponent<ScrollRect>().content;

            // ── SOL ALT: Müze Krokisi ──
            var mapBg = MakePanel(root.transform, "MapQuadrant",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-(qW + gap) / 2f, -(qH + gap) / 2f - 20f), new Vector2(qW, qH),
                new Color(0.12f, 0.12f, 0.14f, 0.95f));
            var mapLabel = MakeText(mapBg.transform, "Label",
                new Vector2(0, 105), new Vector2(qW - 20, 30), 18, TextAlignmentOptions.Center);
            mapLabel.text = "MÜZE KROKİSİ";
            mapLabel.color = new Color(0.6f, 0.7f, 0.95f);
            mapLabel.fontStyle = FontStyles.Bold;

            var mapBtnGo = new GameObject("MapButton");
            mapBtnGo.transform.SetParent(mapBg.transform, false);
            var mbRt = mapBtnGo.AddComponent<RectTransform>();
            mbRt.anchorMin = new Vector2(0.5f, 0.5f);
            mbRt.anchorMax = new Vector2(0.5f, 0.5f);
            mbRt.pivot = new Vector2(0.5f, 0.5f);
            mbRt.anchoredPosition = new Vector2(0, -10);
            mbRt.sizeDelta = new Vector2(qW - 40, qH - 60);
            var mbImg = mapBtnGo.AddComponent<Image>();
            var museumSprite = Resources.Load<Sprite>("MuseumBackground");
            if (museumSprite != null)
            {
                mbImg.sprite = museumSprite;
                mbImg.color = Color.white;
            }
            else
            {
                mbImg.color = new Color(0.3f, 0.35f, 0.45f);
            }
            mbImg.preserveAspect = true;
            var mapBtn = mapBtnGo.AddComponent<Button>();

            // Harita overlay (tıklayınca büyük fotoğraf)
            var mapOverlay = MakePanel(root.transform, "MapOverlay",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(900f, 560f),
                new Color(0f, 0f, 0f, 0.95f));
            var fullImgGo = new GameObject("FullImage");
            fullImgGo.transform.SetParent(mapOverlay.transform, false);
            var fiRt = fullImgGo.AddComponent<RectTransform>();
            fiRt.anchorMin = new Vector2(0.05f, 0.08f);
            fiRt.anchorMax = new Vector2(0.95f, 0.92f);
            fiRt.offsetMin = Vector2.zero;
            fiRt.offsetMax = Vector2.zero;
            var fullImg = fullImgGo.AddComponent<Image>();
            if (museumSprite != null)
            {
                fullImg.sprite = museumSprite;
                fullImg.color = Color.white;
            }
            else
            {
                fullImg.color = new Color(0.3f, 0.35f, 0.45f);
            }
            fullImg.preserveAspect = true;
            var mapCloseBtn = MakeButton(mapOverlay.transform, "MapCloseBtn", "Kapat",
                new Vector2(0, -250), new Vector2(160, 45));
            mapOverlay.SetActive(false);

            // ── SAĞ ALT: Gazete ──
            var emptyBg = MakePanel(root.transform, "NewspaperQuadrant",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2((qW + gap) / 2f, -(qH + gap) / 2f - 20f), new Vector2(qW, qH),
                new Color(0.12f, 0.11f, 0.1f, 0.95f));
            var newsLabel = MakeText(emptyBg.transform, "Label",
                new Vector2(0, 105), new Vector2(qW - 20, 30), 18, TextAlignmentOptions.Center);
            newsLabel.text = "GAZETE";
            newsLabel.color = new Color(0.9f, 0.85f, 0.6f);
            newsLabel.fontStyle = FontStyles.Bold;

            var newsBtnGo = new GameObject("NewsButton");
            newsBtnGo.transform.SetParent(emptyBg.transform, false);
            var nbRt = newsBtnGo.AddComponent<RectTransform>();
            nbRt.anchorMin = new Vector2(0.5f, 0.5f);
            nbRt.anchorMax = new Vector2(0.5f, 0.5f);
            nbRt.pivot = new Vector2(0.5f, 0.5f);
            nbRt.anchoredPosition = new Vector2(0, -10);
            nbRt.sizeDelta = new Vector2(qW - 40, qH - 60);
            var nbImg = newsBtnGo.AddComponent<Image>();
            nbImg.color = new Color(0.92f, 0.88f, 0.75f);
            var newsBtn = newsBtnGo.AddComponent<Button>();

            var newsInnerLabel = MakeText(newsBtnGo.transform, "InnerLabel",
                Vector2.zero, Vector2.zero, 16, TextAlignmentOptions.Center);
            var nilRt = newsInnerLabel.rectTransform;
            nilRt.anchorMin = Vector2.zero; nilRt.anchorMax = Vector2.one;
            nilRt.offsetMin = new Vector2(10, 10); nilRt.offsetMax = new Vector2(-10, -10);
            newsInnerLabel.text = "Gazeteyi Oku";
            newsInnerLabel.color = new Color(0.2f, 0.15f, 0.1f);
            newsInnerLabel.fontStyle = FontStyles.Bold;

            // "Aktif dava yok" label (dava kabul edilmeden gösterilir)
            var noCaseGo = new GameObject("NoCaseLabel");
            noCaseGo.transform.SetParent(root.transform, false);
            var ncRt = noCaseGo.AddComponent<RectTransform>();
            ncRt.anchorMin = new Vector2(0.5f, 0.5f);
            ncRt.anchorMax = new Vector2(0.5f, 0.5f);
            ncRt.pivot = new Vector2(0.5f, 0.5f);
            ncRt.anchoredPosition = new Vector2(0, 40);
            ncRt.sizeDelta = new Vector2(600, 80);
            var ncTmp = noCaseGo.AddComponent<TextMeshProUGUI>();
            ncTmp.text = "Rafa Kaldırılan Davalar";
            ncTmp.fontSize = 32;
            ncTmp.alignment = TextAlignmentOptions.Center;
            ncTmp.color = new Color(0.5f, 0.45f, 0.35f);
            ncTmp.fontStyle = FontStyles.Italic;
            ncTmp.raycastTarget = false;
            noCaseGo.SetActive(false);

            // Rafa kaldırılan davalar listesi (no-case ekranında görünür)
            var shelvedGo = new GameObject("ShelvedArea");
            shelvedGo.transform.SetParent(root.transform, false);
            var saRt = shelvedGo.AddComponent<RectTransform>();
            saRt.anchorMin = new Vector2(0.5f, 0.5f);
            saRt.anchorMax = new Vector2(0.5f, 0.5f);
            saRt.pivot = new Vector2(0.5f, 1f);
            saRt.anchoredPosition = new Vector2(0, -10);
            saRt.sizeDelta = new Vector2(500, 300);
            var saVlg = shelvedGo.AddComponent<VerticalLayoutGroup>();
            saVlg.spacing = 8f;
            saVlg.padding = new RectOffset(8, 8, 8, 8);
            saVlg.childControlHeight = true;
            saVlg.childControlWidth = true;
            saVlg.childForceExpandHeight = false;
            saVlg.childForceExpandWidth = true;
            shelvedGo.SetActive(false);

            // Kapat butonu
            var closeBtn = MakeButton(root.transform, "CloseBtn", "Kapat",
                new Vector2(0, -290), new Vector2(180, 50));

            // Component
            var caseBoardUI = root.AddComponent<CaseBoardUI>();
            caseBoardUI.Setup(root, suspectsContent, cluesContent, mapBtn, mapOverlay, fullImg, mapCloseBtn, closeBtn,
                suspectsBg, cluesBg, mapBg, emptyBg, noCaseGo, newsBtn, saRt);
            root.SetActive(false);
            return caseBoardUI;
        }

        static GameObject MakeScrollRect(Transform parent, string name, Vector2 anchoredPos, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            go.AddComponent<RectMask2D>();

            var contentGo = new GameObject("Content");
            contentGo.transform.SetParent(go.transform, false);
            var cRt = contentGo.AddComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0, 1);
            cRt.anchorMax = new Vector2(1, 1);
            cRt.pivot = new Vector2(0.5f, 1f);
            cRt.anchoredPosition = Vector2.zero;
            cRt.sizeDelta = new Vector2(0, 0);

            var vlg = contentGo.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 6f;
            vlg.padding = new RectOffset(4, 4, 4, 4);
            vlg.childControlHeight = true;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;

            var csf = contentGo.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = go.AddComponent<ScrollRect>();
            scroll.content = cRt;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;

            return go;
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
