using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossScript : MonoBehaviour
{
    public Transform player_pos;
    public float turn_smoothness, walk_smoothness, critical_distance, combat_distance, follow_distance;
    public TimedRandom strafe, straight;

    Animator anim;
    float horizontal = 1f, target_horizontal, vertical = 0f, target_vertical, distance, target_angle, 
        rotation_angle, turn_smooth_velocity, horizontal_smooth_velocity, vertical_smooth_velocity;
    bool is_moving = false, is_walking = false, is_following = false;

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
        if (is_moving)
            transform.rotation = Quaternion.Euler(0f, rotation_angle, 0f);

        // Random movement at combat distance
        target_horizontal = strafe.value > 0.5f ? 1f : -1f;
        horizontal = Mathf.SmoothStep(horizontal, target_horizontal, walk_smoothness * Time.deltaTime);
        target_vertical = Mathf.Round(straight.Range(-1f, 1f));
        vertical = Mathf.SmoothStep(vertical, target_vertical, walk_smoothness * Time.deltaTime);
    }

    private void Animation() 
    {
        anim.SetBool("out_of_combat", distance > follow_distance);

        is_following = distance >= combat_distance && distance <= follow_distance;
        anim.SetBool("is_following", is_following);

        is_walking = distance <= combat_distance;
        anim.SetBool("is_walking", is_walking);
        anim.SetFloat("horizontal", horizontal);
        anim.SetFloat("vertical", vertical);
    }

    void FixedUpdate()
    {
        distance = Vector3.Distance(transform.position, player_pos.transform.position);
        Debug.Log(distance); // <------------- delete

        Animation();
        Movement();
    }
}
