using System;
using App;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Input
{
    public class InputManager : AppModule
    {
        public event Action<Vector2> OnPlayerMove;
        public event Action OnPlayerUse;
        public event Action OnPlayerInteract;
        public event Action<bool> OnPlayerShift;
        public event Action<int> OnPlayerNumKeys;

        public void PlayerMove(InputAction.CallbackContext context)
        {
            OnPlayerMove?.Invoke(context.ReadValue<Vector2>());
        }
        
        public void PlayerUse(InputAction.CallbackContext context)
        {
            if (context.performed) OnPlayerUse?.Invoke();
        }
        
        public void PlayerInteract(InputAction.CallbackContext context)
        {
            if (context.performed) OnPlayerInteract?.Invoke();
        }
        
        public void PlayerShift(InputAction.CallbackContext context)
        {
            if (context.performed) OnPlayerShift?.Invoke(true);
            if (context.canceled) OnPlayerShift?.Invoke(false);
        }

        public void PlayerNumKeys(InputAction.CallbackContext context)
        {
            if (context.performed) OnPlayerNumKeys?.Invoke((int) context.ReadValue<float>());
        }
    }
}
