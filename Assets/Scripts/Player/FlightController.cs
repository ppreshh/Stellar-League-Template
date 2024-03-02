using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using FMOD.Studio;

public class FlightController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody m_Rigidbody = null;
    public Rigidbody Rigidbody { get { return m_Rigidbody; } }

    [Header("Parameters")]
    [SerializeField] private float m_ThrusterForce = 30f;
    [SerializeField] private float m_MagnetThrusterForce = 150f;
    [SerializeField] private float m_BoosterForce = 70f;
    [SerializeField] private float m_BurstForce = 30f;
    [SerializeField] private int m_BurstCooldown = 1;
    [SerializeField] private float m_HyperSpeedBoostMultiplier = 4f;
    [SerializeField] private float m_MagnetForce = 10f;

    public enum HyperSpeedTransition
    {
        PREPARING_TO_GOING, GOING_TO_STOPPING, PREPARING_TO_FAILING, DEFAULT_TO_PREPARING
    }

    public delegate void HyperSpeedStateChanged(HyperSpeedTransition transition);
    public event HyperSpeedStateChanged OnHyperSpeedStateChanged;
    private void RaiseOnHyperSpeedStateChanged(HyperSpeedTransition transition)
    {
        if (OnHyperSpeedStateChanged != null)
        {
            OnHyperSpeedStateChanged(transition);
        }
    }

    public delegate void DirectionalBurst(int burstCooldown);
    public event DirectionalBurst OnDirectionalBurst;
    private void RaiseOnDirectionalBurst()
    {
        if (OnDirectionalBurst != null)
        {
            OnDirectionalBurst(m_BurstCooldown);
        }
    }

    public bool IsPitchingUp { get { return InputManager.Instance.PitchYawValue.y < 0 && !m_IsHyperSpeedPreparing && !m_IsStrafing && !m_IsMagnetActivated; } }
    public bool IsPitchingDown { get { return InputManager.Instance.PitchYawValue.y > 0 && !m_IsHyperSpeedPreparing && !m_IsStrafing && !m_IsMagnetActivated; } }
    public bool IsYawingRight { get { return InputManager.Instance.PitchYawValue.x > 0 && !InputManager.Instance.IsRolling && !m_IsHyperSpeedPreparing && !m_IsStrafing; } }
    public bool IsYawingLeft { get { return InputManager.Instance.PitchYawValue.x < 0 && !InputManager.Instance.IsRolling && !m_IsHyperSpeedPreparing && !m_IsStrafing; } }
    public bool IsRollingRight { get { return InputManager.Instance.PitchYawValue.x > 0 && InputManager.Instance.IsRolling && !m_IsHyperSpeedPreparing && !m_IsStrafing && !m_IsMagnetActivated; } }
    public bool IsRollingLeft { get { return InputManager.Instance.PitchYawValue.x < 0 && InputManager.Instance.IsRolling && !m_IsHyperSpeedPreparing && !m_IsStrafing && !m_IsMagnetActivated; } }
    public bool IsReversing { get { return InputManager.Instance.ReverseBoostersValue > 0 && !m_IsHyperSpeedPreparing && !m_IsHyperSpeedActivated && !m_IsStrafing; } }
    public bool IsStrafingUp { get { return m_IsStrafing && InputManager.Instance.PitchYawValue.y > 0 && !m_IsMagnetActivated; } }
    public bool IsStrafingDown { get { return m_IsStrafing && InputManager.Instance.PitchYawValue.y < 0 && !m_IsMagnetActivated; } }
    public bool IsStrafingRight { get { return m_IsStrafing && InputManager.Instance.PitchYawValue.x > 0; } }
    public bool IsStrafingLeft { get { return m_IsStrafing && InputManager.Instance.PitchYawValue.x < 0; } }

    private bool m_IsStrafing = false;
    public bool IsStrafing { get { return m_IsStrafing; } }

    private bool m_IsHyperSpeedActivated = false;
    public bool IsHyperSpeedActivated { get { return m_IsHyperSpeedActivated; } }

    private bool m_IsHyperSpeedPreparing = false;
    public bool IsHyperSpeedPreparing { get { return m_IsHyperSpeedPreparing; } }

    private bool m_IsBurstActivated = false;
    public bool IsBursting { get { return m_IsBurstActivated && !m_IsBurstCoolingDown && !m_IsHyperSpeedActivated; } }

    public bool IsJumping { get { return m_IsBurstActivated && !m_IsHyperSpeedActivated && m_IsMagnetActivated && InputManager.Instance.PitchYawValue.x == 0 && InputManager.Instance.PitchYawValue.y == 0; } }

    private float m_ChargeUpIntensity = 0f;
    public float ChargeUpIntensity { get { return m_ChargeUpIntensity; } }

    private EventInstance m_BoosterEventInstance;
    private EventInstance m_ChargeUpEventInstance;
    private EventInstance m_HyperSpeedEventInstance;
    
    private Coroutine m_ChargeUpCoroutine = null;
    private Coroutine m_PreparingHyperSpeedCoroutine = null;
    private bool m_IsBurstCoolingDown = false;
    private bool m_IsMagnetActivated = false;

    private void Start()
    {
        InputManager.Instance.OnHyperSpeedPerformed += InputManager_OnHyperSpeedPerformed;
        InputManager.Instance.OnBurstPerformed += InputManager_OnBurstPerformed;

        m_BoosterEventInstance = AudioManager.instance.CreateEventInstance(FMODEvents.instance.Booster);
        m_ChargeUpEventInstance = AudioManager.instance.CreateEventInstance(FMODEvents.instance.ChargeUp);
        m_HyperSpeedEventInstance = AudioManager.instance.CreateEventInstance(FMODEvents.instance.HyperSpeed);
    }

    private void Update()
    {
        UpdateSound();

        UpdateChargeUpIntensity();
    }

    private void FixedUpdate()
    {
        ApplyForces();

        ApplyBurstForce();
    }

    private void OnDestroy()
    {
        InputManager.Instance.OnHyperSpeedPerformed -= InputManager_OnHyperSpeedPerformed;
        InputManager.Instance.OnBurstPerformed -= InputManager_OnBurstPerformed;
    }

    private void UpdateSound()
    {
        AudioManager.instance.PlayLoop(InputManager.Instance.ForwardBoostersValue > 0 && !m_IsHyperSpeedPreparing && !m_IsStrafing, m_BoosterEventInstance);
        m_BoosterEventInstance.setParameterByName("booster_intensity", InputManager.Instance.ForwardBoostersValue);

        AudioManager.instance.PlayLoop(m_ChargeUpIntensity > 0 && !m_IsHyperSpeedPreparing && !m_IsStrafing, m_ChargeUpEventInstance, false);
        m_ChargeUpEventInstance.setParameterByName("chargeup_intensity", m_ChargeUpIntensity);

        AudioManager.instance.PlayLoop(m_IsHyperSpeedActivated, m_HyperSpeedEventInstance, true);
    }

    private void ApplyForces()
    {
        if (!m_IsHyperSpeedPreparing && !m_IsHyperSpeedActivated)
        {
            float thrusterForce = m_IsMagnetActivated ? m_MagnetThrusterForce : m_ThrusterForce;

            if (InputManager.Instance.ForwardBoostersValue == 1f && InputManager.Instance.ReverseBoostersValue == 1f)
            {
                m_IsStrafing = true;

                Vector3 strafingForce = transform.TransformDirection(new Vector3(
                    InputManager.Instance.PitchYawValue.x * thrusterForce,
                    m_IsMagnetActivated ? 0f : InputManager.Instance.PitchYawValue.y * thrusterForce,
                    0f));

                m_Rigidbody.AddForce(strafingForce);
            }
            else
            {
                m_IsStrafing = false;

                // Torque
                Vector3 torque = transform.TransformDirection(new Vector3(
                    m_IsMagnetActivated ? 0f : InputManager.Instance.PitchYawValue.y * m_ThrusterForce,
                    InputManager.Instance.IsRolling ? 0f : InputManager.Instance.PitchYawValue.x * thrusterForce,
                    InputManager.Instance.IsRolling && !m_IsMagnetActivated ? -InputManager.Instance.PitchYawValue.x * m_ThrusterForce : 0f));

                m_Rigidbody.AddTorque(torque);

                // Booster
                Vector3 boosterForceVector = InputManager.Instance.ForwardBoostersValue * m_BoosterForce * transform.forward;
                m_Rigidbody.AddForce(boosterForceVector);

                // Reverse
                Vector3 reverseForceVector = -InputManager.Instance.ReverseBoostersValue * m_ThrusterForce * transform.forward;
                m_Rigidbody.AddForce(reverseForceVector);
            }

            if (m_IsMagnetActivated)
            {
                Vector3 gravityForce = -transform.up * m_MagnetForce;
                m_Rigidbody.AddForce(gravityForce);
            }
        }
        else if (m_IsHyperSpeedActivated)
        {
            // Torque
            Vector3 torque = transform.TransformDirection(new Vector3(
                InputManager.Instance.PitchYawValue.y * m_ThrusterForce / 4f,
                InputManager.Instance.IsRolling ? 0f : InputManager.Instance.PitchYawValue.x * m_ThrusterForce / 4f,
                InputManager.Instance.IsRolling ? -InputManager.Instance.PitchYawValue.x * m_ThrusterForce / 4f : 0f));

            m_Rigidbody.AddTorque(torque);

            // Booster
            Vector3 boosterForceVector = m_HyperSpeedBoostMultiplier * m_BoosterForce * transform.forward;
            m_Rigidbody.AddForce(boosterForceVector);
        }
    }

    private void ApplyBurstForce()
    {
        if (IsBursting)
        {
            if (m_IsMagnetActivated)
            {
                AudioManager.instance.PlayOneShot(FMODEvents.instance.BurstThruster);

                Vector3 jumpForce = transform.up * (m_MagnetForce * 1f);
                m_Rigidbody.AddForce(jumpForce, ForceMode.Impulse);
            }
            else if (Math.Abs(InputManager.Instance.PitchYawValue.x) > 0f || Math.Abs(InputManager.Instance.PitchYawValue.y) > 0f)
            {
                AudioManager.instance.PlayOneShot(FMODEvents.instance.BurstThruster);

                m_IsBurstCoolingDown = true;
                StartCoroutine(BurstCooldown(m_BurstCooldown));
                RaiseOnDirectionalBurst();

                // Convert pitchYawValue to a direction vector
                Vector3 direction = new Vector3(InputManager.Instance.PitchYawValue.x, 0f, InputManager.Instance.PitchYawValue.y);
                direction = transform.TransformDirection(direction);
                direction.Normalize();

                // Calculate the torque to rotate towards the burst direction
                Vector3 rotationAxis = Vector3.Cross(transform.up, direction).normalized;
                Vector3 torque = rotationAxis * 90f * 2f * m_BurstForce;

                // Apply the burst force
                Vector3 burstForce = direction * m_BurstForce;
                m_Rigidbody.AddForce(burstForce, ForceMode.Impulse);

                // Apply the torque
                m_Rigidbody.AddTorque(torque, ForceMode.Impulse);
            }
        }

        // Reset
        m_IsBurstActivated = false;
    }

    private IEnumerator BurstCooldown(int seconds)
    {
        int counter = seconds;
        while (counter > 0)
        {
            yield return new WaitForSeconds(1);
            counter--;
        }

        m_IsBurstCoolingDown = false;
    }

    private IEnumerator ChargeUpIEnumerator()
    {
        while (m_ChargeUpIntensity < 1f)
        {
            m_ChargeUpIntensity += 0.02f;
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void UpdateChargeUpIntensity()
    {
        if (InputManager.Instance.ForwardBoostersValue == 1f && !m_IsStrafing)
        {
            if (m_ChargeUpIntensity == 0)
            {
                if (m_ChargeUpCoroutine != null) StopCoroutine(m_ChargeUpCoroutine);
                m_ChargeUpCoroutine = StartCoroutine(ChargeUpIEnumerator());
            }
        }
        else
        {
            if (m_ChargeUpCoroutine != null)
            {
                StopCoroutine(m_ChargeUpCoroutine);
                m_ChargeUpCoroutine = null;
                if (m_ChargeUpIntensity >= 1 && !m_IsHyperSpeedPreparing) AudioManager.instance.PlayOneShot(FMODEvents.instance.ChargeDown);
            }
            m_ChargeUpIntensity = 0f;
        }
    }

    private void InputManager_OnHyperSpeedPerformed()
    {
        if (m_ChargeUpIntensity >= 1f && InputManager.Instance.ForwardBoostersValue == 1f && !m_IsHyperSpeedActivated && !m_IsHyperSpeedPreparing)
        {
            m_IsHyperSpeedPreparing = true;
            RaiseOnHyperSpeedStateChanged(HyperSpeedTransition.DEFAULT_TO_PREPARING);

            if (m_PreparingHyperSpeedCoroutine != null) StopCoroutine(m_PreparingHyperSpeedCoroutine);
            m_PreparingHyperSpeedCoroutine = StartCoroutine(HyperSpeedPreparationCountdown(1));
        }

        if (m_IsHyperSpeedActivated)
        {
            m_IsHyperSpeedActivated = false;
            RaiseOnHyperSpeedStateChanged(HyperSpeedTransition.GOING_TO_STOPPING);
            AudioManager.instance.PlayOneShot(FMODEvents.instance.ChargeDown);
        }
    }

    private IEnumerator HyperSpeedPreparationCountdown(int seconds)
    {
        AudioManager.instance.PlayOneShot(FMODEvents.instance.HyperSpeedPreparing);

        int counter = seconds;
        while(counter > 0)
        {
            yield return new WaitForSeconds(1);
            counter--;
        }

        m_IsHyperSpeedPreparing = false;

        if (InputManager.Instance.ForwardBoostersValue == 0f)
        {
            m_IsHyperSpeedActivated = true;
            RaiseOnHyperSpeedStateChanged(HyperSpeedTransition.PREPARING_TO_GOING);
        }
        else
        {
            RaiseOnHyperSpeedStateChanged(HyperSpeedTransition.PREPARING_TO_FAILING);
        }

        m_PreparingHyperSpeedCoroutine = null;
    }

    private void InputManager_OnBurstPerformed()
    {
        m_IsBurstActivated = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "MagnetTrigger" && !m_IsMagnetActivated)
        {
            m_IsMagnetActivated = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.name == "MagnetTrigger" && m_IsMagnetActivated)
        {
            m_IsMagnetActivated = false;
        }
    }
}
