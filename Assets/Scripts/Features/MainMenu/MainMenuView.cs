using Common.GameStateService;
using Common.SavingSystem;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class MainMenuView : MonoBehaviour
{
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _exitButton;
    [SerializeField] private TextMeshProUGUI _scoreText;
    
    private GameStateService _gameStateService;
    private SavingSystem _savingSystem;
    
    [Inject]
    private void Construct(GameStateService gameStateService, SavingSystem savingSystem)
    {
        _gameStateService = gameStateService;
        _savingSystem = savingSystem;
    }

    private async void Awake()
    {
        _startButton.onClick.AddListener(StartGame);
        _exitButton.onClick.AddListener(Exit);

        var data = await _savingSystem.LoadDataAsync<AppData>();
        var bestResult = data.BestResult;
        _scoreText.text = "Best Result: " + bestResult.ToString();
    }

    private void StartGame()
    {
        _gameStateService.ChangeState<StartGameState>().Forget();
    }

    private async void Exit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
