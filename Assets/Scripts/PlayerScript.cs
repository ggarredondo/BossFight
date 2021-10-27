using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public Transform cam;
    public Animator anim;
    public CharacterController controller;

    public float walk_min = 0f, walk_max = 0.5f;
    public float turn_smoothness = 0.2f;
    public float jump_height = 2f;
    public float jump_cooldown = 1f;
    public float gravity = 11.81f;

    private float horizontal, vertical, move_magnitude,
        turn_smooth_velocity, target_angle, rotation_angle; // unlocked movement variables
    private Vector3 direction, move_dir;
    private float dist_to_ground, jump_time; // jumping variables
    private Vector3 jump_dir;
    private bool is_moving, is_walking, is_sprinting, is_jumping, is_grounded, is_locked; // animator variables

    private void Start() {
        anim = GetComponent<Animator>();
        dist_to_ground = controller.bounds.extents.y;
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
        is_moving = direction.magnitude > 0f;
        if (is_moving)
            transform.rotation = Quaternion.Euler(0f, rotation_angle, 0f);
        move_dir = (Quaternion.Euler(0f, target_angle, 0f) * Vector3.forward).normalized; // movement relative to the camera

        // base movement phases: walking, running and sprinting
        is_walking = move_magnitude >= walk_min && move_magnitude < walk_max && is_moving && !is_sprinting;
        is_sprinting = Input.GetButton("Sprint") && is_moving && !is_walking;

        // final movement
        controller.Move(move_dir * direction.magnitude * Time.deltaTime);
    }

    private void Jump()
    {
        // jumping (cooldown between jumps) and falling
        is_grounded = IsGrounded();
        is_jumping = Input.GetButtonDown("Jump") && is_grounded && jump_time <= Time.time;
        jump_dir = Vector3.zero;
        if (is_jumping)
        {
            jump_time = Time.time + jump_cooldown;
            jump_dir.y = jump_height;
        }
        else if (!is_grounded)
            jump_dir += Physics.gravity * Time.deltaTime;
        Debug.Log("button: " + Input.GetButtonDown("Jump"));
        Debug.Log("is_grounded: " + is_grounded);
        Debug.Log("jump_time <= Time.time: " + (jump_time <= Time.time));
        Debug.Log("jump_dir: " + jump_dir);
        controller.Move(jump_dir * Time.deltaTime);
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
        anim.SetBool("is_grounded", is_grounded);
    }

    void Update()
    {
        UnlockedMovement();
        Jump();
        Animation();
    }
}
