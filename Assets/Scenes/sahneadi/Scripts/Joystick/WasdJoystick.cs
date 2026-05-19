using UnityEngine;
using UnityEngine.UI;

namespace Argos.Joystick
{
    public class WasdJoystick : MonoBehaviour
    {
        [SerializeField] private RectTransform knob;
        [SerializeField] private float radius = 40f;
        [SerializeField] private float lerpSpeed = 12f;

        private Vector2 target;

        public void SetKnob(RectTransform knobRect) => knob = knobRect;
        public void SetRadius(float r) => radius = r;

        void Update()
        {
            if (knob == null) return;
            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");
            Vector2 input = new Vector2(x, y);
            if (input.sqrMagnitude > 1f) input.Normalize();
            target = input * radius;
            knob.anchoredPosition = Vector2.Lerp(knob.anchoredPosition, target, Time.deltaTime * lerpSpeed);
        }
    }
}
