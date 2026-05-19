using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Argos.Game;

namespace Argos.UI
{
    public class NewspaperUI : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text headline;
        [SerializeField] private TMP_Text dateLabel;
        [SerializeField] private TMP_Text scoreLabel;
        [SerializeField] private Button primaryButton;
        [SerializeField] private TMP_Text primaryButtonLabel;
        [SerializeField] private Button secondaryButton;
        [SerializeField] private TMP_Text secondaryButtonLabel;

        public void Show(bool isEnding, bool isCorrect)
        {
            if (root != null) root.SetActive(true);

            if (!isEnding)
            {
                if (headline != null) headline.text = "ESRARENGİZ CİNAYET MÜZEDE!";
                if (primaryButtonLabel != null) primaryButtonLabel.text = "DAVAYI ÜSTLEN";
                if (secondaryButton != null) secondaryButton.gameObject.SetActive(false);
                if (scoreLabel != null) scoreLabel.text = "";
                BindPrimary(() => SceneManager.LoadScene(Scenes.Museum));
                return;
            }

            int score = ScoreManager.Instance != null ? ScoreManager.Instance.CalculateScore() : 0;
            if (scoreLabel != null) scoreLabel.text = $"Puan: {score}";

            if (isCorrect)
            {
                if (headline != null) headline.text = "DAVA ÇÖZÜLDÜ!";
                if (primaryButtonLabel != null) primaryButtonLabel.text = "TEKRAR OYNA";
                if (secondaryButtonLabel != null) secondaryButtonLabel.text = "ANA MENÜ";
            }
            else
            {
                if (headline != null) headline.text = "YANLIŞ SUÇLAMA!";
                if (primaryButtonLabel != null) primaryButtonLabel.text = "TEKRAR DENE";
                if (secondaryButtonLabel != null) secondaryButtonLabel.text = "ANA MENÜ";
            }
            BindPrimary(() => { GameManager.Instance?.RestartScenario(); SceneManager.LoadScene(Scenes.Museum); });
        }

        void BindPrimary(System.Action callback)
        {
            if (primaryButton == null) return;
            primaryButton.onClick.RemoveAllListeners();
            primaryButton.onClick.AddListener(() => callback?.Invoke());
        }

        public void Hide()
        {
            if (root != null) root.SetActive(false);
        }
    }
}
