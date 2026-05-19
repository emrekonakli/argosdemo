using UnityEngine;
using Argos.Artifacts;
using Argos.NPC;

namespace Argos.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        public ArtifactInspectUI artifactInspect;
        public JournalUI journal;
        public InternalVoiceUI internalVoice;
        public InterrogationUI interrogation;
        public NewspaperUI newspaper;
        public SuspectSelectionUI suspectSelection;
        public QuestBox questBox;
        public GadgetWarningBanner gadgetWarning;
        public InteractPrompt interactPrompt;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        public void OpenArtifactInspect(ArtifactData data)
        {
            CloseAllPanels();
            if (artifactInspect != null) artifactInspect.Show(data);
        }

        public void OpenJournal()
        {
            CloseAllPanels();
            if (journal != null) journal.Show();
        }

        public void OpenInterrogation(NPCData npc, bool culpritMode)
        {
            CloseAllPanels();
            if (interrogation != null) interrogation.Show(npc, culpritMode);
        }

        public void OpenSuspectSelection()
        {
            CloseAllPanels();
            if (suspectSelection != null) suspectSelection.Show();
        }

        public void OpenNewspaper(bool isEnding, bool isCorrect)
        {
            if (newspaper != null) newspaper.Show(isEnding, isCorrect);
        }

        public void PlayInternalVoice(string line)
        {
            if (internalVoice != null) internalVoice.Play(line);
        }

        public void ShowInteractPrompt(string label)
        {
            if (interactPrompt != null) interactPrompt.Show(label);
        }

        public void HideInteractPrompt()
        {
            if (interactPrompt != null) interactPrompt.Hide();
        }

        public void ShowGadgetWarning(string message)
        {
            if (gadgetWarning != null) gadgetWarning.Show(message);
        }

        public void HideGadgetWarning()
        {
            if (gadgetWarning != null) gadgetWarning.Hide();
        }

        public void CloseAllPanels()
        {
            if (artifactInspect != null) artifactInspect.Hide();
            if (journal != null) journal.Hide();
            if (interrogation != null) interrogation.Hide();
            if (suspectSelection != null) suspectSelection.Hide();
            if (newspaper != null) newspaper.Hide();
            if (internalVoice != null) internalVoice.Close();
        }
    }
}
