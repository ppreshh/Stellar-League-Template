using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameModesPanel : MonoBehaviour
{
    [SerializeField] private Button m_FreeRoamModeButton = null;
    [SerializeField] private Button m_BallStadiumModeButton = null;
    [SerializeField] private Button m_RaceTrackModeButton = null;

    private enum GameMode
    {
        FREE_ROAM = 0,
        BALL_STADIUM = 1,
        RACE_TRACK = 2
    }

    private void Awake()
    {
        m_FreeRoamModeButton.onClick.AddListener(() =>
        {
            SLGameManager.Instance.CurrentGameMode = SLGameManager.GameMode.FreeRoam;
        });

        m_BallStadiumModeButton.onClick.AddListener(() =>
        {
            SLGameManager.Instance.CurrentGameMode = SLGameManager.GameMode.BallStadium;
        });

        m_RaceTrackModeButton.onClick.AddListener(() =>
        {
            SLGameManager.Instance.CurrentGameMode = SLGameManager.GameMode.RaceTrack;
        });
    }

    private void OnDestroy()
    {
        m_FreeRoamModeButton.onClick.RemoveAllListeners();
        m_BallStadiumModeButton.onClick.RemoveAllListeners();
        m_RaceTrackModeButton.onClick.RemoveAllListeners();
    }
}
