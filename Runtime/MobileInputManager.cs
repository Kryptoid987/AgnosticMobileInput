using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace MobileInput_NewInputSystem
{
    public class MobileInputManager : MonoBehaviour
    {
        MobileInputControls mobileInputControls;

        public static event Action<TouchInput> OnTouch;

        bool isMouseDragging = false;

        //Used to simulate stationary/moved on pc & Android since pc doesnt get touch phases, and currently android doesnt correctly update the phases
        bool wasMovedPhaseLastUpdate = false;

        private void Awake()
        {
            mobileInputControls = new MobileInputControls();
        }

        private void OnEnable()
        {
            mobileInputControls.Enable();
            mobileInputControls.TouchControls.Touch.actionMap.actionTriggered += TouchTriggered;
        }     

        private void TouchTriggered(InputAction.CallbackContext context)
        {
            var control = context.control;
            var device = control.device;

            var touchInput = context.ReadValue<TouchInput>();
            //If this ends up feeling inaccurate, it may have to be down in TouchInputComposite, but this event
            // should be triggered in the same frame, giving the same pointer location data
            touchInput.IsOverUI = IsPointerOverUIObject();

            //Set touchID to 0 if its the mouse, can only sim one touch
            if (device is Mouse)
                touchInput.TouchId = 0;

            //For now android doesnt handle touch phase and index correctly, so simulate it. Also simulate it for mouse input
#if UNITY_ANDROID || UNITY_EDITOR
            //SO FOR NOW FOR WHATEVER REASON, ON ANDROID DEVICE TOUCHPHASE IS ALWAYS = NONE, BUT 
            // IN SIMULATOR AND IOS IT WORKS PERFECTLY. ALSO TOUCH ID JUST ALWAYS INCREMENTS AND ISNT RELIABLE ATM
            //SO FOR THE TIME BEING WE WILL SIMULATE THE PHASES LIKE WITH MOUSE INPUT, BUT THIS IS A TEMP FIX!!
            if (touchInput.TouchContact && !isMouseDragging)
            {
                isMouseDragging = true;
                touchInput.TouchPhase = UnityEngine.InputSystem.TouchPhase.Began;
            }
            else if (touchInput.TouchContact && isMouseDragging)
            {
                if (touchInput.DeltaPosition.sqrMagnitude > 2.5f)
                {
                    touchInput.TouchPhase = UnityEngine.InputSystem.TouchPhase.Moved;
                    wasMovedPhaseLastUpdate = true;
                }
                else
                {
                    //Position delta is 0 every other update causing moved and stationary to flag every other frame.
                    //Using this bool avoids that and will just trigger moved appropriately unless the next update after that is
                    //also stationary.
                    if(!wasMovedPhaseLastUpdate)
                        touchInput.TouchPhase = UnityEngine.InputSystem.TouchPhase.Stationary;
                    else
                    {
                        wasMovedPhaseLastUpdate = false;
                        touchInput.TouchPhase = UnityEngine.InputSystem.TouchPhase.Moved;
                    }                  
                }
            }
            else if (!touchInput.TouchContact && isMouseDragging)
            {
                isMouseDragging = false;
                touchInput.TouchPhase = UnityEngine.InputSystem.TouchPhase.Ended;
            }
#endif
            //Quick note, iOS functions perfectly so just send out the event
            if (touchInput.TouchPhase != UnityEngine.InputSystem.TouchPhase.None)
            {
                //Debug.Log(Time.time + " " + (touchInput.TouchPhase) + "   " + touchInput.DeltaPosition.sqrMagnitude + " " + touchInput.TouchId + "  " + touchInput.Position);
                OnTouch?.Invoke(touchInput);
            }
        }

        public bool IsPointerOverUIObject()
        {
            if(EventSystem.current == null)
            {
                //Debug.Log("No event system found. Will only return false.");
                return false;
            }

            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            return results.Count > 0;
        }
        private void OnDisable()
        {
            if (mobileInputControls != null)
            {
                mobileInputControls.Disable();
                mobileInputControls.TouchControls.Touch.actionMap.actionTriggered -= TouchTriggered;
            }
        }
    }
}
