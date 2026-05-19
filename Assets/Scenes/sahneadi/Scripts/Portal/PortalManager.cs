using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Argos.Portal
{
    public class PortalManager : MonoBehaviour
    {
        public static PortalManager Instance { get; private set; }

        [SerializeField] private CanvasGroup fadeOverlay;
        [SerializeField] private float fadeDuration = 0.5f;

        public bool IsInPortal { get; private set; }

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

        public void SetFadeOverlay(CanvasGroup overlay) => fadeOverlay = overlay;

        public void OpenPortal()
        {
            StartCoroutine(OpenPortalRoutine());
        }

        public void ReturnToMuseum()
        {
            StartCoroutine(ReturnRoutine());
        }

        IEnumerator OpenPortalRoutine()
        {
            yield return Fade(0f, 1f);
            IsInPortal = true;
            SceneManager.LoadScene("PortalScene");
            yield return Fade(1f, 0f);
        }

        IEnumerator ReturnRoutine()
        {
            yield return Fade(0f, 1f);
            IsInPortal = false;
            SceneManager.LoadScene("MuseumScene");
            yield return Fade(1f, 0f);
        }

        IEnumerator Fade(float from, float to)
        {
            if (fadeOverlay == null) yield break;
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                fadeOverlay.alpha = Mathf.Lerp(from, to, t / fadeDuration);
                yield return null;
            }
            fadeOverlay.alpha = to;
        }
    }
}
