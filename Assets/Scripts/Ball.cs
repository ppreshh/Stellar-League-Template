using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody m_Rigidbody;

    [Header("Parameters")]
    [SerializeField] private bool m_IsExtraForceEnabled = false;
    [SerializeField] private float m_ExtraForce = 10f;

    private CameraController m_CameraController;

    private void Awake()
    {
        m_CameraController = FindObjectOfType<CameraController>();
    }

    private void OnEnable()
    {
        m_CameraController.TargetToLockOn = transform;
    }

    private void OnDisable()
    {
        m_CameraController.ForceUnlockCamera();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Ship")
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.BallHit);
        }
        else if (collision.gameObject.name == "GoalCollider")
        {
            transform.position = Vector3.zero;
            m_Rigidbody.velocity = Vector3.zero;
            return;
        }

        if (m_IsExtraForceEnabled)
        {
            m_Rigidbody.AddForce(collision.GetContact(0).normal * m_ExtraForce, ForceMode.Impulse);
        }
    }
}
