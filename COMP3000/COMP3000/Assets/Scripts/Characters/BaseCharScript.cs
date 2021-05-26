using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class BaseCharScript : MonoBehaviour
{




    public virtual void takeDamage(float damage, string whoShot)
    {

    }

    public virtual void takeHeal(float damage)
    {

    }

    public virtual float getHealth()
    {
        return 0;
    }
}
