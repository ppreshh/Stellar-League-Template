using RebindableInputUI;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public Vector2 PitchYawValue { get { return m_MovementInputAction.ReadValue<Vector2>(); } }
    public Vector2 CameraLookValue { get { return m_CameraLookAction.ReadValue<Vector2>(); } }
    public bool IsRolling { get { return m_AirRollAction.ReadValue<float>() == 1f; } }
    public float ForwardBoostersValue { get { return m_ForwardBoostersInputAction.ReadValue<float>(); } }
    public float ReverseBoostersValue { get { return m_ReverseBoostersInputAction.ReadValue<float>(); } }

    private InputAction m_MovementInputAction;
    private InputAction m_ForwardBoostersInputAction;
    private InputAction m_ReverseBoostersInputAction;
    private InputAction m_BurstAction;
    private InputAction m_AirRollAction;
    private InputAction m_HyperspeedToggleAction;
    private InputAction m_CameraLookAction;
    private InputAction m_CameraLockToggleAction;
    private InputAction m_CameraYOffsetToggleAction;

    public event Action OnHyperSpeedPerformed;
    public event Action OnBurstPerformed;
    public event Action OnCameraLockToggle;
    public event Action OnCameraYOffsetToggle;

    public static InputManager Instance { private set; get; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        m_MovementInputAction = RebindingManager.Instance.InputActionMap.FindAction("Movement");
        m_ForwardBoostersInputAction = RebindingManager.Instance.InputActionMap.FindAction("ForwardBoosters");
        m_ReverseBoostersInputAction = RebindingManager.Instance.InputActionMap.FindAction("ReverseBoosters");
        m_BurstAction = RebindingManager.Instance.InputActionMap.FindAction("Burst");
        m_AirRollAction = RebindingManager.Instance.InputActionMap.FindAction("AirRoll");
        m_HyperspeedToggleAction = RebindingManager.Instance.InputActionMap.FindAction("HyperspeedToggle");
        m_CameraLookAction = RebindingManager.Instance.InputActionMap.FindAction("CameraLook");
        m_CameraLockToggleAction = RebindingManager.Instance.InputActionMap.FindAction("CameraLockToggle");
        m_CameraYOffsetToggleAction = RebindingManager.Instance.InputActionMap.FindAction("CameraYOffsetToggle");

        m_HyperspeedToggleAction.performed += HyperspeedToggleAction_performed;
        m_BurstAction.performed += BurstAction_performed;
        m_CameraLockToggleAction.performed += CameraLockToggleAction_performed;
        m_CameraYOffsetToggleAction.performed += CameraYOffsetToggleAction_performed;
    }

    private void OnDestroy()
    {
        m_HyperspeedToggleAction.performed -= HyperspeedToggleAction_performed;
        m_BurstAction.performed -= BurstAction_performed;
        m_CameraLockToggleAction.performed -= CameraLockToggleAction_performed;
        m_CameraYOffsetToggleAction.performed -= CameraYOffsetToggleAction_performed;
    }

    private void HyperspeedToggleAction_performed(InputAction.CallbackContext obj)
    {
        OnHyperSpeedPerformed?.Invoke();
    }

    private void BurstAction_performed(InputAction.CallbackContext obj)
    {
        OnBurstPerformed?.Invoke();
    }

    private void CameraLockToggleAction_performed(InputAction.CallbackContext obj)
    {
        OnCameraLockToggle?.Invoke();
    }

    private void CameraYOffsetToggleAction_performed(InputAction.CallbackContext obj)
    {
        OnCameraYOffsetToggle?.Invoke();
    }

    public void EnablePlayerControls()
    {
        RebindingManager.Instance.InputActionMap.Enable();
    }

    public void DisablePlayerControls()
    {
        RebindingManager.Instance.InputActionMap.Disable();
    }
}
