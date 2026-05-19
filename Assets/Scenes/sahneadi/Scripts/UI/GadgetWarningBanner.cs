using TMPro;
using UnityEngine;

namespace Argos.UI
{
    public class GadgetWarningBanner : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text label;

        public void Setup(GameObject rootObj, TMP_Text labelField)
        {
            root = rootObj;
            label = labelField;
        }

        public void Show(string message)
        {
            if (root != null) root.SetActive(true);
            if (label != null) label.text = message;
        }

        public void Hide()
        {
            if (root != null) root.SetActive(false);
        }
    }
}
