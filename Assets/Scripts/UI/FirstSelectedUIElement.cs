using UnityEngine;
using UnityEngine.EventSystems;

public class FirstSelectedUIElement : MonoBehaviour
{
    [SerializeField] private GameObject m_FirstSelectedUIElement;

    private void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(m_FirstSelectedUIElement);
    }
}
