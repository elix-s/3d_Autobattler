using Common.GameStateService;
using Cysharp.Threading.Tasks;

public class StartLoadingState : IGameState
{
    private GameStateService _gameState;
    private readonly Logger _logger;
    
    public StartLoadingState(GameStateService gameStateService, Logger logger)
    {
        _gameState = gameStateService;
        _logger = logger;
    }
    
    public async UniTask Enter(StatePayload payload)
    {
        _logger.Log("StartLoadingState");
        _gameState.ChangeState<MenuState>().Forget();
    }
    
    public void Update(){}
    
    public async UniTask Exit() {}
}
