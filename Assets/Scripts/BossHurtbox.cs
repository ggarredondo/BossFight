using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossHurtbox : MonoBehaviour
{
    public BossScript boss;

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "Player Sword Hitbox" && !boss.is_hurt)
            boss.is_hurt = true;
        else if (other.name == "Bash Hitbox" && !boss.is_hurt)
            boss.is_bashed = true;
    }
}
