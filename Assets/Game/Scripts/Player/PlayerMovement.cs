using UnityEngine;

public sealed class PlayerMovement : MonoBehaviour
{
    [SerializeField] private InputService inputService;
    [SerializeField] private PlayerMotor2D motor;

    private void Update()
    {
        if (inputService != null && motor != null)
            motor.SetMoveIntent(inputService.MoveIntent);
    }

    private void OnDisable()
    {
        if (motor != null)
            motor.SetMoveIntent(0f);
    }
}
