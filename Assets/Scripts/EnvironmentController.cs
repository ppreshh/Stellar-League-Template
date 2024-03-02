using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentController : MonoBehaviour
{
    [SerializeField] private GameObject m_AsteroidField;
    [SerializeField] private GameObject m_Stadium;
    [SerializeField] private GameObject m_Ball;
    [SerializeField] private RaceTrack m_RaceTrack;

    private void Start()
    {
        SLGameManager.Instance.OnGameModeChanged += SLGameManager_OnGameModeChanged;

        UpdateEnvironmentAssets();
    }

    private void OnDestroy()
    {
        SLGameManager.Instance.OnGameModeChanged -= SLGameManager_OnGameModeChanged;
    }

    private void SLGameManager_OnGameModeChanged()
    {
        UpdateEnvironmentAssets();
    }

    private void UpdateEnvironmentAssets()
    {
        m_AsteroidField.SetActive(false);
        m_Stadium.SetActive(false);
        m_Ball.SetActive(false);
        m_RaceTrack.ClearTrack();

        switch (SLGameManager.Instance.CurrentGameMode)
        {
            case SLGameManager.GameMode.FreeRoam:
                m_AsteroidField.SetActive(true);
                break;

            case SLGameManager.GameMode.BallStadium:
                m_Stadium.SetActive(true);
                m_Ball.SetActive(true);
                break;

            case SLGameManager.GameMode.RaceTrack:
                m_RaceTrack.InitRaceTrack();
                break;
        }
    }
}
