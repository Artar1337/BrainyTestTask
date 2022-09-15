using UnityEngine;

// player movement script

[RequireComponent(typeof(Rigidbody2D))]
public class Movement : MonoBehaviour
{
    [SerializeField]
    private float _speed = 1f;
    [SerializeField]
    [Range(0,359.99f)]
    private float _minimalDetectionSpeed = 0.01f, _minimalRotationDetectionAngle = 5f;

    private Rigidbody2D _rigidbody;
    private Camera _mainCam;
    private Vector2 _movement, _mousePosition;
    private float _lastRotation;

    // Start is called before the first frame update
    void Start()
    {
        _mainCam = GameObject.Find("Main Camera").GetComponent<Camera>();
        _rigidbody = GetComponent<Rigidbody2D>();
        _lastRotation = _rigidbody.rotation;
    }

    public bool CheckYVelocity()
    {
        return Mathf.Abs(_movement.y) > _minimalDetectionSpeed;
    }

    public bool CheckXVelocity()
    {
        return Mathf.Abs(_movement.x) > _minimalDetectionSpeed;
    }

    public bool CheckRotation()
    {
        return Mathf.Abs(_rigidbody.rotation - _lastRotation) > _minimalRotationDetectionAngle;
    }

    private void FixedUpdate()
    {
        // get buttons
        _movement.x = Input.GetAxis("Horizontal");
        _movement.y = Input.GetAxis("Vertical");
        // get rotation
        _mousePosition = _mainCam.ScreenToWorldPoint(Input.mousePosition);

        // move player
        _rigidbody.MovePosition(_rigidbody.position + _movement * _speed * Time.fixedDeltaTime);

        //rotate player
        Vector2 lookDirection = _mousePosition - _rigidbody.position;
        _lastRotation = _rigidbody.rotation;
        _rigidbody.rotation = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg - 90f;

        CombinationReader.instance.CheckIfActionWasCommited(EActions.Walk);
        CombinationReader.instance.CheckIfActionWasCommited(EActions.SideWalk);
        CombinationReader.instance.CheckIfActionWasCommited(EActions.Rotation);
    }
}
