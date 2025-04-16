using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    [SerializeField] private Rigidbody[] _cubes;
    [SerializeField] private float _explosionForce = 8f; 
    [SerializeField] private float _explosionRadius = 5f; 
    [SerializeField] private float _upwardModifier = 1f; 
    [SerializeField] private float _destructionDelay = 3f; 

    private void OnEnable()
    {
        Explode();
    }

    private void Explode()
    {
        foreach (Rigidbody cube in _cubes)
        {
            Vector3 explosionPosition = transform.position; 
            cube.AddExplosionForce(_explosionForce, explosionPosition, _explosionRadius, _upwardModifier, ForceMode.Impulse);
        }
        
        Invoke(nameof(DestroyCubes), _destructionDelay);
    }

    private void DestroyCubes()
    {
        foreach (Rigidbody cube in _cubes)
        {
            if (cube != null)
                Destroy(cube.gameObject);
        }
    }
}

