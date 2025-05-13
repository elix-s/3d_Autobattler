using Common.AssetsSystem;
using Common.AudioService;
using Common.UIService;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class MenuState : IGameState
{
    private Logger _logger;
    private SceneLoader _sceneLoader;
    private UIService _uiService;
    private AudioService _audioService;
    private IObjectResolver _container;
    private readonly IAssetProvider _assetProvider;
    private IAssetUnloader _assetUnloader;
    
    private bool _isInitialized = false;
    
    public MenuState(Logger logger, SceneLoader sceneLoader, UIService uiService, AudioService audioService, IObjectResolver container, 
        IAssetProvider assetProvider, IAssetUnloader assetUnloader)
    {
        _logger = logger;
        _sceneLoader = sceneLoader;
        _uiService = uiService;
        _audioService = audioService;
        _container = container;
        _assetProvider = assetProvider;
        _assetUnloader = assetUnloader;
    }

    public async UniTask Enter(StatePayload payload)
    {
        var transition = await _uiService.ShowUIPanelWithComponent<StateTransitionWindowView>("StateTransitionWindow");
        transition.Fade(1500);
        
        var mainMenu = await _uiService.ShowUIPanelWithComponent<MainMenuView>("MainMenu");
        var panel = await _assetProvider.GetAssetAsync<GameObject>("MenuState");
        var prefab = _container.Instantiate(panel);
        
        _assetUnloader.AddResource(panel);
        _assetUnloader.AttachInstance(prefab);
        _assetUnloader.AttachInstance(mainMenu.gameObject);
        
        _isInitialized = true;
    }
    
    public void Update()
    {
        
    }

    public async UniTask Exit()
    {
        _assetUnloader.Dispose();
    }
}
