using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using ETouch = UnityEngine.InputSystem.EnhancedTouch;

public class TouchController : Singleton<TouchController>
{
    private Finger _currentFinger;
    private Camera _mainCamera => Camera.main;

    public static Action OnFingerDownAction;
    public static Action OnFingerMoveAction;
    public static Action OnFingerUpAction;

    private void OnEnable()
    {
        TouchSimulation.Enable();
        EnhancedTouchSupport.Enable();

        ETouch.Touch.onFingerDown += OnFingerDown;
        ETouch.Touch.onFingerMove += OnFingerMove;
        ETouch.Touch.onFingerUp += OnFingerUp;
    }

    private void OnDisable()
    {
        TouchSimulation.Disable();
        ETouch.Touch.onFingerDown -= OnFingerDown;
        ETouch.Touch.onFingerMove -= OnFingerMove;
        ETouch.Touch.onFingerUp -= OnFingerUp;
    }

    private void OnFingerDown(Finger finger)
    {
        _currentFinger = finger;
        OnFingerDownAction?.Invoke();
    }

    private void OnFingerMove(Finger finger)
    {
        // Handle finger move event
        OnFingerMoveAction?.Invoke();
    }

    private void OnFingerUp(Finger finger)
    {
        if (_currentFinger == null || _currentFinger != finger)
            return;
        else
        {
            _currentFinger = null;
        }
        OnFingerUpAction?.Invoke();
    }

    /// <summary>
    /// Get the component of type T under the current finger's position using a raycast.
    /// </summary>
    public T GetObjectThroughRaycast<T>() where T : MonoBehaviour
    {
        if (_currentFinger == null)
        {
            Debug.LogWarning("No finger is currently active.");
            return null;
        }

        Ray ray = _mainCamera.ScreenPointToRay(_currentFinger.screenPosition);
        Debug.DrawRay(ray.origin, ray.direction * 50f, Color.red, 2f);

        RaycastHit[] hits = new RaycastHit[200];
        int hitCount = Physics.RaycastNonAlloc(ray, hits, 100, LayerMask.GetMask("Interactable"));

        if (hitCount > 0)
        {
            foreach (var hit in hits.Take(hitCount))
            {
                if (hit.transform.TryGetComponent<T>(out var component))
                {
                    return component;
                }
            }
        }
        return null;
    }
}
