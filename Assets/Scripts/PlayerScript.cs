using Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerScript : MonoBehaviour
{
    public Transform cam, boss_pos;
    public Animator anim;
    public CharacterController controller;
    public CinemachineVirtualCamera LockOnCamera;
    public BossScript boss;
    public GameObject SwordHitbox, BlockHurtbox, BashHitbox, UpperbodyHurtbox, LowerbodyHurtbox;

    public float speed = 1f, walk_range_min = 0f, walk_range_max = 0.5f;
    public float turn_smoothness = 0.14f;
    public float jump_height = 4f;
    public float atk_side;
    public float atk_time_end = 0.4f, parry_window = 0.3f, death_time = 2f;
    public bool parry_late = false, is_hurt = false, is_hurt_legs = false, god_mode = false;

    // movement variables
    private float horizontal, vertical, move_magnitude, turn_smooth_velocity, target_angle, rotation_angle,
        dist_to_ground, boss_defend_rng;
    private Vector3 direction, height_dir;
    private bool is_moving, is_walking, is_sprinting, is_dodge_pressed, is_jump_pressed, is_grounded, is_locked, is_block_pressed,
        is_dodging, is_blocking, is_jumping, is_landing, is_attacking, is_attack1_pressed, is_attack2_pressed, is_bashing, is_atk_ended; // animator variables
    private bool no_movement; // variable for situations where I don't want the character to be able to move

    private void Start() {
        anim = GetComponent<Animator>();
        dist_to_ground = controller.bounds.extents.y;
        is_locked = false;
        height_dir = Vector3.zero;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
    }

    private bool IsGrounded() {
        return Physics.Raycast(transform.position, -Vector3.up, dist_to_ground + 0.1f);
    }

    private void Movement()
    {
        move_magnitude = direction.sqrMagnitude;
        direction = direction.normalized;

        // character faces the direction it's moving to
        target_angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
        if (is_locked && !is_sprinting && !is_dodge_pressed)
            target_angle = Quaternion.LookRotation(boss_pos.transform.position - transform.position).eulerAngles.y;
        rotation_angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, target_angle, ref turn_smooth_velocity, turn_smoothness *
            System.Convert.ToSingle(!is_dodge_pressed) + 0.01f);
        is_moving = direction.magnitude > 0f;
        if ((is_moving && !no_movement) || (is_attacking && is_locked))
            transform.rotation = Quaternion.Euler(0f, rotation_angle, 0f);

        // base movement phases: walking, running (default) and sprinting
        is_walking = move_magnitude >= walk_range_min && move_magnitude < walk_range_max && is_moving && !is_sprinting;
        if (Input.GetButtonDown("Sprint")) // toggle sprint
            is_sprinting = !is_sprinting;
        is_sprinting = is_sprinting && is_moving && !is_walking && !anim.GetCurrentAnimatorStateInfo(0).IsName("Unlocked.Sprint Bash");
    }

    private void Fall()
    {
        anim.applyRootMotion = is_grounded;
        if (!is_grounded)
            height_dir += Physics.gravity * Time.deltaTime;
        else if (height_dir.y < 0f)
            height_dir.y = 0f;
    }

    bool rt_in_use = false;
    private bool UseRT() {
        bool use = false;
        if (rt_in_use)
            rt_in_use = Input.GetAxis("Attack2") == 1;
        else {
            rt_in_use = Input.GetAxis("Attack2") == 1;
            use = rt_in_use;
        }
        return use;
    }

    private void Action()
    {
        is_dodge_pressed = Input.GetButtonDown("Dodge") && is_grounded && !is_dodging && !is_blocking && !is_jumping && !is_landing && !is_hurt && !is_hurt_legs;
        is_jump_pressed = Input.GetButtonDown("Jump") && is_grounded && !is_dodging && !is_blocking && !is_jumping && !is_hurt && !is_hurt_legs;
        if (is_jump_pressed)
            height_dir.y += jump_height;
        is_block_pressed = is_grounded && Input.GetButtonDown("Block") && !is_dodging && !is_blocking && !is_jumping && !is_hurt && !is_hurt_legs;
        is_attack1_pressed = Input.GetButtonDown("Attack1") && !is_dodging && !is_hurt && !is_hurt_legs;
        is_attack2_pressed = UseRT() && !is_dodging && !is_hurt && !is_hurt_legs;
        if (Input.GetButtonDown("LockOn")) { // toggle lock on
            is_locked = !is_locked;
            LockOnCamera.Priority = System.Convert.ToInt32(is_locked);
        }
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
        anim.SetBool("is_dodge_pressed", is_dodge_pressed);
        anim.SetBool("is_dodging", is_dodging);
        anim.SetBool("is_jump_pressed", is_jump_pressed);
        anim.SetBool("is_jumping", is_jumping);
        anim.SetBool("is_landing", is_landing);
        anim.SetBool("is_grounded", is_grounded);
        anim.SetBool("is_block_pressed", is_block_pressed);
        anim.SetBool("is_blocking", is_blocking);
        anim.SetBool("is_attack1_pressed", is_attack1_pressed);
        anim.SetBool("is_attack2_pressed", is_attack2_pressed);
        anim.SetBool("is_attacking", is_attacking);
        anim.SetFloat("atk_side", atk_side);
        anim.SetBool("parry_late", parry_late);
        parry_late = false;
        anim.SetBool("is_locked", is_locked);

        anim.SetBool("is_hurt", is_hurt);
        anim.SetBool("is_hurt_legs", is_hurt_legs);
    }

    private void Hitbox() {
        is_bashing = anim.GetCurrentAnimatorStateInfo(0).IsName("Unlocked.Sprint Bash");
        is_atk_ended = anim.GetCurrentAnimatorStateInfo(0).normalizedTime > atk_time_end;
        UpperbodyHurtbox.SetActive(!is_dodging && !BlockHurtbox.activeInHierarchy);
        LowerbodyHurtbox.SetActive(!is_dodging && !is_jumping);
        SwordHitbox.SetActive(is_attacking && !is_bashing && !is_atk_ended);
        BashHitbox.SetActive(is_bashing && !is_atk_ended);
        BlockHurtbox.SetActive(is_blocking && anim.GetCurrentAnimatorStateInfo(0).normalizedTime <= parry_window);
    }

    void Update()
    {
        is_grounded = IsGrounded();
        is_dodging = anim.GetCurrentAnimatorStateInfo(0).IsName("Rolling");
        is_blocking = anim.GetCurrentAnimatorStateInfo(0).IsName("Parrying.Parrying Base") ||
            anim.GetCurrentAnimatorStateInfo(0).IsName("Parrying.Parrying Success") ||
            anim.GetCurrentAnimatorStateInfo(0).IsName("Parrying.Parrying Late");
        is_jumping = anim.GetCurrentAnimatorStateInfo(0).IsName("Jumping");
        is_landing = anim.GetCurrentAnimatorStateInfo(0).IsName("Landing") || anim.GetCurrentAnimatorStateInfo(0).IsName("Jump Attack Landing");
        is_attacking = anim.GetCurrentAnimatorStateInfo(0).IsName("Attacking.Attack2_combo1") || anim.GetCurrentAnimatorStateInfo(0).IsName("Attacking.Attack2_combo2")
            || anim.GetCurrentAnimatorStateInfo(0).IsName("Attacking.Attack2_combo3") || anim.GetCurrentAnimatorStateInfo(0).IsName("Attacking.Attack1_combo1") ||
            anim.GetCurrentAnimatorStateInfo(0).IsName("Attacking.Attack1_combo2") || anim.GetCurrentAnimatorStateInfo(0).IsName("Attacking.Attack1_combo3") || 
            anim.GetCurrentAnimatorStateInfo(0).IsName("Unlocked.Sprint Bash") || anim.GetCurrentAnimatorStateInfo(0).IsName("Jump Attack") 
            || anim.GetCurrentAnimatorStateInfo(0).IsName("Parrying.Riposte");
        no_movement = anim.GetCurrentAnimatorStateInfo(0).IsName("Unlocked.Sprinting Stop")  || is_dodging || is_blocking || is_landing || is_attacking || is_hurt || is_hurt_legs;
        if (is_attack1_pressed || is_attack2_pressed)
            boss_defend_rng = Random.value;
        boss.defend = is_attacking && anim.GetCurrentAnimatorStateInfo(0).normalizedTime <= atk_time_end && boss_defend_rng <= boss.defend_chance;

        // basic input
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        direction.Set(horizontal, 0f, vertical);

        Action();
        Movement();
        Fall();
        Animation();
        Hitbox();

        // final movement
        controller.Move(height_dir * Time.deltaTime);

        if ((anim.GetCurrentAnimatorStateInfo(0).IsName("Death Normal") || anim.GetCurrentAnimatorStateInfo(0).IsName("Death Legs")) 
            && anim.GetCurrentAnimatorStateInfo(0).normalizedTime > death_time)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
