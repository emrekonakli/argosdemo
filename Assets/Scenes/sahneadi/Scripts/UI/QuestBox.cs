using TMPro;
using UnityEngine;

namespace Argos.UI
{
    public class QuestBox : MonoBehaviour
    {
        [SerializeField] private TMP_Text questText;

        public void SetTextField(TMP_Text field) => questText = field;

        public void SetText(string text)
        {
            if (questText != null) questText.text = text;
        }
    }
}
