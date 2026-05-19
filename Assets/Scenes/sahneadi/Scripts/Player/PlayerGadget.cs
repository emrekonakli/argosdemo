using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Argos.Artifacts;
using Argos.Evidence;

namespace Argos.Player
{
    public class PlayerGadget : MonoBehaviour
    {
        [SerializeField] private CanvasGroup flashOverlay;
        [SerializeField] private AudioSource cameraSound;
        [SerializeField] private float flashDuration = 0.15f;

        public void SetFlashOverlay(CanvasGroup overlay) => flashOverlay = overlay;
        public void SetCameraSound(AudioSource src) => cameraSound = src;

        public void TakePhoto(ArtifactData artifact)
        {
            if (artifact == null) return;
            PlayerInventory.Instance?.AddPhoto(new EvidencePhoto
            {
                title = artifact.artifactName,
                description = artifact.artifactDescription,
                thumbnail = artifact.artifactSprite,
                sourceArtifact = artifact
            });
            if (cameraSound != null) cameraSound.Play();
            StartCoroutine(FlashRoutine());
        }

        IEnumerator FlashRoutine()
        {
            if (flashOverlay == null) yield break;
            flashOverlay.alpha = 1f;
            float t = 0f;
            while (t < flashDuration)
            {
                t += Time.deltaTime;
                flashOverlay.alpha = Mathf.Lerp(1f, 0f, t / flashDuration);
                yield return null;
            }
            flashOverlay.alpha = 0f;
        }
    }
}
