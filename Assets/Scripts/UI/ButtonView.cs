using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonView : MonoBehaviour
{
    [SerializeField] private Button m_Button;

    [Header("Parameters")]
    [SerializeField] private Image m_BackgroundImage;
    [SerializeField] private Color m_InteractableColor;
    [SerializeField] private Color m_UninteractableColor;

    private void Update()
    {
        m_BackgroundImage.color = m_Button.interactable ? m_InteractableColor : m_UninteractableColor;
    }
}
