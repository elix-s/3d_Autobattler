using System;
using TMPro;
using UnityEngine;
using VContainer;

public class GameMenuView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _scoresText;
    private EventsDispatcher _dispatcher;

    [Inject]
    private void Construct(EventsDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    private void Awake()
    {
        _dispatcher.GameDispatcher.Subscribe<IncreaseScoreEvent>(this, SetScores);
    }

    private void SetScores(IncreaseScoreEvent e)
    {
        _scoresText.text = "Scores: " + e.Score.ToString();
    }
}
