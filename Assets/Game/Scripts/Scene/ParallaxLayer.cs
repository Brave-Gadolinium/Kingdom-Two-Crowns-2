using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [SerializeField]
    [Range(0f, 1f)]
    private float parallaxStrength = 0.5f;

    private Transform cameraTransform;
    private Vector3 previousCameraPosition;

    private void Start()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            enabled = false;
            return;
        }

        cameraTransform = mainCamera.transform;
        previousCameraPosition = cameraTransform.position;
    }

    private void LateUpdate()
    {
        Vector3 delta =
            cameraTransform.position - previousCameraPosition;

        transform.position += new Vector3(
            delta.x * parallaxStrength,
            0f,
            0f
        );

        previousCameraPosition = cameraTransform.position;
    }
}
