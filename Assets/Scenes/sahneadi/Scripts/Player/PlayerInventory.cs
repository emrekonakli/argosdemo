using System.Collections.Generic;
using UnityEngine;
using Argos.Evidence;

namespace Argos.Player
{
    public class PlayerInventory : MonoBehaviour
    {
        public static PlayerInventory Instance { get; private set; }

        private readonly List<EvidenceNote> notes = new List<EvidenceNote>();
        private readonly List<EvidencePhoto> photos = new List<EvidencePhoto>();

        public IReadOnlyList<EvidenceNote> Notes => notes;
        public IReadOnlyList<EvidencePhoto> Photos => photos;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void AddNote(EvidenceNote note)
        {
            if (note == null) return;
            notes.Add(note);
        }

        public void AddPhoto(EvidencePhoto photo)
        {
            if (photo == null) return;
            photos.Add(photo);
        }

        public List<EvidenceItem> GetAll()
        {
            var all = new List<EvidenceItem>();
            all.AddRange(notes);
            all.AddRange(photos);
            return all;
        }

        public void Clear()
        {
            notes.Clear();
            photos.Clear();
        }
    }
}
