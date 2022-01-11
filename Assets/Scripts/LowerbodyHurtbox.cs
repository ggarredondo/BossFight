using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LowerbodyHurtbox : MonoBehaviour
{
    public PlayerScript player;

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "Boss Sword Hitbox" && !player.is_hurt && !player.is_hurt_legs && !player.god_mode)
            player.is_hurt_legs = true;
    }
}
