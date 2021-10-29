using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public Transform cam;
    public Animator anim;
    public CharacterController controller;

    public float speed = 1f, walk_range_min = 0f, walk_range_max = 0.5f;
    public float turn_smoothness = 0.2f;
    public float jump_height = 4f, jump_cooldown = 1f;

    // movement variables
    private float horizontal, vertical, move_magnitude, turn_smooth_velocity, target_angle, rotation_angle,
        dist_to_ground, jump_time;
    private Vector3 direction, move_dir, height_dir;
    private bool is_moving, is_walking, is_sprinting, is_dodging, is_jumping, is_grounded, is_locked; // animator variables

    private void Start() {
        anim = GetComponent<Animator>();
        dist_to_ground = controller.bounds.extents.y;
        is_locked = false;
        height_dir = Vector3.zero;
        jump_time = 0f;
    }

    private bool IsGrounded() {
        return Physics.Raycast(transform.position, -Vector3.up, dist_to_ground + 0.1f);
    }

    private void UnlockedMovement()
    {
        move_magnitude = direction.sqrMagnitude;
        direction = direction.normalized;

        // character faces the direction it's moving to
        target_angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
        rotation_angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, target_angle, ref turn_smooth_velocity, turn_smoothness);
        is_moving = direction.magnitude > 0f;
        if (is_moving)
            transform.rotation = Quaternion.Euler(0f, rotation_angle, 0f);
        move_dir = (Quaternion.Euler(0f, target_angle, 0f) * Vector3.forward).normalized; // movement relative to the camera

        // base movement phases: walking, running (default) and sprinting
        is_walking = move_magnitude >= walk_range_min && move_magnitude < walk_range_max && is_moving && !is_sprinting;
        if (Input.GetButtonDown("Sprint")) // toggle sprint
            is_sprinting = !is_sprinting;
        is_sprinting = is_sprinting && is_moving && !is_walking;
    }

    private void Dodge() // directional dodge is dash, no direction is jump
    {
        is_dodging = Input.GetButtonDown("Dodge") && is_grounded;
        is_jumping = is_dodging && !is_moving && jump_time <= Time.time;
        if (is_jumping) {
            height_dir.y += jump_height;
            jump_time = jump_cooldown + Time.time;
        }
    }

    private void Fall()
    {
        anim.applyRootMotion = is_grounded;
        if (!is_grounded)
            height_dir += Physics.gravity * Time.deltaTime;
        else if (height_dir.y < 0f)
            height_dir.y = 0f;
    }

    private void Animation()
    {
        anim.SetFloat("speed", speed);
        anim.SetFloat("horizontal", horizontal);
        anim.SetFloat("vertical", vertical);
        anim.SetBool("is_locked", is_locked);
        anim.SetBool("is_moving", is_moving);
        anim.SetBool("is_walking", is_walking);
        anim.SetBool("is_sprinting", is_sprinting);
        anim.SetBool("is_dodging", is_dodging);
        anim.SetBool("is_jumping", is_jumping);
        anim.SetBool("is_grounded", is_grounded);
    }

    void Update()
    {
        is_grounded = IsGrounded();

        // basic input
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        direction.Set(horizontal, 0f, vertical);

        UnlockedMovement();
        Dodge();
        Fall();
        Animation();

        // final movement
        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Landing"))
            controller.Move((move_dir * direction.magnitude + height_dir) * Time.deltaTime);
    }
}
