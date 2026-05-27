using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Combat
{
    public class PlayerRotation
    {
        public void RotationSlerp(Transform transform, Vector3 direction, float velocity)
        {
            if((direction.normalized - transform.forward).sqrMagnitude < 0.0001f) return;
            direction.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * velocity);
        }

    }
}


