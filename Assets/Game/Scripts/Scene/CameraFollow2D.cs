using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow2D : MonoBehaviour
{
    [SerializeField]
    private Transform target;

    [SerializeField]
    private float smoothSpeed = 5f;

    [SerializeField]
    private float mapLeft = -250f;

    [SerializeField]
    private float mapRight = 250f;

    private Camera cam;

    public void ConfigureBounds(float left, float right)
    {
        mapLeft = Mathf.Min(left, right);
        mapRight = Mathf.Max(left, right);
    }

    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("CameraFollow2D: компонент Camera отсутствует.", this);
            enabled = false;
        }
    }

    private void LateUpdate()
    {
        if (target == null || cam == null)
            return;

        float halfWidth =
            cam.orthographicSize * cam.aspect;

        float wantedX = target.position.x;

        float minimumX = mapLeft + halfWidth;
        float maximumX = mapRight - halfWidth;
        float clampedX = minimumX <= maximumX
            ? Mathf.Clamp(wantedX, minimumX, maximumX)
            : (mapLeft + mapRight) * 0.5f;

        Vector3 position = transform.position;

        position.x = Mathf.Lerp(
            position.x,
            clampedX,
            smoothSpeed * Time.deltaTime
        );

        transform.position = position;
    }
}
