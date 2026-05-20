using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Argos.AI;
using Argos.Scenarios;
using Argos.UI;

namespace Argos.Game
{
    /// <summary>
    /// NewspaperScene için runtime kurulum. Sadece kamera + Canvas + NewspaperUI.
    /// Manager'lar ManagersInitializer ile zaten kurulu.
    /// </summary>
    public class NewspaperSceneBootstrap : MonoBehaviour
    {
        [Header("Build flags")]
        public bool buildOnStart = true;
        public bool isEndingScene = false;
        public bool isCorrect = false;

        [Header("Optional refs")]
        public AIConfig aiConfig;
        public ScenarioData scenario;

        void Start()
        {
            if (!buildOnStart) return;

            if (aiConfig != null && AIManager.Instance != null) AIManager.Instance.SetConfig(aiConfig);
            if (scenario != null && GameManager.Instance != null && GameManager.Instance.CurrentScenario == null)
                GameManager.Instance.SetScenario(scenario);

            // Sorgu sonrası NewspaperScene'e yönlendirildiysek ending modunda aç,
            // doğru/yanlış ayrımını GameManager.CaseSolved belirler.
            if (GameManager.PendingNewspaperEnding)
            {
                isEndingScene = true;
                isCorrect = GameManager.Instance != null && GameManager.Instance.CaseSolved;
                GameManager.PendingNewspaperEnding = false;
            }

            BuildCamera();
            var paper = BuildCanvas();
            paper.Show(isEndingScene, isCorrect);
        }

        void BuildCamera()
        {
            if (Camera.main != null) return;
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.backgroundColor = new Color(0.06f, 0.05f, 0.05f);
            cam.transform.position = new Vector3(0f, 0f, -10f);
        }

        NewspaperUI BuildCanvas()
        {
            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();

            if (FindAnyObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }

            var paper = UIBuilders.BuildNewspaperPanel(canvasGo.transform);
            var ui = canvasGo.AddComponent<UIManager>();
            ui.newspaper = paper;
            return paper;
        }
    }
}
