using System.Collections.Generic;
using UnityEngine;
using Argos.Game;
using Argos.NPC;

namespace Argos.UI
{
    public class SuspectSelectionUI : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Transform listParent;
        [SerializeField] private GameObject suspectItemPrefab;
        [SerializeField] private List<NPCData> suspects = new List<NPCData>();

        public void SetSuspects(List<NPCData> list) => suspects = list;

        public void Show()
        {
            if (root != null) root.SetActive(true);
            // TODO Faz 10: suspects listesini listParent altında item prefab'larıyla doldur.
        }

        public void Hide()
        {
            if (root != null) root.SetActive(false);
        }

        public void OnSuspectChosen(NPCData chosen)
        {
            if (chosen == null) return;
            bool isCulprit = GameManager.Instance != null
                && GameManager.Instance.CurrentScenario != null
                && GameManager.Instance.CurrentScenario.culprit == chosen;
            if (!isCulprit) GameManager.Instance?.RegisterWrongSuspect();
            UIManager.Instance?.OpenInterrogation(chosen, true);
        }
    }
}
