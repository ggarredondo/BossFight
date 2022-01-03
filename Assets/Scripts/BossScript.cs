using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossScript : MonoBehaviour
{
    public Transform player_pos;
    public float turn_smoothness = 0.15f, combat_distance, follow_distance;

    Animator anim;
    float distance, target_angle, rotation_angle, turn_smooth_velocity;
    bool no_movement;

    private void Start()
    {
        anim = GetComponent<Animator>();
        no_movement = false;
    }

    private void Movement() 
    {
        target_angle = Quaternion.LookRotation(player_pos.transform.position - transform.position).eulerAngles.y;
        rotation_angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, target_angle, ref turn_smooth_velocity, turn_smoothness);
        if (!no_movement)
            transform.rotation = Quaternion.Euler(0f, rotation_angle, 0f);
    }

    private void Animation() 
    {
        anim.SetBool("out_of_combat", distance >= combat_distance);
        anim.SetBool("is_following", false);
    }

    void Update()
    {
        no_movement = anim.GetCurrentAnimatorStateInfo(0).IsName("Noncombat Idle") || anim.GetCurrentAnimatorStateInfo(0).IsName("Equip")
            || anim.GetCurrentAnimatorStateInfo(0).IsName("Unequip") || anim.GetCurrentAnimatorStateInfo(0).IsName("Combat Idle");
        distance = Vector3.Distance(transform.position, player_pos.transform.position);
        Debug.Log(distance);

        Animation();
        Movement();
    }
}
