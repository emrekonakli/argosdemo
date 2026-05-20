using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Argos.Artifacts;

namespace Argos.UI
{
    public class ArtifactInspectUI : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text nameLabel;
        [SerializeField] private TMP_Text periodOriginLabel;
        [SerializeField] private TMP_Text descriptionLabel;
        [SerializeField] private Button photoButton;
        [SerializeField] private Button closeButton;

        public ArtifactData CurrentData { get; private set; }
        public bool IsOpen => root != null && root.activeSelf;

        public System.Action<ArtifactData> OnTakePhoto;

        public void Setup(GameObject rootObj, TMP_Text nameField, TMP_Text periodField, TMP_Text descField, Button photoBtn, Button closeBtn)
        {
            root = rootObj;
            nameLabel = nameField;
            periodOriginLabel = periodField;
            descriptionLabel = descField;
            photoButton = photoBtn;
            closeButton = closeBtn;
            if (closeButton != null) closeButton.onClick.AddListener(Hide);
            if (photoButton != null) photoButton.onClick.AddListener(() => OnTakePhoto?.Invoke(CurrentData));
        }

        public void Show(ArtifactData data)
        {
            CurrentData = data;
            if (root != null) root.SetActive(true);
            if (data == null) return;
            if (nameLabel != null) nameLabel.text = data.artifactName;
            if (periodOriginLabel != null) periodOriginLabel.text = $"{data.artifactPeriod} • {data.artifactOrigin}";
            if (descriptionLabel != null) descriptionLabel.text = data.artifactDescription;
            if (photoButton != null) photoButton.interactable = data != null;
        }

        public void Hide()
        {
            CurrentData = null;
            if (root != null) root.SetActive(false);
        }
    }
}
