using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Movement : MonoBehaviour
{
    [SerializeField]
    private float _speed = 1f;
    [SerializeField]
    private bool _isPlayer = true;

    private Rigidbody2D _rigidbody;
    private Camera _mainCam;
    private Vector2 _movement, _mousePosition;

    // Start is called before the first frame update
    void Start()
    {
        _mainCam = GameObject.Find("Main Camera").GetComponent<Camera>();
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_isPlayer)
        {
            _movement.x = Input.GetAxis("Horizontal");
            _movement.y = Input.GetAxis("Vertical");
            _mousePosition = _mainCam.ScreenToWorldPoint(Input.mousePosition);
        }
    }

    private void FixedUpdate()
    {
        _rigidbody.MovePosition(_rigidbody.position + _movement * _speed * Time.fixedDeltaTime);

        Vector2 lookDirection = _mousePosition - _rigidbody.position;
        _rigidbody.rotation = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg - 90f;
    }
}
