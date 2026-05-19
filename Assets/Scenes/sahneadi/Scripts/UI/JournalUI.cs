using UnityEngine;

namespace Argos.UI
{
    public class JournalUI : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Transform listParent;
        [SerializeField] private GameObject itemPrefab;

        public void Show()
        {
            if (root != null) root.SetActive(true);
            // TODO Faz 4: PlayerInventory'den item'ları al ve listParent altında prefab instance et.
        }

        public void Hide()
        {
            if (root != null) root.SetActive(false);
        }
    }
}
