using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Vector2 movement;
    [SerializeField]
    private float speed = 1f;
    [SerializeField]
    private Rigidbody2D rb;

    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        
    }

    void FixedUpdate() {
        if(movement != Vector2.zero) Move(movement.normalized*speed);
    }

    private void Move(Vector2 velocity) {
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }
}
