using Common.GameStateService;
using Common.UIService;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class MainMenuView : MonoBehaviour
{
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _exitButton;
    
    private GameStateService _gameStateService;
    private UIService _uiService;

    [Inject]
    private void Construct(GameStateService gameStateService, UIService uiService)
    {
        _gameStateService = gameStateService;
        _uiService = uiService;
    }

    private void Awake()
    {
        _startButton.onClick.AddListener(()=> StartGame());
        _exitButton.onClick.AddListener(()=> Exit());
    }

    private async void StartGame()
    {
        await UniTask.Delay(200);
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
