using UnityEngine;

namespace Argos.Artifacts
{
    [CreateAssetMenu(fileName = "ArtifactData", menuName = "ARGOS/Artifact Data", order = 0)]
    public class ArtifactData : ScriptableObject
    {
        public string artifactName;
        [TextArea(3, 8)] public string artifactDescription;
        public string artifactPeriod;
        public string artifactOrigin;
        public Sprite artifactSprite;
        public GameObject artifact3DModel;

        public bool isEvidenceTarget;
        public bool isPortalTrigger;

        [TextArea(2, 4)] public string internalVoiceLine;
    }
}
