using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Argos.AI;
using Argos.Artifacts;
using Argos.Joystick;
using Argos.Player;
using Argos.Portal;
using Argos.Scenarios;
using Argos.UI;
using Argos.Game;

namespace Argos.Game
{
    /// <summary>
    /// MVP'de sahneyi runtime'da prosedürel kurar. Faz 2'de Tilemap eklenince
    /// buildWalls=false yapılabilir; Player/Camera/Canvas/Managers tarafı kalır.
    /// </summary>
    public class MuseumSceneBootstrap : MonoBehaviour
    {
        [Header("Build flags")]
        public bool buildOnStart = true;
        public bool buildWalls = true;

        [Header("Optional refs")]
        public ArtifactData[] sampleArtifacts;
        public AIConfig aiConfig;
        public ScenarioData scenario;

        [Header("Room layout")]
        public Vector2 roomMin = new Vector2(-10f, -7.5f);
        public Vector2 roomMax = new Vector2(10f, 7.5f);

        private GameObject managersRoot;
        private GameObject player;
        private Camera cam;
        private Canvas canvas;
        private UIManager uiManager;

        void Start()
        {
            if (buildOnStart) BuildScene();
        }

        public void BuildScene()
        {
            BuildManagers();
            if (buildWalls) BuildWalls();
            BuildPlayer();
            BuildCamera();
            BuildCanvas();
            WireUp();
        }

        // ---------------------------------------------------------------
        // Managers
        // ---------------------------------------------------------------
        void BuildManagers()
        {
            managersRoot = new GameObject("Managers");
            managersRoot.AddComponent<GameManager>();
            managersRoot.AddComponent<QuestManager>();
            managersRoot.AddComponent<ScoreManager>();
            managersRoot.AddComponent<PortalManager>();
            var ai = managersRoot.AddComponent<AIManager>();
            var fb = managersRoot.AddComponent<FallbackResponseManager>();
            if (aiConfig != null) ai.SetConfig(aiConfig);
            ai.SetFallback(fb);
            if (scenario != null) GameManager.Instance?.SetScenario(scenario);
        }

        // ---------------------------------------------------------------
        // Walls (placeholder — Faz 2'de Tilemap'le değiştirilecek)
        // ---------------------------------------------------------------
        void BuildWalls()
        {
            var wallsRoot = new GameObject("PlaceholderWalls");
            float thickness = 0.5f;
            Color wallColor = new Color(0.2f, 0.2f, 0.22f);

            float w = roomMax.x - roomMin.x;
            float h = roomMax.y - roomMin.y;
            Vector2 center = (roomMin + roomMax) * 0.5f;

            MakeWall(wallsRoot.transform, "Wall_Top",    new Vector2(center.x, roomMax.y + thickness * 0.5f), new Vector2(w + thickness * 2f, thickness), wallColor);
            MakeWall(wallsRoot.transform, "Wall_Bottom", new Vector2(center.x, roomMin.y - thickness * 0.5f), new Vector2(w + thickness * 2f, thickness), wallColor);
            MakeWall(wallsRoot.transform, "Wall_Left",   new Vector2(roomMin.x - thickness * 0.5f, center.y), new Vector2(thickness, h), wallColor);
            MakeWall(wallsRoot.transform, "Wall_Right",  new Vector2(roomMax.x + thickness * 0.5f, center.y), new Vector2(thickness, h), wallColor);

            // Zemin (görsel, sadece arka plan)
            var floor = new GameObject("PlaceholderFloor");
            var fsr = floor.AddComponent<SpriteRenderer>();
            fsr.sprite = MakeWhiteSprite();
            fsr.color = new Color(0.78f, 0.78f, 0.74f);
            fsr.sortingOrder = -10;
            floor.transform.position = new Vector3(center.x, center.y, 0f);
            floor.transform.localScale = new Vector3(w, h, 1f);
        }

        void MakeWall(Transform parent, string name, Vector2 pos, Vector2 size, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = new Vector3(pos.x, pos.y, 0f);
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = MakeWhiteSprite();
            sr.color = color;
            sr.sortingOrder = 0;
            go.AddComponent<BoxCollider2D>();
        }

        // ---------------------------------------------------------------
        // Player
        // ---------------------------------------------------------------
        void BuildPlayer()
        {
            player = new GameObject("Player");
            player.tag = "Player";
            player.transform.position = Vector3.zero;
            player.transform.localScale = new Vector3(0.5f, 0.5f, 1f);

            var sr = player.AddComponent<SpriteRenderer>();
            sr.sprite = MakeWhiteSprite();
            sr.color = new Color(0.25f, 0.55f, 0.95f);
            sr.sortingOrder = 10;

            var rb = player.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            player.AddComponent<BoxCollider2D>();
            player.AddComponent<PlayerController>();
            player.AddComponent<PlayerInventory>();
            player.AddComponent<PlayerGadget>();
        }

        // ---------------------------------------------------------------
        // Camera
        // ---------------------------------------------------------------
        void BuildCamera()
        {
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.backgroundColor = new Color(0.1f, 0.1f, 0.12f);
            cam.transform.position = new Vector3(0f, 0f, -10f);

            var follow = camGo.AddComponent<ArgosCameraFollow>();
            follow.target = player.transform;
            follow.minBounds = roomMin;
            follow.maxBounds = roomMax;
        }

        // ---------------------------------------------------------------
        // Canvas + UI (joystick visual, quest box, banners, panels)
        // ---------------------------------------------------------------
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

            BuildJoystickVisual(canvasGo.transform);
            BuildQuestBox(canvasGo.transform);
            BuildGadgetBanner(canvasGo.transform);
            BuildInteractPrompt(canvasGo.transform);
            BuildFadeOverlay(canvasGo.transform);

            // UIManager singleton'ı Canvas üstüne ekleyelim (sahne-scoped).
            uiManager = canvasGo.AddComponent<UIManager>();
        }

        void BuildJoystickVisual(Transform parent)
        {
            var area = new GameObject("JoystickArea");
            area.transform.SetParent(parent, false);
            var areaRt = area.AddComponent<RectTransform>();
            areaRt.anchorMin = new Vector2(0f, 0f);
            areaRt.anchorMax = new Vector2(0f, 0f);
            areaRt.pivot = new Vector2(0.5f, 0.5f);
            areaRt.anchoredPosition = new Vector2(120f, 120f);
            areaRt.sizeDelta = new Vector2(140f, 140f);
            var areaImg = area.AddComponent<Image>();
            areaImg.color = new Color(1f, 1f, 1f, 0.18f);

            var knob = new GameObject("JoystickKnob");
            knob.transform.SetParent(area.transform, false);
            var knobRt = knob.AddComponent<RectTransform>();
            knobRt.anchorMin = new Vector2(0.5f, 0.5f);
            knobRt.anchorMax = new Vector2(0.5f, 0.5f);
            knobRt.pivot = new Vector2(0.5f, 0.5f);
            knobRt.anchoredPosition = Vector2.zero;
            knobRt.sizeDelta = new Vector2(64f, 64f);
            var knobImg = knob.AddComponent<Image>();
            knobImg.color = new Color(1f, 1f, 1f, 0.55f);

            var joy = area.AddComponent<WasdJoystick>();
            joy.SetKnob(knobRt);
            joy.SetRadius(38f);
        }

        void BuildQuestBox(Transform parent)
        {
            var box = new GameObject("QuestBox");
            box.transform.SetParent(parent, false);
            var rt = box.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-20f, -20f);
            rt.sizeDelta = new Vector2(320f, 80f);
            var bg = box.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.55f);

            var label = new GameObject("Text");
            label.transform.SetParent(box.transform, false);
            var lrt = label.AddComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = new Vector2(10f, 10f);
            lrt.offsetMax = new Vector2(-10f, -10f);
            var tmp = label.AddComponent<TextMeshProUGUI>();
            tmp.text = "Aktif Görev";
            tmp.fontSize = 18f;
            tmp.color = Color.white;

            var qb = box.AddComponent<QuestBox>();
            qb.SetTextField(tmp);
        }

        void BuildGadgetBanner(Transform parent)
        {
            var root = new GameObject("GadgetWarning");
            root.transform.SetParent(parent, false);
            var rt = root.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -30f);
            rt.sizeDelta = new Vector2(420f, 60f);
            var bg = root.AddComponent<Image>();
            bg.color = new Color(0.9f, 0.7f, 0.1f, 0.85f);

            var label = new GameObject("Text");
            label.transform.SetParent(root.transform, false);
            var lrt = label.AddComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = new Vector2(10f, 5f);
            lrt.offsetMax = new Vector2(-10f, -5f);
            var tmp = label.AddComponent<TextMeshProUGUI>();
            tmp.text = "";
            tmp.fontSize = 22f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.black;

            var banner = root.AddComponent<GadgetWarningBanner>();
            banner.Setup(root, tmp);
            root.SetActive(false);
        }

        void BuildInteractPrompt(Transform parent)
        {
            var root = new GameObject("InteractPrompt");
            root.transform.SetParent(parent, false);
            var rt = root.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, 30f);
            rt.sizeDelta = new Vector2(280f, 50f);
            var bg = root.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.7f);

            var label = new GameObject("Text");
            label.transform.SetParent(root.transform, false);
            var lrt = label.AddComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = new Vector2(10f, 5f);
            lrt.offsetMax = new Vector2(-10f, -5f);
            var tmp = label.AddComponent<TextMeshProUGUI>();
            tmp.text = "";
            tmp.fontSize = 20f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            var prompt = root.AddComponent<InteractPrompt>();
            prompt.Setup(root, tmp);
            root.SetActive(false);
        }

        void BuildFadeOverlay(Transform parent)
        {
            var go = new GameObject("FadeOverlay");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var img = go.AddComponent<Image>();
            img.color = Color.black;
            var cg = go.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.blocksRaycasts = false;
            PortalManager.Instance?.SetFadeOverlay(cg);
        }

        // ---------------------------------------------------------------
        // Wiring
        // ---------------------------------------------------------------
        void WireUp()
        {
            if (uiManager != null)
            {
                uiManager.questBox = canvas.GetComponentInChildren<QuestBox>(true);
                uiManager.gadgetWarning = canvas.GetComponentInChildren<GadgetWarningBanner>(true);
                uiManager.interactPrompt = canvas.GetComponentInChildren<InteractPrompt>(true);
            }
            if (QuestManager.Instance != null && uiManager != null && uiManager.questBox != null)
                QuestManager.Instance.SetQuestBox(uiManager.questBox);
        }

        // ---------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------
        static Sprite cachedWhite;
        static Sprite MakeWhiteSprite()
        {
            if (cachedWhite != null) return cachedWhite;
            var tex = Texture2D.whiteTexture;
            cachedWhite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), tex.width);
            return cachedWhite;
        }
    }
}
