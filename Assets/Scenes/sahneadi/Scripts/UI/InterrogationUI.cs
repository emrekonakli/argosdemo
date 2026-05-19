using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Argos.AI;
using Argos.Evidence;
using Argos.Game;
using Argos.NPC;
using Argos.Player;

namespace Argos.UI
{
    public class InterrogationUI : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Image npcPortrait;
        [SerializeField] private TMP_Text npcName;
        [SerializeField] private TMP_Text patienceLabel;
        [SerializeField] private TMP_Text chatHistory;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Button sendButton;
        [SerializeField] private Button evidenceButton;
        [SerializeField] private Button summaryButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private GameObject preInterrogationPanel;
        [SerializeField] private TMP_Text preInterrogationLabel;
        [SerializeField] private Button preInterrogationContinue;
        [SerializeField] private GameObject evidencePickerPanel;
        [SerializeField] private RectTransform evidencePickerContent;
        [SerializeField] private Button evidencePickerClose;

        private NPCData currentNpc;
        private bool culpritMode;
        private int remainingPatience;
        private readonly List<ChatMessage> history = new List<ChatMessage>();
        private readonly HashSet<EvidencePhoto> shownEvidence = new HashSet<EvidencePhoto>();
        private readonly StringBuilder chatBuilder = new StringBuilder(1024);
        private bool waitingForReply;

        public void Setup(
            GameObject rootObj,
            Image portrait, TMP_Text nameLbl, TMP_Text patienceLbl, TMP_Text chatLbl,
            TMP_InputField input, Button send, Button evidence, Button summary, Button exit,
            GameObject preRoot, TMP_Text preLabel, Button preContinue,
            GameObject pickerRoot, RectTransform pickerContent, Button pickerCloseBtn)
        {
            root = rootObj;
            npcPortrait = portrait;
            npcName = nameLbl;
            patienceLabel = patienceLbl;
            chatHistory = chatLbl;
            inputField = input;
            sendButton = send;
            evidenceButton = evidence;
            summaryButton = summary;
            exitButton = exit;
            preInterrogationPanel = preRoot;
            preInterrogationLabel = preLabel;
            preInterrogationContinue = preContinue;
            evidencePickerPanel = pickerRoot;
            evidencePickerContent = pickerContent;
            evidencePickerClose = pickerCloseBtn;

            if (sendButton != null) sendButton.onClick.AddListener(OnSend);
            if (exitButton != null) exitButton.onClick.AddListener(Hide);
            if (evidenceButton != null) evidenceButton.onClick.AddListener(OpenEvidencePicker);
            if (summaryButton != null) summaryButton.onClick.AddListener(OpenSummary);
            if (preInterrogationContinue != null) preInterrogationContinue.onClick.AddListener(CloseSummary);
            if (evidencePickerClose != null) evidencePickerClose.onClick.AddListener(CloseEvidencePicker);
        }

        public void Show(NPCData npc, bool culprit)
        {
            if (npc == null) return;
            currentNpc = npc;
            culpritMode = culprit;
            history.Clear();
            shownEvidence.Clear();
            chatBuilder.Clear();
            remainingPatience = npc.patienceCount;
            waitingForReply = false;

            if (root != null) root.SetActive(true);
            if (npcName != null) npcName.text = npc.npcName + (culprit ? " (SUÇLU)" : "");
            if (npcPortrait != null) npcPortrait.sprite = npc.portrait;
            if (chatHistory != null) chatHistory.text = "";
            if (exitButton != null) exitButton.gameObject.SetActive(!culprit);
            UpdatePatienceLabel();
            OpenSummary();
        }

        public void Hide()
        {
            if (root != null) root.SetActive(false);
            if (preInterrogationPanel != null) preInterrogationPanel.SetActive(false);
            if (evidencePickerPanel != null) evidencePickerPanel.SetActive(false);
        }

        // -------- Summary (pre-interrogation) --------
        void OpenSummary()
        {
            if (preInterrogationPanel == null || preInterrogationLabel == null) return;
            preInterrogationPanel.SetActive(true);

            var sb = new StringBuilder();
            sb.AppendLine("<b>Bilinen bilgiler</b>");
            var inv = PlayerInventory.Instance;
            if (inv != null && inv.Notes.Count > 0)
            {
                foreach (var note in inv.Notes)
                    sb.AppendLine($"• {note.title}: {note.description}");
            }
            else
            {
                sb.AppendLine("• (Defterde henüz not yok.)");
            }
            sb.AppendLine();
            sb.AppendLine($"<b>Toplanan fotoğraflar:</b> {(inv != null ? inv.Photos.Count : 0)}");
            preInterrogationLabel.text = sb.ToString();
        }

        void CloseSummary()
        {
            if (preInterrogationPanel != null) preInterrogationPanel.SetActive(false);
        }

        // -------- Send / AI reply --------
        async void OnSend()
        {
            if (waitingForReply) return;
            if (inputField == null || string.IsNullOrWhiteSpace(inputField.text)) return;

            string userMsg = inputField.text;
            inputField.text = "";
            AppendChat("Sen", userMsg);
            remainingPatience = Mathf.Max(0, remainingPatience - 1);
            UpdatePatienceLabel();

            waitingForReply = true;
            string reply = AIManager.Instance != null
                ? await AIManager.Instance.SendChat(BuildSystemPrompt(), history, userMsg)
                : "...";
            waitingForReply = false;

            AppendChat(currentNpc != null ? currentNpc.npcName : "NPC", reply);

            if (culpritMode && reply != null && reply.StartsWith("İTİRAF:"))
            {
                GameManager.Instance?.SolveCase(currentNpc);
                if (ScoreManager.Instance != null)
                    ScoreManager.Instance.LastInterrogationRemainingPatience = remainingPatience;
                // TODO Faz 11/12: NewspaperUI ending akışı.
                return;
            }

            if (remainingPatience <= 0 && !culpritMode)
            {
                AppendChat("Sistem", "Görüşme sona erdi.");
                if (ScoreManager.Instance != null)
                    ScoreManager.Instance.LastInterrogationRemainingPatience = 0;
                Hide();
            }
        }

        void AppendChat(string speaker, string line)
        {
            history.Add(new ChatMessage { role = speaker, content = line });
            chatBuilder.AppendLine($"<b>{speaker}:</b> {line}");
            if (chatHistory != null) chatHistory.text = chatBuilder.ToString();
        }

        void UpdatePatienceLabel()
        {
            if (patienceLabel == null) return;
            patienceLabel.text = culpritMode ? "SUÇLU SORGUSU" : $"Sabır: {remainingPatience}/{(currentNpc != null ? currentNpc.patienceCount : 0)}";
        }

        string BuildSystemPrompt()
        {
            if (currentNpc == null) return "";
            var sb = new StringBuilder();
            sb.Append($"Sen {currentNpc.npcName} adlı karaktersin. {currentNpc.period}. ");
            sb.AppendLine($"Kişilik: {currentNpc.personality}");
            if (currentNpc.knownFacts != null && currentNpc.knownFacts.Count > 0)
                sb.AppendLine("Bildiğin bilgiler: " + string.Join("; ", currentNpc.knownFacts));
            if (currentNpc.hiddenFacts != null && currentNpc.hiddenFacts.Count > 0)
                sb.AppendLine("Saklamak istediğin bilgiler: " + string.Join("; ", currentNpc.hiddenFacts));
            if (remainingPatience <= 1 && !string.IsNullOrEmpty(currentNpc.mandatoryFinalFact))
                sb.AppendLine("Son hakta mutlaka bu bilgiyi ver: " + currentNpc.mandatoryFinalFact);
            sb.AppendLine($"Sabır: {remainingPatience} hakkın kaldı. Kısa, dönemine uygun cümlelerle konuş.");
            if (culpritMode)
            {
                sb.AppendLine("Sen suçlusun. Oyuncu yeterli kanıt sunarsa cevabını 'İTİRAF:' ile başlat ve gerçeği itiraf et. Yeterli kanıt yoksa inkâr et.");
                if (shownEvidence.Count > 0)
                {
                    sb.Append("Oyuncu şu kanıtları sundu: ");
                    foreach (var ev in shownEvidence) sb.Append("[" + ev.title + "] ");
                    sb.AppendLine();
                }
            }
            return sb.ToString();
        }

        // -------- Evidence picker --------
        void OpenEvidencePicker()
        {
            if (evidencePickerPanel == null || evidencePickerContent == null) return;
            evidencePickerPanel.SetActive(true);

            for (int i = evidencePickerContent.childCount - 1; i >= 0; i--)
                Destroy(evidencePickerContent.GetChild(i).gameObject);

            var inv = PlayerInventory.Instance;
            if (inv == null || inv.Photos.Count == 0)
            {
                CreatePickerLabel("Defterde henüz fotoğraf yok.");
                return;
            }

            foreach (var photo in inv.Photos)
            {
                if (shownEvidence.Contains(photo)) continue;
                CreatePickerButton(photo);
            }
        }

        void CreatePickerLabel(string text)
        {
            var go = new GameObject("EmptyLabel");
            go.transform.SetParent(evidencePickerContent, false);
            var le = go.AddComponent<LayoutElement>();
            le.minHeight = 40f;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 14;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(0.85f, 0.85f, 0.85f);
        }

        void CreatePickerButton(EvidencePhoto photo)
        {
            var go = new GameObject("Photo_" + photo.title);
            go.transform.SetParent(evidencePickerContent, false);
            var le = go.AddComponent<LayoutElement>();
            le.minHeight = 50f;
            var img = go.AddComponent<Image>();
            img.color = new Color(0.95f, 0.85f, 0.2f, 0.9f);
            var btn = go.AddComponent<Button>();

            var label = new GameObject("Label");
            label.transform.SetParent(go.transform, false);
            var rt = label.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(10f, 4f);
            rt.offsetMax = new Vector2(-10f, -4f);
            var tmp = label.AddComponent<TextMeshProUGUI>();
            tmp.text = "[FOTO] " + photo.title;
            tmp.fontSize = 16;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.color = Color.black;

            btn.onClick.AddListener(() =>
            {
                shownEvidence.Add(photo);
                AppendChat("Sen", "[FOTO: " + photo.title + "]");
                CloseEvidencePicker();
            });
        }

        void CloseEvidencePicker()
        {
            if (evidencePickerPanel != null) evidencePickerPanel.SetActive(false);
        }
    }
}
