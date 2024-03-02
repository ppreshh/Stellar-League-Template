using UnityEngine;

public class MainCanvas : MonoBehaviour
{
    [SerializeField] private SettingsPanel m_SettingsPanel;

    private void Start()
    {
        UIManager.Instance.OnToggleStartMenuPerformed += UIManager_OnToggleStartMenuPerformed;
    }

    private void OnDestroy()
    {
        UIManager.Instance.OnToggleStartMenuPerformed -= UIManager_OnToggleStartMenuPerformed;
    }

    private void UIManager_OnToggleStartMenuPerformed(object sender, UIManager.ToggleStartMenuPerformedEventArgs e)
    {
        m_SettingsPanel.gameObject.SetActive(e.IsActive);
    }
}
