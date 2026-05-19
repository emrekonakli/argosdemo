using System.Collections.Generic;
using UnityEngine;

namespace Argos.NPC
{
    [CreateAssetMenu(fileName = "NPCData", menuName = "ARGOS/NPC Data", order = 1)]
    public class NPCData : ScriptableObject
    {
        public string npcName;
        public string period;
        [TextArea(2, 5)] public string personality;
        public Sprite portrait;

        public List<string> knownFacts = new List<string>();
        public List<string> hiddenFacts = new List<string>();
        [TextArea(2, 4)] public string mandatoryFinalFact;

        public int patienceCount = 6;
    }
}
