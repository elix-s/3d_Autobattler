using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CanvasComponent : MonoBehaviour
{
    [SerializeField] private Image _fadeBackground;
    
    private void OnEnable()
    {
        if(_fadeBackground != null) _fadeBackground.DOFade(0, 1.5f).SetEase(Ease.OutQuad);
    }
}

