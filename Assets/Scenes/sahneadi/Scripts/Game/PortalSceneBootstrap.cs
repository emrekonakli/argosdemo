using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Argos.NPC;
using Argos.Player;
using Argos.Portal;
using Argos.UI;

namespace Argos.Game
{
    /// <summary>
    /// PortalScene için runtime kurulum. MuseumScene'in çok daha minimal versiyonu:
    /// zemin/duvar placeholder + ExitTrigger + bir NPC + standart UI.
    /// </summary>
    public class PortalSceneBootstrap : MonoBehaviour
    {
        [Header("Build flags")]
        public bool buildOnStart = true;

        [Header("Optional refs")]
        public NPCData portalNPC;

        [Header("Room layout")]
        public Vector2 roomMin = new Vector2(-10f, -7.5f);
        public Vector2 roomMax = new Vector2(10f, 7.5f);

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
            BuildPlaceholderRoom();
            BuildPlayer();
            BuildCamera();
            BuildCanvas();
            BuildExitTrigger();
            BuildNPC();
            WireUp();
        }

        // ---------------------------------------------------------------
        // Placeholder room (Tilemap yerine koyu renkli sprite duvar/zemin)
        // ---------------------------------------------------------------
        void BuildPlaceholderRoom()
        {
            var root = new GameObject("PortalRoom");
            float thickness = 0.5f;
            Color wallColor = new Color(0.18f, 0.18f, 0.22f);
            Color floorColor = new Color(0.32f, 0.3f, 0.28f);

            float w = roomMax.x - roomMin.x;
            float h = roomMax.y - roomMin.y;
            Vector2 center = (roomMin + roomMax) * 0.5f;

            MakeWall(root.transform, "Wall_Top",    new Vector2(center.x, roomMax.y + thickness * 0.5f), new Vector2(w + thickness * 2f, thickness), wallColor);
            MakeWall(root.transform, "Wall_Bottom", new Vector2(center.x, roomMin.y - thickness * 0.5f), new Vector2(w + thickness * 2f, thickness), wallColor);
            MakeWall(root.transform, "Wall_Left",   new Vector2(roomMin.x - thickness * 0.5f, center.y), new Vector2(thickness, h), wallColor);
            MakeWall(root.transform, "Wall_Right",  new Vector2(roomMax.x + thickness * 0.5f, center.y), new Vector2(thickness, h), wallColor);

            var floor = new GameObject("PortalFloor");
            floor.transform.SetParent(root.transform);
            var fsr = floor.AddComponent<SpriteRenderer>();
            fsr.sprite = MakeWhiteSprite();
            fsr.color = floorColor;
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
            go.AddComponent<BoxCollider2D>();
        }

        // ---------------------------------------------------------------
        // Player — sahne reload sonrası DontDestroyOnLoad'lu Player kalmış olabilir.
        // ---------------------------------------------------------------
        void BuildPlayer()
        {
            var existing = GameObject.FindGameObjectWithTag("Player");
            if (existing != null)
            {
                player = existing;
                player.transform.position = new Vector3(roomMin.x + 2f, 0f, 0f);
                return;
            }

            player = new GameObject("Player");
            player.tag = "Player";
            player.transform.position = new Vector3(roomMin.x + 2f, 0f, 0f);
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
            var gadget = player.AddComponent<PlayerGadget>();
            var audio = player.AddComponent<AudioSource>();
            audio.playOnAwake = false;
            gadget.SetCameraSound(audio);
        }

        void BuildCamera()
        {
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.backgroundColor = new Color(0.05f, 0.05f, 0.07f);
            cam.transform.position = new Vector3(0f, 0f, -10f);

            var follow = camGo.AddComponent<ArgosCameraFollow>();
            follow.target = player.transform;
            follow.minBounds = roomMin;
            follow.maxBounds = roomMax;
        }

        // ---------------------------------------------------------------
        // Canvas — Faz 7 için sadece InteractPrompt + QuestBox + GadgetWarning.
        // Diğer panel'ler (artifact, journal, internal voice) PortalScene'de
        // gerekmiyor; UIManager null referansları no-op davranır.
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

            BuildInteractPromptUI(canvasGo.transform);
            BuildQuestBoxUI(canvasGo.transform);
            UIBuilders.BuildInterrogationPanel(canvasGo.transform);

            uiManager = canvasGo.AddComponent<UIManager>();
        }

        void BuildInteractPromptUI(Transform parent)
        {
            var root = new GameObject("InteractPrompt");
            root.transform.SetParent(parent, false);
            var rt = root.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, 30f);
            rt.sizeDelta = new Vector2(280f, 50f);
            root.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.7f);

            var label = new GameObject("Text");
            label.transform.SetParent(root.transform, false);
            var lrt = label.AddComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = new Vector2(10f, 5f);
            lrt.offsetMax = new Vector2(-10f, -5f);
            var tmp = label.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 20;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            var prompt = root.AddComponent<InteractPrompt>();
            prompt.Setup(root, tmp);
            root.SetActive(false);
        }

        void BuildQuestBoxUI(Transform parent)
        {
            var box = new GameObject("QuestBox");
            box.transform.SetParent(parent, false);
            var rt = box.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-20f, -20f);
            rt.sizeDelta = new Vector2(320f, 80f);
            box.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);

            var label = new GameObject("Text");
            label.transform.SetParent(box.transform, false);
            var lrt = label.AddComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = new Vector2(10f, 10f);
            lrt.offsetMax = new Vector2(-10f, -10f);
            var tmp = label.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 18f;
            tmp.color = Color.white;

            var qb = box.AddComponent<QuestBox>();
            qb.SetTextField(tmp);
        }

        // ---------------------------------------------------------------
        // Exit trigger (kırmızı X)
        // ---------------------------------------------------------------
        void BuildExitTrigger()
        {
            var go = new GameObject("PortalExit");
            go.transform.position = new Vector3(roomMax.x - 2f, 0f, 0f);
            go.transform.localScale = new Vector3(1f, 1f, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = MakeWhiteSprite();
            sr.color = new Color(0.85f, 0.15f, 0.15f);
            sr.sortingOrder = 8;

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(2f, 2f);

            go.AddComponent<PortalExitTrigger>();
        }

        // ---------------------------------------------------------------
        // NPC (Faz 9 sorgu deneyi için)
        // ---------------------------------------------------------------
        void BuildNPC()
        {
            if (portalNPC == null) return;

            var go = new GameObject("NPC_" + portalNPC.npcName);
            go.transform.position = new Vector3(0f, -2f, 0f);
            go.transform.localScale = new Vector3(0.6f, 0.6f, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = MakeWhiteSprite();
            sr.color = new Color(0.85f, 0.35f, 0.35f);
            sr.sortingOrder = 10;

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(3f, 3f);

            var npc = go.AddComponent<NPCInteractable>();
            npc.data = portalNPC;
        }

        void WireUp()
        {
            if (uiManager == null) return;
            uiManager.questBox = canvas.GetComponentInChildren<QuestBox>(true);
            uiManager.interactPrompt = canvas.GetComponentInChildren<InteractPrompt>(true);
            uiManager.interrogation = canvas.GetComponentInChildren<InterrogationUI>(true);
            if (QuestManager.Instance != null && uiManager.questBox != null)
                QuestManager.Instance.SetQuestBox(uiManager.questBox);
        }

        // ---------------------------------------------------------------
        // Helper (MuseumSceneBootstrap'in cache'iyle aynı pattern)
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
