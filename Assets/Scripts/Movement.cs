using UnityEngine;

/// <summary>
/// Контроль движения игрока
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Movement : MonoBehaviour
{
    private const string VERTICALAXIS = "Vertical";
    private const string HORIZONTALAXIS = "Horizontal";
    private const float DEG90 = 90f;

    [SerializeField]
    private float speed = 5f;
    [SerializeField]
    private float minimalDetectionSpeed = 0.01f;
    [SerializeField]
    [Range(0, 359.99f)]
    private float minimalRotationDetectionAngle = 5f;
    [SerializeField]
    private Camera mainCam;

    private new Rigidbody2D rigidbody;
    private Vector2 movement;
    private Vector2 mousePosition;
    private float lastRotation;

    void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        lastRotation = rigidbody.rotation;
    }

    /// <summary>
    /// Проверка на то, было ли совершено действие "Движение"
    /// </summary>
    /// <returns>True, если действие было совершено</returns>
    public bool CheckYVelocity()
    {
        return Mathf.Abs(movement.y) > minimalDetectionSpeed;
    }

    /// <summary>
    /// Проверка на то, было ли совершено действие "Движение в сторону"
    /// </summary>
    /// <returns>True, если действие было совершено</returns>
    public bool CheckXVelocity()
    {
        return Mathf.Abs(movement.x) > minimalDetectionSpeed;
    }

    /// <summary>
    /// Проверка на то, было ли совершено действие "Поворот"
    /// </summary>
    /// <returns>True, если действие было совершено</returns>
    public bool CheckRotation()
    {
        return Mathf.Abs(rigidbody.rotation - lastRotation) > minimalRotationDetectionAngle;
    }

    /// <summary>
    /// Обработка движения в зависимости от нажатых клавиш
    /// </summary>
    private void FixedUpdate()
    {
        movement.x = Input.GetAxis(HORIZONTALAXIS);
        movement.y = Input.GetAxis(VERTICALAXIS);
        mousePosition = mainCam.ScreenToWorldPoint(Input.mousePosition);

        rigidbody.MovePosition(rigidbody.position + movement * speed * Time.fixedDeltaTime);

        Vector2 lookDirection = mousePosition - rigidbody.position;
        lastRotation = rigidbody.rotation;
        rigidbody.rotation = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg - DEG90;

        CombinationReader.instance.CheckIfActionWasCommited(EActions.Walk);
        CombinationReader.instance.CheckIfActionWasCommited(EActions.SideWalk);
        CombinationReader.instance.CheckIfActionWasCommited(EActions.Rotation);
    }
}
