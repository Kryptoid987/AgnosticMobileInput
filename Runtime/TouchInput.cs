#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.Utilities;

namespace MobileInput_NewInputSystem
{
    public struct TouchInput
    {
        public bool TouchContact;
        public int TouchId;
        public Vector2 Position;
        public Vector2 DeltaPosition;
        public UnityEngine.InputSystem.TouchPhase TouchPhase;
        public bool IsOverUI; //This will not show in the input maps, just for sending params to user
    }

#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class TouchInputComposite : InputBindingComposite<TouchInput>
    {
        [InputControl(layout = "Button")]
        public int contact;

        [InputControl(layout = "Vector2")]
        public int position;

        [InputControl(layout = "DeltaPosition")]
        public int deltaPosition;

        [InputControl(layout = "IntegerControl")]
        public int inputId;

        [InputControl(layout = "TouchPhaseControl")]
        public int touchPhase;

        public override TouchInput ReadValue(ref InputBindingCompositeContext context)
        {
            var contact = context.ReadValueAsButton(this.contact);
            var touchID = context.ReadValue<int>(this.inputId);
            var touchPhase = context.ReadValueAsObject(this.touchPhase);
            var position = context.ReadValue<Vector2, Vector2MagnitudeComparer>(this.position);
            var deltaPosition = context.ReadValue<Vector2, Vector2MagnitudeComparer>(this.deltaPosition);

            return new TouchInput
            {
                TouchContact = contact,
                TouchId = touchID,
                Position = position,
                DeltaPosition = deltaPosition,
                TouchPhase = touchPhase != null ? (UnityEngine.InputSystem.TouchPhase)touchPhase : 0
                //IsOverUI set by MobileTouchManager
            };
        }

#if UNITY_EDITOR
        static TouchInputComposite()
        {
            Register();
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            InputSystem.RegisterBindingComposite<TouchInputComposite>();
        }
    }
}
