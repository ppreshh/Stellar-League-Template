using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    [SerializeField] private string m_ActionMapName;
    [SerializeField] private InputActionAsset m_InputActionAsset;

    private InputAction m_ToggleStartMenuAction;

    private bool m_IsStartMenuActive = false;

    public event EventHandler<ToggleStartMenuPerformedEventArgs> OnToggleStartMenuPerformed;
    public class ToggleStartMenuPerformedEventArgs : EventArgs
    {
        public bool IsActive;
    }

    public static UIManager Instance { private set; get; }

    private void Awake()
    {
        Instance = this;

        Cursor.visible = false;

        var inputActionMap = m_InputActionAsset.FindActionMap(m_ActionMapName);

        m_ToggleStartMenuAction = inputActionMap.FindAction("ToggleStartMenu");

        m_ToggleStartMenuAction.performed += ToggleStartMenuAction_performed;
    }

    private void OnDestroy()
    {
        m_ToggleStartMenuAction.performed -= ToggleStartMenuAction_performed;
    }

    private void ToggleStartMenuAction_performed(InputAction.CallbackContext obj)
    {
        m_IsStartMenuActive = !m_IsStartMenuActive;

        Cursor.visible = m_IsStartMenuActive;

        if (m_IsStartMenuActive) InputManager.Instance.DisablePlayerControls();
        else InputManager.Instance.EnablePlayerControls();

        OnToggleStartMenuPerformed?.Invoke(this, new ToggleStartMenuPerformedEventArgs { IsActive = m_IsStartMenuActive });
    }
}
