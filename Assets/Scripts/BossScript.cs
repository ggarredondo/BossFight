using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossScript : MonoBehaviour
{
    public Transform player_pos;
    public PlayerScript player;
    public float turn_smoothness, walk_smoothness, critical_distance, combat_distance, follow_distance, attack_speed, defend_chance, atk_time_end = 0.4f;
    public TimedRandom horizontal_rng, vertical_rng, attack_rng;
    public bool defend = false, is_hurt = false, is_bashed = false, is_parried = false;
    public GameObject SwordHitbox, UpperbodyHurtbox, LowerbodyHurtbox;

    private Animator anim;
    private float horizontal = 1f, target_horizontal, vertical = 0f, target_vertical, distance, target_angle,
        rotation_angle, turn_smooth_velocity;
    private bool is_moving = false, is_walking = false, is_following = false, is_attacking, on_ground = false, is_being_parried;
    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void Movement() 
    {
        // Rotate the boss towards the player
        target_angle = Quaternion.LookRotation(player_pos.transform.position - transform.position).eulerAngles.y;
        rotation_angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, target_angle, ref turn_smooth_velocity, turn_smoothness);
        is_moving = is_following || is_walking;
        if (is_moving && !is_hurt)
            transform.rotation = Quaternion.Euler(0f, rotation_angle, 0f);

        // Random movement at combat distance
        target_horizontal = horizontal_rng.value > 0.5f ? 1f : -1f;
        horizontal = Mathf.SmoothStep(horizontal, target_horizontal, walk_smoothness * Time.deltaTime);
        target_vertical = Mathf.Round(vertical_rng.Range(-1f, 1f));
        vertical = Mathf.SmoothStep(vertical, target_vertical, walk_smoothness * Time.deltaTime);
    }

    private void Animation() 
    {
        on_ground = anim.GetCurrentAnimatorStateInfo(0).IsName("Bashed");
        is_being_parried = anim.GetCurrentAnimatorStateInfo(0).IsName("Parried") || anim.GetCurrentAnimatorStateInfo(0).IsName("Recovery");

        anim.SetBool("out_of_combat", distance > follow_distance || player.is_hurt || player.is_hurt_legs);
        anim.SetBool("in_critical_distance", distance <= critical_distance);

        is_following = distance >= combat_distance && distance <= follow_distance;
        anim.SetBool("is_following", is_following);

        is_walking = distance <= combat_distance;
        anim.SetBool("is_walking", is_walking);
        anim.SetFloat("horizontal", horizontal);
        anim.SetFloat("vertical", vertical);

        is_attacking = anim.GetCurrentAnimatorStateInfo(0).IsName("Combo 1 1") || anim.GetCurrentAnimatorStateInfo(0).IsName("Combo 1 2") ||
            anim.GetCurrentAnimatorStateInfo(0).IsName("Combo 1 3") || anim.GetCurrentAnimatorStateInfo(0).IsName("Combo 2 1") ||
            anim.GetCurrentAnimatorStateInfo(0).IsName("Combo 2 2") || anim.GetCurrentAnimatorStateInfo(0).IsName("Combo 2 3");
        
        anim.SetBool("attack", is_walking && attack_rng.one_use_value > 0.5f && !on_ground && !is_being_parried);
        anim.SetBool("is_attacking", is_attacking);
        anim.SetFloat("attack_speed", attack_speed);

        anim.SetBool("defend", defend && !on_ground);

        anim.SetBool("is_bashed", is_bashed);
        is_bashed = false;
        anim.SetBool("on_ground", on_ground);

        anim.SetBool("is_hurt", is_hurt);

        anim.SetBool("is_parried", is_parried);
        is_parried = false;
    }

    private void Hitbox()
    {
        UpperbodyHurtbox.SetActive(!defend);
        LowerbodyHurtbox.SetActive(!defend);
        SwordHitbox.SetActive(is_attacking && anim.GetCurrentAnimatorStateInfo(0).normalizedTime <= atk_time_end);
    }

    void FixedUpdate()
    {
        distance = Vector3.Distance(transform.position, player_pos.transform.position);
        Debug.Log(distance);

        Animation();
        Movement();
        Hitbox();
    }
}
