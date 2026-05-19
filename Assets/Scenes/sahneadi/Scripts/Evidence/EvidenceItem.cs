using System;
using UnityEngine;

namespace Argos.Evidence
{
    [Serializable]
    public abstract class EvidenceItem
    {
        public string title;
        public string description;
        public Sprite thumbnail;
    }
}
