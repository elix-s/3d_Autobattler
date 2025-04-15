using UnityEngine;

public class GameCamera : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform _target;           
    [SerializeField] private Vector3 _lookAtOffset = new Vector3(0, 1.5f, 0);

    [Header("Camera Movement")]
    [SerializeField] private float _followSmoothTime = 0.2f; 
    [SerializeField] private Vector3 _initialOffset = new Vector3(0, 5, -7); 

    [Header("Camera Rotation")]
    [SerializeField] private bool _enableRotation = true;    
    [SerializeField] private int _rotateMouseButton = 1;      
    [SerializeField] private float _rotationSpeedX = 4.0f;   
    [SerializeField] private float _rotationSpeedY = 2.0f;   
    [SerializeField] private float _minPitch = -30.0f;        
    [SerializeField] private float _maxPitch = 70.0f;        

    [Header("Camera Zoom")]
    [SerializeField] private bool _enableZoom = true;        
    [SerializeField] private float _zoomSpeed = 5.0f;         
    [SerializeField] private float _minZoomDistance = 3.0f;   
    [SerializeField] private float _maxZoomDistance = 15.0f;  

    [Header("Collision Handling")]
    [SerializeField] private bool _handleCollisions = true;  
    [SerializeField] private LayerMask _collisionLayers = 1; 
    [SerializeField] private float _collisionPadding = 0.2f; 
    
    private float _currentYaw = 0.0f;    
    private float _currentPitch = 0.0f;  
    private float _currentDistance;      
    private Vector3 _positionVelocity = Vector3.zero; 
    private bool _isRotating = false;    

    private void Start()
    {
        if (_target == null)
        {
            Debug.LogError("Target not set for GameCamera!", this);
            enabled = false; 
            return;
        }
        
        _currentDistance = _initialOffset.magnitude;
        Quaternion initialRotation = Quaternion.LookRotation(GetLookAtPoint() - (GetTargetPosition() + _initialOffset));
        _currentYaw = initialRotation.eulerAngles.y;
        _currentPitch = initialRotation.eulerAngles.x;
        
        _currentDistance = Mathf.Clamp(_currentDistance, _minZoomDistance, _maxZoomDistance);
        _currentPitch = Mathf.Clamp(_currentPitch, _minPitch, _maxPitch);
        
        Vector3 initialPosition = CalculateDesiredPosition();
        transform.position = initialPosition;
        transform.LookAt(GetLookAtPoint());
    }

    private void Update()
    {
        if (_target == null) return;
        
        HandleZoomInput();
        HandleRotationInput();
    }

    private void FixedUpdate()
    {
        if (_target == null) return;
        
        Vector3 desiredPosition = CalculateDesiredPosition();
        Vector3 finalPosition = desiredPosition;
        
        if (_handleCollisions)
        {
            RaycastHit hit;
           
            if (Physics.Linecast(GetLookAtPoint(), desiredPosition, out hit, _collisionLayers))
            {
                finalPosition = hit.point + hit.normal * _collisionPadding;
              
                 if (Vector3.Distance(finalPosition, GetLookAtPoint()) < _minZoomDistance * 0.5f) 
                 {
                     finalPosition = GetLookAtPoint() - transform.forward * (_minZoomDistance * 0.5f);
                 }
            }
        }
        
        transform.position = Vector3.SmoothDamp(transform.position, finalPosition, ref _positionVelocity, _followSmoothTime);
        transform.LookAt(GetLookAtPoint());
        
        ManageCursor();
    }
    
    private Vector3 GetTargetPosition()
    {
        return _target.position;
    }

    private Vector3 GetLookAtPoint()
    {
        return GetTargetPosition() + _lookAtOffset;
    }

    private void HandleZoomInput()
    {
        if (!_enableZoom) return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        
        if (Mathf.Abs(scroll) > 0.01f) 
        {
            _currentDistance -= scroll * _zoomSpeed;
            _currentDistance = Mathf.Clamp(_currentDistance, _minZoomDistance, _maxZoomDistance);
        }
    }

    private void HandleRotationInput()
    {
        if (!_enableRotation) return;
        
        if (Input.GetMouseButtonDown(_rotateMouseButton))
        {
            _isRotating = true;
        }
        if (Input.GetMouseButtonUp(_rotateMouseButton))
        {
            _isRotating = false;
        }
        
        if (_isRotating)
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            _currentYaw += mouseX * _rotationSpeedX * Time.deltaTime * 100f; 
            _currentPitch -= mouseY * _rotationSpeedY * Time.deltaTime * 100f; 

            _currentPitch = Mathf.Clamp(_currentPitch, _minPitch, _maxPitch); 
        }
    }

    private Vector3 CalculateDesiredPosition()
    {
        Quaternion rotation = Quaternion.Euler(_currentPitch, _currentYaw, 0);
        Vector3 direction = rotation * Vector3.forward;
        Vector3 desiredPosition = GetLookAtPoint() - direction * _currentDistance;
        return desiredPosition;
    }

     private void ManageCursor()
     {
         if (_enableRotation) 
         {
             if (_isRotating)
             {
                 Cursor.lockState = CursorLockMode.Locked; 
                 Cursor.visible = false;                  
             }
             else
             {
                 Cursor.lockState = CursorLockMode.None;   
                 Cursor.visible = true;                   
             }
         }
     }
     
     private void OnDisable() 
     {
         if (Cursor.lockState == CursorLockMode.Locked)
         {
              Cursor.lockState = CursorLockMode.None;
              Cursor.visible = true;
         }
     }
     
    private void OnDrawGizmosSelected()
    {
        if (_target != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(GetLookAtPoint(), 0.1f); 
            Gizmos.color = Color.blue;
            Vector3 desiredPos = CalculateDesiredPosition();
            Gizmos.DrawLine(GetLookAtPoint(), desiredPos); 
            Gizmos.DrawWireSphere(desiredPos, 0.2f); 
            
            if (_handleCollisions && Application.isPlaying) 
            {
                 RaycastHit hit;
                 
                 if (Physics.Linecast(GetLookAtPoint(), desiredPos, out hit, _collisionLayers))
                 {
                     Gizmos.color = Color.red;
                     Gizmos.DrawSphere(hit.point, 0.15f);
                     Gizmos.DrawLine(hit.point, hit.point + hit.normal * 0.5f);
                 }
            }
        }
    }
}