using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TimedRandom
{
    private float timer, _value;
    public float cooldown = 0f;

    public float value { 
        get {
            if (timer <= Time.time) {
                _value = Random.value;
                timer = Time.time + cooldown;
            }
            return _value;
        }
    }

    public float one_use_value { 
        get {
            float new_value = value;
            _value = 0;
            return new_value;
        }
    }

    public float integer {
        get {
            return Mathf.Round(value);
        }
    }

    public float Range(float minInclusive, float maxInclusive) {
        if (timer <= Time.time) {
            _value = Random.Range(minInclusive, maxInclusive);
            timer = Time.time + cooldown;
        }
        return _value;
    }
}
