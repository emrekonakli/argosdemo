using System.Collections.Generic;
using UnityEngine;
using Argos.NPC;

namespace Argos.Scenarios
{
    [CreateAssetMenu(fileName = "ScenarioData", menuName = "ARGOS/Scenario Data", order = 2)]
    public class ScenarioData : ScriptableObject
    {
        public string scenarioTitle;
        [TextArea(3, 8)] public string newspaperHeadline;
        public string scenarioDate;
        public Sprite newspaperSprite;

        public NPCData culprit;
        public List<NPCData> suspects = new List<NPCData>();
    }
}
