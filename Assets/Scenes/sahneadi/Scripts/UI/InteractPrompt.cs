using TMPro;
using UnityEngine;

namespace Argos.UI
{
    public class InteractPrompt : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text label;

        public void Setup(GameObject rootObj, TMP_Text labelField)
        {
            root = rootObj;
            label = labelField;
        }

        public void Show(string text)
        {
            if (root != null) root.SetActive(true);
            if (label != null) label.text = text;
        }

        public void Hide()
        {
            if (root != null) root.SetActive(false);
        }
    }
}
