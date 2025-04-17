using TMPro;
using UnityEngine;
using VContainer;
using Features.EventDispatcher;

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

    private void OnDestroy()
    {
        _dispatcher.GameDispatcher.UnsubscribeListener<IncreaseScoreEvent>(this);
    }
}
