using UnityEngine;

namespace Argos.AI
{
    public enum AIProvider
    {
        OpenAI,
        Anthropic
    }

    [CreateAssetMenu(fileName = "AIConfig", menuName = "ARGOS/AI Config", order = 3)]
    public class AIConfig : ScriptableObject
    {
        public AIProvider provider = AIProvider.OpenAI;
        public string apiKey = "";
        public string modelName = "gpt-4o-mini";
        public int maxTokens = 256;
        [Range(0f, 2f)] public float temperature = 0.7f;
    }
}
