using Common.AssetsSystem;
using Cysharp.Threading.Tasks;
using DG.DemiEditor;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class UIService 
{
    private readonly IAssetProvider _assetProvider;
    private IAssetUnloader _assetUnloader;
    private IObjectResolver _container;

    public UIService(IAssetProvider assetProvider, IAssetUnloader assetUnloader, IObjectResolver container)
    {
        _assetProvider = assetProvider;
        _assetUnloader = assetUnloader;
        _container = container;
    }
    
    public async UniTask<T> ShowUIPanel<T>(string assetKey) where T : Component
    {
        if (!assetKey.IsNullOrEmpty())
        {
            var panel = await _assetProvider.GetAssetAsync<GameObject>(assetKey);
            _assetUnloader.AddResource(panel);

            var prefab = _container.Instantiate(panel).GetComponent<T>();
            _assetUnloader.AttachInstance(prefab.gameObject);

            return prefab;
        }
        else
        {
            return null;
        }
    }

    public async UniTask HideUIPanel()
    {
        _assetUnloader.Dispose();
    }
}
