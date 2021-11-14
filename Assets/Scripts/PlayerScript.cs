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
    public float jump_height = 4f, dodge_cooldown = 1f, jump_cooldown = 1.2f;

    // movement variables
    private float horizontal, vertical, move_magnitude, turn_smooth_velocity, target_angle, rotation_angle,
        dist_to_ground, dodge_time;
    private Vector3 direction, move_dir, height_dir;
    private bool is_moving, is_walking, is_sprinting, press_dodge, press_jump, is_grounded, is_locked, press_block; // animator variables
    private bool no_movement; // variable for situations where I don't want the character to be able to move

    private void Start() {
        anim = GetComponent<Animator>();
        dist_to_ground = controller.bounds.extents.y;
        is_locked = false;
        height_dir = Vector3.zero;
        dodge_time = 0f;
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
        if (is_moving && !no_movement)
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
        press_dodge = Input.GetButtonDown("Dodge") && is_grounded && dodge_time <= Time.time && !anim.GetCurrentAnimatorStateInfo(0).IsName("Parrying");
        press_jump = press_dodge && !is_moving;
        if (press_jump) {
            height_dir.y += jump_height;
            dodge_time = jump_cooldown + Time.time;
        }
        else if (press_dodge)
            dodge_time = dodge_cooldown + Time.time;
    }

    private void Fall()
    {
        anim.applyRootMotion = is_grounded;
        if (!is_grounded)
            height_dir += Physics.gravity * Time.deltaTime;
        else if (height_dir.y < 0f)
            height_dir.y = 0f;
    }

    public float atk_force, atk_side, atk_height; // temp
    private void Animation()
    {
        anim.SetFloat("speed", speed);
        anim.SetFloat("horizontal", horizontal);
        anim.SetFloat("vertical", vertical);
        anim.SetBool("is_locked", is_locked);
        anim.SetBool("is_moving", is_moving);
        anim.SetBool("is_walking", is_walking);
        anim.SetBool("is_sprinting", is_sprinting);
        anim.SetBool("press_dodge", press_dodge);
        anim.SetBool("press_jump", press_jump);
        anim.SetBool("is_grounded", is_grounded);
        anim.SetBool("press_block", press_block);

        anim.SetFloat("atk_force", atk_force); // temp
        anim.SetFloat("atk_side", atk_side); // temp
        anim.SetFloat("atk_height", atk_height); // temp
    }

    void Update()
    {
        is_grounded = IsGrounded();
        no_movement = anim.GetCurrentAnimatorStateInfo(0).IsName("Unlocked.Sprinting Stop") || 
            anim.GetCurrentAnimatorStateInfo(0).IsName("Landing") || anim.GetCurrentAnimatorStateInfo(0).IsName("Rolling")
            || anim.GetCurrentAnimatorStateInfo(0).IsName("Parrying");
        press_block = is_grounded && Input.GetButtonDown("Block") && !anim.GetCurrentAnimatorStateInfo(0).IsName("Rolling");

        // basic input
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        direction.Set(horizontal, 0f, vertical);

        UnlockedMovement();
        Dodge();
        Fall();
        Animation();

        // final movement
        controller.Move((move_dir * direction.magnitude * System.Convert.ToSingle(!no_movement) + height_dir) * Time.deltaTime);
    }
}
