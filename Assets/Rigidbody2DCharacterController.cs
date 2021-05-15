using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Rigidbody2DCharacterController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] float speed;

    Rigidbody2D rb;
    Vector2 movementDir;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update() {
        movementDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        movementDir.Normalize();
    }
    
    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + movementDir * speed * Time.fixedDeltaTime);
    }
}
