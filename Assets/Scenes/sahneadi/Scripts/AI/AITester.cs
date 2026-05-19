using System.Collections.Generic;
using UnityEngine;

namespace Argos.AI
{
    /// <summary>
    /// Geçici test bileşeni — F tuşuna basıldığında AIManager.SendChat'i tetikler
    /// ve cevabı Console'a düşer. Faz 8 denetimi için. Faz 9'da
    /// InterrogationUI gerçek test akışına geçince bu kaldırılabilir.
    /// </summary>
    public class AITester : MonoBehaviour
    {
        [SerializeField] private KeyCode triggerKey = KeyCode.F;
        [TextArea(2, 5)] public string systemPrompt = "Sen yaşlı bir Frigya kâhinisin. Kısa, gizemli cümleler kurarsın.";
        public string userMessage = "Bana ne söyleyebilirsin?";

        async void Update()
        {
            if (!Input.GetKeyDown(triggerKey)) return;
            if (AIManager.Instance == null)
            {
                Debug.LogWarning("[AITester] AIManager.Instance null.");
                return;
            }
            Debug.Log($"[AITester] '{userMessage}' gönderiliyor...");
            string reply = await AIManager.Instance.SendChat(systemPrompt, new List<ChatMessage>(), userMessage);
            Debug.Log($"[AITester] Cevap: {reply}");
        }
    }
}
