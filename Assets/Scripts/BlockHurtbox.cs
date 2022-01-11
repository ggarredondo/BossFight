using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockHurtbox : MonoBehaviour
{
    public PlayerScript player;
    public BossScript boss;

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "Boss Sword Hitbox") {
            player.parry_late = true;
            boss.is_parried = true;
        }
    }
}
