using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SLGameManager : MonoBehaviour
{
    [SerializeField] private Transform m_PlayerTransform;

    private GameMode m_CurrentGameMode = GameMode.FreeRoam;
    public GameMode CurrentGameMode 
    { 
        get { return m_CurrentGameMode; }
        set
        {
            m_PlayerTransform.position = Vector3.zero;
            m_PlayerTransform.rotation = Quaternion.identity;

            m_CurrentGameMode = value;
            OnGameModeChanged?.Invoke();
        }
    }

    public enum GameMode
    {
        FreeRoam,
        BallStadium,
        RaceTrack
    }

    public event Action OnGameModeChanged;

    public static SLGameManager Instance { private set; get; }

    private void Awake()
    {
        Instance = this;
    }
}
