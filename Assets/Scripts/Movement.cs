using UnityEngine;

// player movement script

[RequireComponent(typeof(Rigidbody2D))]
public class Movement : MonoBehaviour
{
    [SerializeField]
    private float _speed = 1f;

    private Rigidbody2D _rigidbody;
    private Camera _mainCam;
    private Vector2 _movement, _mousePosition;

    // Start is called before the first frame update
    void Start()
    {
        _mainCam = GameObject.Find("Main Camera").GetComponent<Camera>();
        _rigidbody = GetComponent<Rigidbody2D>();
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
        _rigidbody.rotation = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg - 90f;
    }
}
