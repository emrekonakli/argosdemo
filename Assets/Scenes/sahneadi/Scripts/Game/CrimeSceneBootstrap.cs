using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Argos.UI;

namespace Argos.Game
{
    /// <summary>
    /// OlayYeriScene (flashback / cinayet mahalli) için runtime kurulum.
    /// Karakter YOK — sahne tamamen mouse ile incelenir. Sadece arka plan,
    /// sabit kamera, UI ve sol-tık ile çalışan "Çıkış" butonu kurulur.
    /// İncelenebilir ipuçları bir sonraki adımda eklenecek.
    /// </summary>
    public class CrimeSceneBootstrap : MonoBehaviour
    {
        [Header("Build flags")]
        public bool buildOnStart = true;

        private Camera cam;
        private Canvas canvas;
        private UIManager uiManager;
        private GameObject hiddenPlayer;

        void Start()
        {
            if (buildOnStart) BuildScene();
        }

        public void BuildScene()
        {
            HidePlayer();
            BuildBackground();
            BuildCamera();
            BuildCanvas();
        }

        // Karakter kullanılmıyor: PortalScene'den taşınan DontDestroyOnLoad'lu
        // player varsa gizle. Çıkışta tekrar aktif edilecek (yoksa PortalScene
        // yeni bir player üretir ve çift player olur).
        void HidePlayer()
        {
            var existing = GameObject.FindGameObjectWithTag("Player");
            if (existing != null)
            {
                hiddenPlayer = existing;
                hiddenPlayer.SetActive(false);
            }
        }

        void BuildBackground()
        {
            var sprite = Resources.Load<Sprite>("CrimeSceneBackground");
            if (sprite == null) return;

            var go = new GameObject("CrimeSceneBackground");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = -10;
            go.transform.position = Vector3.zero;
        }

        // Sabit kamera: karakter olmadığı için takip yok. Arka planı (origin'de,
        // ~22x12.4 birim) ortalar; orthographicSize tam yüksekliği gösterir.
        void BuildCamera()
        {
            cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                cam = camGo.AddComponent<Camera>();
                camGo.AddComponent<AudioListener>();
            }

            cam.orthographic = true;
            cam.orthographicSize = 6.2f;
            cam.backgroundColor = new Color(0.08f, 0.07f, 0.06f);
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.transform.rotation = Quaternion.identity;

            var follow = cam.GetComponent<ArgosCameraFollow>();
            if (follow != null) follow.enabled = false;

            if (cam.GetComponent<AudioListener>() == null)
                cam.gameObject.AddComponent<AudioListener>();
        }

        void BuildCanvas()
        {
            var canvasGo = new GameObject("Canvas");
            canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();

            if (FindAnyObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }

            BuildExitButton(canvasGo.transform);

            uiManager = canvasGo.AddComponent<UIManager>();
        }

        // Sol tık ile tetiklenen Çıkış butonu (sağ üst köşe) → PortalScene.
        void BuildExitButton(Transform parent)
        {
            var go = new GameObject("ExitButton");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-20f, -20f);
            rt.sizeDelta = new Vector2(150f, 56f);

            var img = go.AddComponent<Image>();
            img.color = new Color(0.6f, 0.18f, 0.16f, 0.95f);

            var btn = go.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = new Color(0.75f, 0.25f, 0.22f, 1f);
            colors.pressedColor = new Color(0.45f, 0.12f, 0.1f, 1f);
            btn.colors = colors;
            btn.onClick.AddListener(OnExitClicked);

            var lblGo = new GameObject("Label");
            lblGo.transform.SetParent(go.transform, false);
            var lrt = lblGo.AddComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = Vector2.zero;
            lrt.offsetMax = Vector2.zero;
            var tmp = lblGo.AddComponent<TextMeshProUGUI>();
            tmp.text = "Çıkış";
            tmp.fontSize = 24;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
        }

        void OnExitClicked()
        {
            if (hiddenPlayer != null) hiddenPlayer.SetActive(true);
            SceneManager.LoadScene(Scenes.Portal);
        }
    }
}
