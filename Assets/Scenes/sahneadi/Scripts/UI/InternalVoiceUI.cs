using TMPro;
using UnityEngine;

namespace Argos.UI
{
    public class InternalVoiceUI : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text label;
        [SerializeField] private float displaySeconds = 3f;

        public void Setup(GameObject rootObj, TMP_Text labelField)
        {
            root = rootObj;
            label = labelField;
        }

        public void Play(string line)
        {
            CancelInvoke(nameof(Close));
            if (root != null) root.SetActive(true);
            if (label != null) label.text = line;
            Invoke(nameof(Close), displaySeconds);
        }

        public void Close()
        {
            if (root != null) root.SetActive(false);
        }
    }
}
