using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.EventSystems;

namespace InputModules
{
    [AddComponentMenu("Event/Vita Input Module")]
    public class VitaInputModule : PointerInputModule
    {
        private float m_PrevActionTime;
        Vector2 m_LastMoveVector;
        int m_ConsecutiveMoveCount = 0;
        private bool m_IsRepeating = false;
        private Vector2 m_LastMousePosition;
        private Vector2 m_MousePosition;

        protected VitaInputModule()
        {
        }

        [Tooltip("Enables touch input.")]
        public bool m_EnableTouch = true;

        [Tooltip("Enables axis and button input.")]
        public bool m_EnableAxesAndButtons = true;

        [Tooltip("When ticked causes the current selection to be cleared if a touch has no selectable target.")]
        public bool m_DeselectIfNoTouchSelection = false;

        [Tooltip("Input manager horizontal axis to use for UI navigation.")]
        public string m_HorizontalAxis = "Horizontal";

        [Tooltip("Input manager vertical axis to use for UI navigation.")]
        public string m_VerticalAxis = "Vertical";

        [Tooltip("Input manager button to use for submit.")]
        public string m_SubmitButton = "Submit";

        [Tooltip("Input manager button to use for cancel.")]
        public string m_CancelButton = "Cancel";

        [Tooltip("Repeat speed in actions per second.")]
        public float m_RepeatSpeed = 4;

        [Tooltip("Repeat delay in seconds.")]
        public float m_RepeatDelay = 0.25f;

        public override void UpdateModule()
        {
            m_LastMousePosition = m_MousePosition;
            m_MousePosition = Input.mousePosition;
        }

        public override bool IsModuleSupported()
        {
            return true;
        }

        private bool UseFakeInput()
        {
    #if UNITY_PSP2 && !UNITY_EDITOR
            return false;
    #else
            return !Input.touchSupported;
    #endif
        }

        public override bool ShouldActivateModule()
        {
            if (!base.ShouldActivateModule())
                return false;

            if (UseFakeInput())
            {
                if (Input.GetMouseButtonDown(0))
                {
                    return true;
                }

                if ((m_MousePosition - m_LastMousePosition).sqrMagnitude > 0.0f)
                {
                    return true;
                }
            }

            var shouldActivate = false;
            Input.GetButtonDown(m_SubmitButton);
            shouldActivate |= Input.GetButtonDown(m_CancelButton);
            shouldActivate |= !Mathf.Approximately(Input.GetAxisRaw(m_HorizontalAxis), 0.0f);
            shouldActivate |= !Mathf.Approximately(Input.GetAxisRaw(m_VerticalAxis), 0.0f);
            return shouldActivate;
        }

        public override void ActivateModule()
        {
            base.ActivateModule();
            m_MousePosition = Input.mousePosition;
            m_LastMousePosition = Input.mousePosition;

            var toSelect = eventSystem.currentSelectedGameObject;
            if (toSelect == null)
                toSelect = eventSystem.firstSelectedGameObject;

            eventSystem.SetSelectedGameObject(toSelect, GetBaseEventData());
        }

        public override void DeactivateModule()
        {
            base.DeactivateModule();
            ClearSelection();
        }

        private void ProcessTouchEvents()
        {
            for (int i = 0; i < Input.touchCount; ++i)
            {
                Touch input = Input.GetTouch(i);

                bool released;
                bool pressed;
                var pointer = GetTouchPointerEventData(input, out pressed, out released);

                ProcessTouchPress(pointer, pressed, released);

                if (!released)
                {
                    ProcessMove(pointer);
                    ProcessDrag(pointer);
                }
                else
                    RemovePointerData(pointer);
            }
        }

        private void ProcessTouchPress(PointerEventData pointerEvent, bool pressed, bool released)
        {
            var currentOverGo = pointerEvent.pointerCurrentRaycast.gameObject;

            // PointerDown notification
            if (pressed)
            {
                pointerEvent.eligibleForClick = true;
                pointerEvent.delta = Vector2.zero;
                pointerEvent.dragging = false;
                pointerEvent.useDragThreshold = true;
                pointerEvent.pressPosition = pointerEvent.position;
                pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast;

                DeselectIfSelectionChangedAndValid(currentOverGo, pointerEvent, !m_DeselectIfNoTouchSelection);

                if (pointerEvent.pointerEnter != currentOverGo)
                {
                    // send a pointer enter to the touched element if it isn't the one to select...
                    HandlePointerExitAndEnter(pointerEvent, currentOverGo);
                    pointerEvent.pointerEnter = currentOverGo;
                }

                // search for the control that will receive the press
                // if we can't find a press handler set the press
                // handler to be what would receive a click.
                var newPressed = ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.pointerDownHandler);

                // didnt find a press handler... search for a click handler
                if (newPressed == null)
                    newPressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

                float time = Time.unscaledTime;

                if (newPressed == pointerEvent.lastPress)
                {
                    var diffTime = time - pointerEvent.clickTime;
                    if (diffTime < 0.3f)
                        ++pointerEvent.clickCount;
                    else
                        pointerEvent.clickCount = 1;

                    pointerEvent.clickTime = time;
                }
                else
                {
                    pointerEvent.clickCount = 1;
                }

                pointerEvent.pointerPress = newPressed;
                pointerEvent.rawPointerPress = currentOverGo;

                pointerEvent.clickTime = time;

                // Save the drag handler as well
                pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(currentOverGo);

                if (pointerEvent.pointerDrag != null)
                    ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.initializePotentialDrag);
            }

            // PointerUp notification
            if (released)
            {
                // Debug.Log("Executing pressup on: " + pointer.pointerPress);
                ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);

                // Debug.Log("KeyCode: " + pointer.eventData.keyCode);

                // see if we mouse up on the same element that we clicked on...
                var pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

                // PointerClick and Drop events
                if (pointerEvent.pointerPress == pointerUpHandler && pointerEvent.eligibleForClick)
                {
                    ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerClickHandler);
                }
                else if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
                {
                    ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.dropHandler);
                }

                pointerEvent.eligibleForClick = false;
                pointerEvent.pointerPress = null;
                pointerEvent.rawPointerPress = null;

                if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
                    ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);

                pointerEvent.dragging = false;
                pointerEvent.pointerDrag = null;

                if (pointerEvent.pointerDrag != null)
                    ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);

                pointerEvent.pointerDrag = null;

                // send exit events as we need to simulate this on touch up on touch device
                ExecuteEvents.ExecuteHierarchy(pointerEvent.pointerEnter, pointerEvent, ExecuteEvents.pointerExitHandler);
                pointerEvent.pointerEnter = null;
            }
        }

        private void FakeTouches()
        {
            var pointerData = GetMousePointerEventData(0);

            var leftPressData = pointerData.GetButtonState(PointerEventData.InputButton.Left).eventData;

            // fake touches... on press clear delta
            if (leftPressData.PressedThisFrame())
                leftPressData.buttonData.delta = Vector2.zero;

            ProcessTouchPress(leftPressData.buttonData, leftPressData.PressedThisFrame(), leftPressData.ReleasedThisFrame());

            // only process move if we are pressed...
            if (Input.GetMouseButton(0))
            {
                ProcessMove(leftPressData.buttonData);
                ProcessDrag(leftPressData.buttonData);
            }
        }

        public override void Process()
        {
            bool usedEvent = SendUpdateEventToSelectedObject();

            if (m_EnableAxesAndButtons)
            {
                if (eventSystem.sendNavigationEvents)
                {
                    if (!usedEvent)
                        usedEvent |= SendMoveEventToSelectedObject();

                    if (!usedEvent)
                        SendSubmitEventToSelectedObject();
                }
            }

            if (m_EnableTouch)
            {
                if (UseFakeInput())
                {
                    FakeTouches();
                }
                else
                {
                    ProcessTouchEvents();
                }
            }
        }

        protected bool SendSubmitEventToSelectedObject()
        {
            if (eventSystem.currentSelectedGameObject == null)
                return false;

            var data = GetBaseEventData();
            if (Input.GetButtonDown(m_SubmitButton))
                ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.submitHandler);

            if (Input.GetButtonDown(m_CancelButton))
                ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.cancelHandler);
            return data.used;
        }

        private Vector2 GetRawMoveVector()
        {
            Vector2 move = Vector2.zero;
            move.x = Input.GetAxisRaw(m_HorizontalAxis);
            move.y = Input.GetAxisRaw(m_VerticalAxis);

            if (Input.GetButtonDown(m_HorizontalAxis))
            {
                if (move.x < 0)
                    move.x = -1f;
                if (move.x > 0)
                    move.x = 1f;
            }
            if (Input.GetButtonDown(m_VerticalAxis))
            {
                if (move.y < 0)
                    move.y = -1f;
                if (move.y > 0)
                    move.y = 1f;
            }
            return move;
        }

        protected bool SendMoveEventToSelectedObject()
        {
            float time = Time.unscaledTime;

            Vector2 movement = GetRawMoveVector();
            if (Mathf.Approximately(movement.x, 0f) && Mathf.Approximately(movement.y, 0f))
            {
                m_PrevActionTime = time;
                m_LastMoveVector = movement;
                m_ConsecutiveMoveCount = 0;
                m_IsRepeating = false;
                return false;
            }

            bool similarDir = (Vector2.Dot(movement, m_LastMoveVector) > 0);

            if (m_IsRepeating)
            {
                // If user pressed key again, always allow event
                bool allow = Input.GetButtonDown(m_HorizontalAxis) || Input.GetButtonDown(m_VerticalAxis);

                if (!allow)
                {
                    // Otherwise, user held down key or axis.
                    // If direction didn't change at least 90 degrees, wait for delay before allowing consecutive event.
                    if (similarDir && m_ConsecutiveMoveCount == 1)
                        allow = (time > m_PrevActionTime + m_RepeatDelay);
                    // If direction changed at least 90 degree, or we already had the delay, repeat at repeat rate.
                    else
                        allow = (time > m_PrevActionTime + 1f / m_RepeatSpeed);
                }
                if (!allow)
                    return false;
            }

            var axisEventData = GetAxisEventData(movement.x, movement.y, 0.0f);
            ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, axisEventData, ExecuteEvents.moveHandler);
            if (!similarDir)
            {
                m_ConsecutiveMoveCount = 0;
            }
            m_ConsecutiveMoveCount++;
            m_PrevActionTime = time;
            m_LastMoveVector = movement;
            m_IsRepeating = true;
            return axisEventData.used;
        }

        protected bool SendUpdateEventToSelectedObject()
        {
            if (eventSystem.currentSelectedGameObject == null)
                return false;

            var data = GetBaseEventData();
            ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.updateSelectedHandler);
            return data.used;
        }

        protected void DeselectIfSelectionChangedAndValid(GameObject currentOverGo, BaseEventData pointerEvent, bool ignoreNullSelection)
        {
            // Selection tracking

            // If we have clicked something new, deselect the old thing
            // leave 'selection handling' up to the press event though.

            var selectHandlerGO = ExecuteEvents.GetEventHandler<ISelectHandler>(currentOverGo);

            if (ignoreNullSelection && selectHandlerGO == null)
            {
                return;
            }

            if (selectHandlerGO != eventSystem.currentSelectedGameObject)
            {
                eventSystem.SetSelectedGameObject(null, pointerEvent);
            }
        }
    }
}
