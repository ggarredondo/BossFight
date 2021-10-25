using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public Rigidbody rb;
    public Collider col;
    public Transform cam;

    public float walk_speed = 10f, walk_speed_min = 0f, walk_speed_max = 0.5f;
    public float run_speed = 15f;
    public float sprint_speed = 20f;
    public float turn_smoothness = 0.2f;
    public float jump_force = 5f;
    public float jump_cooldown = 1f;

    private float dist_to_ground, jump_time;
    private float move_magnitude, turn_smooth_velocity, target_angle, rotation_angle, final_speed; // movement variables
    private Vector3 direction, move_dir;

    private void Start() {
        rb = GetComponent<Rigidbody>();
        dist_to_ground = col.bounds.extents.y;
    }

    private bool IsGrounded() {
        return Physics.Raycast(transform.position, -Vector3.up, dist_to_ground + 0.1f);
    }

    private void UnlockedMovement()
    {
        // base movement
        direction.Set(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        move_magnitude = direction.sqrMagnitude;
        direction = direction.normalized;

        // character faces the direction it's moving to
        target_angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
        rotation_angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, target_angle, ref turn_smooth_velocity, turn_smoothness);
        if (direction.magnitude >= 0.1f)
            transform.rotation = Quaternion.Euler(0f, rotation_angle, 0f);
        move_dir = (Quaternion.Euler(0f, target_angle, 0f) * Vector3.forward).normalized; // movement relative to the camera

        if (move_magnitude >= walk_speed_min && move_magnitude < walk_speed_max)
            final_speed = walk_speed;
        else if (Input.GetButton("Sprint"))
            final_speed = sprint_speed;
        else
            final_speed = run_speed;
        rb.AddForce(move_dir * direction.sqrMagnitude * final_speed);
    }

    private void Update()
    {
        // jumping (cooldown between jumps)
        if (Input.GetButtonDown("Jump") && IsGrounded() && jump_time <= Time.time) {
            jump_time = Time.time + jump_cooldown;
            rb.AddForce(Vector3.up * jump_force, ForceMode.Impulse);
        }
    }

    void FixedUpdate()
    {
        UnlockedMovement();
    }
}
