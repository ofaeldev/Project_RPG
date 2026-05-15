using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace RPGProject.Inputs
{
    /// <summary>
    /// Captura o input bruto do jogador local pelo Input System.
    /// Nao interpreta regras de gameplay; apenas expoe estados e acoes deste frame.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-100)]
    public sealed class PlayerInputReader : MonoBehaviour
    {
        [Header("Input Actions")]
        [Tooltip("Referencia para o asset de acoes de entrada do jogador.")]
        [SerializeField]
        private InputActionAsset actionsAsset;

        [Header("Gamepad")]
        [Tooltip("Valor minimo do analogico para ser considerado movimento.")]
        [SerializeField]
        [Range(0f, 1f)]
        private float gamepadDeadZone = 0.15f;

        private readonly PlayerInputState state = new();

        public event Action<Vector2> MovementChanged;
        public event Action<Vector2, Vector2> PointerChanged;
        public event Action<Vector2> ClickMovePressedEvent;
        public event Action<Vector2> RightClickActionPressedEvent;

        public PlayerInputState State => state;
        public Vector2 Movement => state.Movement;
        public Vector2 PointerScreenPosition => state.PointerScreenPosition;
        public Vector2 PointerWorldPosition => state.PointerWorldPosition;
        public bool ClickMovePressed { get; private set; }
        public bool RightClickActionPressed { get; private set; }
        public bool IsMoving => state.IsMoving;

        private InputAction moveAction;
        private InputAction pointerPositionAction;
        private InputAction clickMoveAction;
        private InputAction rightClickAction;
        private InputActionMap playerMap;
        private InputActionMap uiMap;

        private void OnEnable()
        {
            ResolveActionsAsset();
            if (actionsAsset == null)
            {
                Debug.LogError("PlayerInputReader precisa de um InputActionAsset atribuido.", this);
                return;
            }

            playerMap = actionsAsset.FindActionMap("Player", throwIfNotFound: true);
            moveAction = playerMap.FindAction("Move", throwIfNotFound: true);
            pointerPositionAction = playerMap.FindAction("PointerPosition", throwIfNotFound: true);
            clickMoveAction = playerMap.FindAction("ClickMove", throwIfNotFound: true);
            playerMap.Enable();

            uiMap = actionsAsset.FindActionMap("UI", throwIfNotFound: true);
            rightClickAction = uiMap.FindAction("RightClick", throwIfNotFound: true);
            uiMap.Enable();
        }

        private void OnDisable()
        {
            playerMap?.Disable();
            uiMap?.Disable();
            ResetInput();
        }

        private void Update()
        {
            ReadMovement();
            ReadPointer();
            ClickMovePressed = clickMoveAction != null && clickMoveAction.WasPerformedThisFrame();
            RightClickActionPressed = rightClickAction != null && rightClickAction.WasPerformedThisFrame();

            if (ClickMovePressed)
            {
                ClickMovePressedEvent?.Invoke(state.PointerWorldPosition);
            }

            if (RightClickActionPressed)
            {
                RightClickActionPressedEvent?.Invoke(state.PointerWorldPosition);
            }
        }

        public void ResetInput()
        {
            state.Reset();
            ClickMovePressed = false;
            RightClickActionPressed = false;
            MovementChanged?.Invoke(Vector2.zero);
            PointerChanged?.Invoke(Vector2.zero, Vector2.zero);
        }

        private void ResolveActionsAsset()
        {
#if UNITY_EDITOR
            if (actionsAsset == null)
            {
                actionsAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");
            }
#endif
        }

        private void ReadMovement()
        {
            if (moveAction == null)
            {
                if (state.SetMovement(Vector2.zero))
                {
                    MovementChanged?.Invoke(state.Movement);
                }

                return;
            }

            Vector2 rawMovement = moveAction.ReadValue<Vector2>();
            Vector2 nextMovement = rawMovement.sqrMagnitude >= gamepadDeadZone * gamepadDeadZone
                ? Vector2.ClampMagnitude(rawMovement, 1f)
                : Vector2.zero;
            if (state.SetMovement(nextMovement))
            {
                MovementChanged?.Invoke(state.Movement);
            }
        }

        private void ReadPointer()
        {
            if (pointerPositionAction == null)
            {
                if (state.SetPointer(Vector2.zero, Vector2.zero))
                {
                    PointerChanged?.Invoke(state.PointerScreenPosition, state.PointerWorldPosition);
                }

                return;
            }

            Vector2 screenPosition = pointerPositionAction.ReadValue<Vector2>();
            Vector2 worldPosition = Camera.main != null
                ? Camera.main.ScreenToWorldPoint(screenPosition)
                : Vector2.zero;
            if (state.SetPointer(screenPosition, worldPosition))
            {
                PointerChanged?.Invoke(state.PointerScreenPosition, state.PointerWorldPosition);
            }
        }
    }
}
