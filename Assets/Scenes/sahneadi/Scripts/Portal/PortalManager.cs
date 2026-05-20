using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Argos.Game;

namespace Argos.Portal
{
    public class PortalManager : MonoBehaviour
    {
        public static PortalManager Instance { get; private set; }

        [SerializeField] private CanvasGroup fadeOverlay;
        [SerializeField] private float fadeDuration = 0.5f;
        // Faz 13 placeholder ses: clip atanırsa portal/fade'de çalar, atanmazsa no-op.
        [SerializeField] private AudioSource portalOpenAudio;
        [SerializeField] private AudioSource fadeAudio;

        public bool IsInPortal { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureFadeOverlay();
            EnsureAudioSources();
        }

        void EnsureAudioSources()
        {
            if (portalOpenAudio == null)
            {
                portalOpenAudio = gameObject.AddComponent<AudioSource>();
                portalOpenAudio.playOnAwake = false;
            }
            if (fadeAudio == null)
            {
                fadeAudio = gameObject.AddComponent<AudioSource>();
                fadeAudio.playOnAwake = false;
            }
        }

        // PortalManager kendi DDOL canvas'ında bir tam ekran siyah overlay tutar;
        // bu sayede fade sahne geçişinden etkilenmez.
        void EnsureFadeOverlay()
        {
            if (fadeOverlay != null) return;

            var canvasGo = new GameObject("PortalFadeCanvas");
            DontDestroyOnLoad(canvasGo);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();

            var overlayGo = new GameObject("FadeOverlay");
            overlayGo.transform.SetParent(canvasGo.transform, false);
            var rt = overlayGo.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var img = overlayGo.AddComponent<Image>();
            img.color = Color.black;
            var cg = overlayGo.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.blocksRaycasts = false;
            fadeOverlay = cg;
        }

        // Eski API; artık no-op (PortalManager kendi overlay'ini Awake'de kuruyor).
        public void SetFadeOverlay(CanvasGroup overlay) { }

        public void OpenPortal()
        {
            QuestManager.Instance?.TryAdvance(1);
            ShakeMainCamera();
            PlayIfHasClip(portalOpenAudio);
            StartCoroutine(OpenPortalRoutine());
        }

        static void PlayIfHasClip(AudioSource src)
        {
            if (src != null && src.clip != null) src.Play();
        }

        public void ReturnToMuseum()
        {
            QuestManager.Instance?.TryAdvance(2);
            StartCoroutine(ReturnRoutine());
        }

        void ShakeMainCamera()
        {
            var cam = Camera.main;
            if (cam == null) return;
            cam.GetComponent<ArgosCameraFollow>()?.Shake(0.3f, 0.25f);
        }

        IEnumerator OpenPortalRoutine()
        {
            yield return Fade(0f, 1f);
            IsInPortal = true;
            SceneManager.LoadScene(Scenes.Portal);
            yield return Fade(1f, 0f);
        }

        IEnumerator ReturnRoutine()
        {
            yield return Fade(0f, 1f);
            IsInPortal = false;
            SceneManager.LoadScene(Scenes.Museum);
            yield return Fade(1f, 0f);
        }

        IEnumerator Fade(float from, float to)
        {
            if (fadeOverlay == null) yield break;
            PlayIfHasClip(fadeAudio);
            fadeOverlay.alpha = from;
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
