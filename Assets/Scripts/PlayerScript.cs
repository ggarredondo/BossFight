using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public Transform cam;
    public Animator anim;
    public CharacterController controller;

    public float speed = 1f, walk_range_min = 0f, walk_range_max = 0.5f;
    public float turn_smoothness = 0.14f;
    public float jump_height = 4f;

    // movement variables
    private float horizontal, vertical, move_magnitude, turn_smooth_velocity, target_angle, rotation_angle,
        dist_to_ground;
    private Vector3 direction, move_dir, height_dir;
    private bool is_moving, is_walking, is_sprinting, is_dodge_pressed, is_jump_pressed, is_grounded, is_locked, is_block_pressed,
        is_dodging, is_blocking, is_jumping, is_landing; // animator variables
    private bool no_movement; // variable for situations where I don't want the character to be able to move

    private void Start() {
        anim = GetComponent<Animator>();
        dist_to_ground = controller.bounds.extents.y;
        is_locked = false;
        height_dir = Vector3.zero;
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
        rotation_angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, target_angle, ref turn_smooth_velocity, turn_smoothness *
            System.Convert.ToSingle(!is_dodge_pressed) + 0.01f);
        is_moving = direction.magnitude > 0f;
        if (is_moving && !no_movement)
            transform.rotation = Quaternion.Euler(0f, rotation_angle, 0f);
        move_dir = (Quaternion.Euler(0f, target_angle, 0f) * Vector3.forward).normalized; // movement relative to the camera

        // base movement phases: walking, running (default) and sprinting
        is_walking = move_magnitude >= walk_range_min && move_magnitude < walk_range_max && is_moving && !is_sprinting;
        if (Input.GetButtonDown("Sprint")) // toggle sprint
            is_sprinting = !is_sprinting;
        is_sprinting = is_sprinting && is_moving && !is_walking;
    }

    private void Fall()
    {
        anim.applyRootMotion = is_grounded;
        if (!is_grounded)
            height_dir += Physics.gravity * Time.deltaTime;
        else if (height_dir.y < 0f)
            height_dir.y = 0f;
    }

    private void Action()
    {
        is_dodge_pressed = Input.GetButtonDown("Dodge") && is_grounded && !is_dodging && !is_blocking && !is_jumping && !is_landing;
        is_jump_pressed = Input.GetButtonDown("Dodge") && is_grounded && !is_dodging && !is_blocking && !is_jumping && !is_moving;
        if (is_jump_pressed)
            height_dir.y += jump_height;
        is_block_pressed = is_grounded && Input.GetButtonDown("Block") && !is_dodge_pressed && !is_dodging && !is_blocking && !is_jumping;
    }

    public float atk_side; // temp
    public bool parry_late; // temp
    private void Animation()
    {
        anim.SetFloat("speed", speed);
        anim.SetFloat("horizontal", horizontal);
        anim.SetFloat("vertical", vertical);
        anim.SetBool("is_locked", is_locked);
        anim.SetBool("is_moving", is_moving);
        anim.SetBool("is_walking", is_walking);
        anim.SetBool("is_sprinting", is_sprinting);
        anim.SetBool("is_dodge_pressed", is_dodge_pressed);
        anim.SetBool("is_jump_pressed", is_jump_pressed);
        anim.SetBool("is_grounded", is_grounded);
        anim.SetBool("is_block_pressed", is_block_pressed);

        anim.SetFloat("atk_side", atk_side); // temp
        anim.SetBool("parry_late", parry_late); // temp
    }

    void Update()
    {
        is_grounded = IsGrounded();
        is_dodging = anim.GetCurrentAnimatorStateInfo(0).IsName("Rolling");
        is_blocking = anim.GetCurrentAnimatorStateInfo(0).IsName("Parrying.Parrying Base") ||
            anim.GetCurrentAnimatorStateInfo(0).IsName("Parrying.Parrying Success") ||
            anim.GetCurrentAnimatorStateInfo(0).IsName("Parrying.Parrying Late");
        is_jumping = anim.GetCurrentAnimatorStateInfo(0).IsName("Jumping");
        is_landing = anim.GetCurrentAnimatorStateInfo(0).IsName("Landing");
        no_movement = anim.GetCurrentAnimatorStateInfo(0).IsName("Unlocked.Sprinting Stop")  || is_dodging || is_blocking || is_jumping
            || is_landing;

        // basic input
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        direction.Set(horizontal, 0f, vertical);

        Action();
        UnlockedMovement();
        Fall();
        Animation();

        // final movement
        controller.Move((move_dir * direction.magnitude * System.Convert.ToSingle(!no_movement) + height_dir) * Time.deltaTime);
    }
}
