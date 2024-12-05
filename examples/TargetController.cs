using UnityEngine;

[RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(Collider))]
public class NPBehaveExampleMouseController : MonoBehaviour
{
    private Camera mainCamera;
    private Rigidbody body;

    [SerializeField] private float Kp; 
    private const bool topDown = false;

    void Start()
    {
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        body = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (!TryGetMouseWorldPosition(out Vector3 targetPos)) 
            return;
        
        var force = (targetPos - transform.position) * Kp;
        body.AddForce(force, ForceMode.VelocityChange);
    }

    private bool TryGetMouseWorldPosition(out Vector3 targetPos)
    {
        var ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (ray.direction.z != 0)
        {
            var delta = -ray.origin.z / ray.direction.z;
            targetPos = ray.origin + ray.direction * delta;
            return delta > 0;
        }

        targetPos = Vector3.zero;
        return false;
    }
}