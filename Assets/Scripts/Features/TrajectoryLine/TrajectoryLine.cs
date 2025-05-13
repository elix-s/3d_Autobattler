using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TrajectoryLine : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] private Transform _playerTransform; 

    [Header("Line Settings")]
    [SerializeField] private float _lineThickness = 0.1f;
    [SerializeField] private Color _lineColor = Color.cyan;
    [Tooltip("The minimum distance a player must travel for a new point to be added.")]
    [SerializeField] private float _minVertexDistance = 0.1f;
    [Tooltip("Maximum number of points in the tail. 0 - no limit.")]
    [SerializeField] private int _maxPoints = 0;
    [SerializeField] private float _groundOffset = 0.01f;

    [Header("Ground Detection (Optional)")]
    [SerializeField] private bool _followGround = true;
    [SerializeField] private LayerMask _groundLayerMask = 1; 
    [SerializeField] private float _groundRaycastDistance = 2f;
    [SerializeField] private float _fixedLineY = 0f;
    [SerializeField] private bool _usePlayerYForLineIfNoGround = true;
    
    private Mesh _mesh;
    private MeshRenderer _meshRenderer;
    private List<Vector3> _points; 
    private List<Vector3> _vertices;
    private List<int> _triangles;
    private List<Vector2> _uvs;
    private Material _lineMaterial;

    private void Awake()
    {
        if (_playerTransform == null)
        {
            Debug.LogError("PlayerTransform not assigned!");
            enabled = false;
            return;
        }

        _mesh = new Mesh();
        _mesh.name = "Line";
        GetComponent<MeshFilter>().mesh = _mesh;

        _meshRenderer = GetComponent<MeshRenderer>();
        _lineMaterial = new Material(Shader.Find("Unlit/Color")); 
        _lineMaterial.color = _lineColor;
        _meshRenderer.material = _lineMaterial;

        _points = new List<Vector3>();
        _vertices = new List<Vector3>();
        _triangles = new List<int>();
        _uvs = new List<Vector2>();
        
        AddPoint(GetCharacterGroundPosition(_playerTransform.position));
    }

    private void OnDestroy()
    {
        if (_lineMaterial != null)
        {
            Destroy(_lineMaterial);
        }
    }

    private void LateUpdate()
    {
        if (_playerTransform == null) return;

        Vector3 currentCharacterPos = _playerTransform.position;
        Vector3 currentGroundPos = GetCharacterGroundPosition(currentCharacterPos);
        
        if (_points.Count == 0 || Vector3.Distance(_points[_points.Count - 1], currentGroundPos) > _minVertexDistance)
        {
            AddPoint(currentGroundPos);
            UpdateMesh();
        }
        else if (_points.Count > 0 && _points[_points.Count - 1] != currentGroundPos && _followGround)
        {
             _points[_points.Count - 1] = currentGroundPos;
             UpdateMesh();
        }
        
        if (_lineMaterial.color != _lineColor)
        {
            _lineMaterial.color = _lineColor;
        }
    }

    private Vector3 GetCharacterGroundPosition(Vector3 characterPosition)
    {
        Vector3 groundPos = characterPosition;

        if (_followGround)
        {
            RaycastHit hit;
           
            if (Physics.Raycast(characterPosition + Vector3.up * 0.5f, Vector3.down, out hit, 
                    _groundRaycastDistance + 0.5f, _groundLayerMask))
            {
                groundPos = hit.point;
            }
            else
            {
                groundPos.y = _usePlayerYForLineIfNoGround ? characterPosition.y : _fixedLineY;
            }
        }
        else
        {
            groundPos.y = _usePlayerYForLineIfNoGround ? characterPosition.y : _fixedLineY;
        }
        
        return groundPos + Vector3.up * _groundOffset;
    }

    private void AddPoint(Vector3 point)
    {
        _points.Add(point);
        
        if (_maxPoints > 0 && _points.Count > _maxPoints)
        {
            _points.RemoveAt(0);
        }
    }

    private void UpdateMesh()
    {
        if (_points.Count < 2)
        {
            _mesh.Clear();
            return;
        }

        _vertices.Clear();
        _triangles.Clear();
        _uvs.Clear();

        for (int i = 0; i < _points.Count; i++)
        {
            Vector3 currentPoint = _points[i];
            Vector3 direction;

            if (i < _points.Count - 1)
            {
                direction = (_points[i + 1] - currentPoint).normalized;
            }
            else
            {
                direction = (currentPoint - _points[i - 1]).normalized;
            }

            if (direction == Vector3.zero)
            {
                if (i > 0) direction = (currentPoint - _points[i - 1]).normalized;
                if (direction == Vector3.zero && i > 1) direction = (_points[i - 1] - _points[i - 2]).normalized;
                if (direction == Vector3.zero && _playerTransform != null) direction = _playerTransform.forward;
                if (direction == Vector3.zero) direction = Vector3.forward; 
            }
            
            Vector3 sideVector = Vector3.Cross(direction, Vector3.up).normalized;
            
            if (sideVector == Vector3.zero)
            {
                if (_playerTransform != null)
                {
                    sideVector = Vector3.Cross(_playerTransform.forward, direction).normalized;
                    
                    if (sideVector == Vector3.zero) 
                    {
                        sideVector = _playerTransform.right.normalized;
                        
                        if (sideVector == Vector3.zero) 
                        {
                            sideVector = Vector3.right; 
                        }
                    }
                }
                else
                {
                    sideVector = Vector3.right;
                }
            }

            _vertices.Add(currentPoint + sideVector * (_lineThickness / 2f));
            _vertices.Add(currentPoint - sideVector * (_lineThickness / 2f));

            float u = (i == _points.Count -1 && _points.Count > 1) ? 1f : (float)i / (_points.Count > 1 ? _points.Count - 1 : 1);
            _uvs.Add(new Vector2(u, 0));
            _uvs.Add(new Vector2(u, 1));

            if (i > 0)
            {
                int baseIndex = _vertices.Count - 4;
                _triangles.Add(baseIndex + 0);
                _triangles.Add(baseIndex + 2);
                _triangles.Add(baseIndex + 1);
                _triangles.Add(baseIndex + 1);
                _triangles.Add(baseIndex + 2);
                _triangles.Add(baseIndex + 3);
            }
        }

        _mesh.Clear();
        _mesh.vertices = _vertices.ToArray();
        _mesh.triangles = _triangles.ToArray();
        _mesh.uv = _uvs.ToArray();
        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();
    }
        
    private void OnDrawGizmosSelected()
    {
        if (_points == null || _points.Count < 2) return;

        Gizmos.color = Color.yellow;
        
        for (int i = 0; i < _points.Count; i++)
        {
            Gizmos.DrawSphere(_points[i], 0.05f);
            
            if (i < _points.Count - 1)
            {
                Gizmos.DrawLine(_points[i], _points[i+1]);
            }
        }
        
        if (_vertices != null && _vertices.Count > 0)
        {
            Gizmos.color = Color.magenta;
            
            foreach (var vertex in _vertices)
            {
                Gizmos.DrawSphere(vertex, 0.02f);
            }
        }
    }
}