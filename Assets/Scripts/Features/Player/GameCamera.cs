using UnityEngine;

public class GameCamera : MonoBehaviour
{
    [SerializeField] private Transform _target; 
    [SerializeField] private Rigidbody _targetRb; 
    [SerializeField] private float _delayTime = 0.2f;
    [SerializeField] private Vector3 _offset = new Vector3(0, 5, -7);
    [SerializeField] private float _zoomSpeed = 2f;
    [SerializeField] private float _minZoom = 3f;
    [SerializeField] private float _maxZoom = 10f;
    [SerializeField] private float _currentZoom;
    [SerializeField] private Vector3 _velocity = Vector3.zero;
    
    void Start()
    {
        _currentZoom = -_offset.z;
    }

    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        
        if (scroll != 0)
        {
            _currentZoom -= scroll * _zoomSpeed;
            _currentZoom = Mathf.Clamp(_currentZoom, _minZoom, _maxZoom);
            _offset.z = -_currentZoom;
        }
    }

    void FixedUpdate()
    {
        if (_target == null || _targetRb == null) return;
        
        Vector3 targetPosition = _targetRb.position + _offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _velocity, _delayTime);
        
        transform.LookAt(_targetRb.position);
    }
}