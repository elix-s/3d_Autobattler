using Common.AssetsSystem;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class StartGameState : IGameState
{
    private readonly IAssetProvider _assetProvider;
    private IObjectResolver _container;
    private IAssetUnloader _assetUnloader;
    private UIService _uiService;
    
    public StartGameState(IObjectResolver container, IAssetProvider assetProvider, IAssetUnloader assetUnloader, UIService uiService)
    {
        _container = container;
        _assetProvider = assetProvider;
        _assetUnloader = assetUnloader;
        _uiService = uiService;
    }
    
    public async void Enter()
    {
        var gameMenu = await _uiService.ShowUIPanel<GameMenuView>("GameMenu");
        var panel = await _assetProvider.GetAssetAsync<GameObject>("GameState");
        var prefab = _container.Instantiate(panel);
        _assetUnloader.AddResource(panel);
        _assetUnloader.AttachInstance(prefab);
        _assetUnloader.AttachInstance(gameMenu.gameObject);
    }
    
    public void Update() {}

    public void Exit()
    {
        _assetUnloader.Dispose();
    }
}