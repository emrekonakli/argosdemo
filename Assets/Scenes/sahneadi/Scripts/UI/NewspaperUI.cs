using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Argos.Game;
using Argos.Scenarios;

namespace Argos.UI
{
    public class NewspaperUI : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text headline;
        [SerializeField] private TMP_Text dateLabel;
        [SerializeField] private TMP_Text bodyLabel;
        [SerializeField] private TMP_Text scoreLabel;
        [SerializeField] private Button primaryButton;
        [SerializeField] private TMP_Text primaryButtonLabel;
        [SerializeField] private Button secondaryButton;
        [SerializeField] private TMP_Text secondaryButtonLabel;

        public bool IsOpen => root != null && root.activeSelf;

        public void Setup(GameObject rootObj, TMP_Text headlineLbl, TMP_Text dateLbl, TMP_Text bodyLbl, TMP_Text scoreLbl, Button pri, TMP_Text priLbl, Button sec, TMP_Text secLbl)
        {
            root = rootObj;
            headline = headlineLbl;
            dateLabel = dateLbl;
            bodyLabel = bodyLbl;
            scoreLabel = scoreLbl;
            primaryButton = pri;
            primaryButtonLabel = priLbl;
            secondaryButton = sec;
            secondaryButtonLabel = secLbl;
        }

        public void Show(bool isEnding, bool isCorrect)
        {
            if (root != null) root.SetActive(true);

            ScenarioData scenario = GameManager.Instance != null ? GameManager.Instance.CurrentScenario : null;
            if (dateLabel != null) dateLabel.text = scenario != null ? scenario.scenarioDate : "";

            if (!isEnding)
            {
                if (headline != null) headline.text = scenario != null && !string.IsNullOrEmpty(scenario.newspaperHeadline)
                    ? scenario.newspaperHeadline
                    : "ESRARENGİZ CİNAYET MÜZEDE!";
                if (bodyLabel != null) bodyLabel.text = scenario != null
                    ? $"<i>{scenario.scenarioTitle}</i>\n\nMüze'de yaşanan esrarengiz ölüm yetkilileri şaşkına çevirdi. Dedektif Karaca davanın peşinde."
                    : "";
                bool alreadyAccepted = GameManager.Instance != null && GameManager.Instance.CaseAccepted;
                if (primaryButtonLabel != null) primaryButtonLabel.text = alreadyAccepted ? "DAVAYI BIRAK" : "DAVAYI ÜSTLEN";
                if (scoreLabel != null) scoreLabel.text = "";
                if (alreadyAccepted)
                {
                    if (secondaryButton != null) secondaryButton.gameObject.SetActive(true);
                    if (secondaryButtonLabel != null) secondaryButtonLabel.text = "Çık";
                    BindPrimary(() =>
                    {
                        if (GameManager.Instance != null)
                        {
                            GameManager.Instance.ShelveCase();
                        }
                        Hide();
                    });
                    BindSecondary(() => Hide());
                }
                else
                {
                    if (secondaryButton != null) secondaryButton.gameObject.SetActive(false);
                    BindPrimary(() =>
                    {
                        if (GameManager.Instance != null) GameManager.Instance.CaseAccepted = true;
                        Hide();
                        if (SceneManager.GetActiveScene().name == Scenes.Newspaper)
                            SceneManager.LoadScene(Scenes.Office);
                    });
                }
                return;
            }

            int score = ScoreManager.Instance != null ? ScoreManager.Instance.CalculateScore() : 0;
            if (scoreLabel != null) scoreLabel.text = $"Puan: {score}";

            if (isCorrect)
            {
                if (headline != null) headline.text = "DAVA ÇÖZÜLDÜ!";
                if (bodyLabel != null) bodyLabel.text = $"Dedektif Karaca, davanın gerçek faili olan <b>{(GameManager.Instance?.CurrentScenario?.culprit?.npcName ?? "suçlu")}</b>'yu kıstırdı ve itirafa yöneltti. Saraydan altın bir madalya yolda.";
                if (primaryButtonLabel != null) primaryButtonLabel.text = "TEKRAR OYNA";
                if (secondaryButton != null) secondaryButton.gameObject.SetActive(true);
                if (secondaryButtonLabel != null) secondaryButtonLabel.text = "Ana Menü";
            }
            else
            {
                if (headline != null) headline.text = "YANLIŞ SUÇLAMA!";
                if (bodyLabel != null) bodyLabel.text = "Dedektif Karaca yanlış kişiyi suçladı. Gerçek fail kayıplara karıştı, müze'de tedirgin bir bekleyiş başladı.";
                if (primaryButtonLabel != null) primaryButtonLabel.text = "TEKRAR DENE";
                if (secondaryButton != null) secondaryButton.gameObject.SetActive(true);
                if (secondaryButtonLabel != null) secondaryButtonLabel.text = "Ana Menü";
            }
            BindPrimary(() =>
            {
                GameManager.Instance?.RestartScenario();
                SceneManager.LoadScene(Scenes.Office);
            });
            BindSecondary(() => SceneManager.LoadScene(Scenes.Office));
        }

        void BindPrimary(System.Action callback)
        {
            if (primaryButton == null) return;
            primaryButton.onClick.RemoveAllListeners();
            primaryButton.onClick.AddListener(() => callback?.Invoke());
        }

        void BindSecondary(System.Action callback)
        {
            if (secondaryButton == null) return;
            secondaryButton.onClick.RemoveAllListeners();
            secondaryButton.onClick.AddListener(() => callback?.Invoke());
        }

        public void Hide()
        {
            if (root != null) root.SetActive(false);
        }

        void Update()
        {
            // Panel açıkken Space / Enter primary button'ı tetiklesin
            // (intro: DAVAYI ÜSTLEN, ending: TEKRAR OYNA / TEKRAR DENE).
            if (root == null || !root.activeSelf) return;
            if (primaryButton == null || !primaryButton.gameObject.activeInHierarchy || !primaryButton.interactable) return;
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                primaryButton.onClick.Invoke();
        }
    }
}
