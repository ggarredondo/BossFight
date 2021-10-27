using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public Rigidbody rb;
    public Collider col;
    public Transform cam;
    public Animator anim;

    public float walk_speed = 10f, walk_speed_min = 0f, walk_speed_max = 0.5f;
    public float run_speed = 15f;
    public float sprint_speed = 20f;
    public float turn_smoothness = 0.2f;
    public float jump_force = 5f;
    public float jump_cooldown = 1f;

    private float horizontal, vertical, move_magnitude, final_speed,
        turn_smooth_velocity, target_angle, rotation_angle; // movement variables
    private Vector3 direction, move_dir;
    private float dist_to_ground, jump_time; // jumping variables
    private bool is_moving, is_walking, is_sprinting, is_jumping, is_falling, is_locked; // animator variables

    private void Start() {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        dist_to_ground = col.bounds.extents.y;
        is_locked = false;
    }

    private bool IsGrounded() {
        return Physics.Raycast(transform.position, -Vector3.up, dist_to_ground + 0.1f);
    }

    private void UnlockedMovement()
    {
        // base movement
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        direction.Set(horizontal, 0f, vertical);
        move_magnitude = direction.sqrMagnitude;
        direction = direction.normalized;

        // character faces the direction it's moving to
        target_angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
        rotation_angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, target_angle, ref turn_smooth_velocity, turn_smoothness);
        is_moving = direction.magnitude >= 0.1f;
        if (is_moving)
            transform.rotation = Quaternion.Euler(0f, rotation_angle, 0f);
        move_dir = (Quaternion.Euler(0f, target_angle, 0f) * Vector3.forward).normalized; // movement relative to the camera

        // base movement phases: walking, running and sprinting
        is_walking = move_magnitude >= walk_speed_min && move_magnitude < walk_speed_max && is_moving;
        is_sprinting = Input.GetButton("Sprint") && !is_walking;
        if (is_walking)
            final_speed = walk_speed;
        else if (is_sprinting)
            final_speed = sprint_speed;
        else
            final_speed = run_speed;
        rb.AddForce(move_dir * direction.sqrMagnitude * final_speed);
    }

    private void Jump()
    {
        // jumping (cooldown between jumps)
        is_falling = !IsGrounded();
        is_jumping = Input.GetButtonDown("Jump") && !is_falling && jump_time <= Time.time;
        if (is_jumping) {
            jump_time = Time.time + jump_cooldown;
            rb.AddForce(Vector3.up * jump_force, ForceMode.Impulse);
        }
    }

    private void Animation()
    {
        anim.SetFloat("horizontal", horizontal);
        anim.SetFloat("vertical", vertical);
        anim.SetBool("is_locked", is_locked);
        anim.SetBool("is_moving", is_moving);
        anim.SetBool("is_walking", is_walking);
        anim.SetBool("is_sprinting", is_sprinting);
        anim.SetBool("is_jumping", is_jumping);
        anim.SetBool("is_falling", is_falling);
    }

    void Update()
    {
        Jump();
        Animation();
    }

    void FixedUpdate()
    {
        UnlockedMovement();
    }
}
