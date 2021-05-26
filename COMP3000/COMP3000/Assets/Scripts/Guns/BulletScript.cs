using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    private float damage;
    private float velocity;
    private float size;
    private GameObject shooter;

    public void newBullet(float newVelocity, float newDamage, float newSize, GameObject newShooter)
    {
        velocity = newVelocity;
        damage = newDamage;
        size = newSize;
        shooter = newShooter;
        gameObject.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        gameObject.GetComponent<Rigidbody>().AddForce(transform.forward * velocity);
        gameObject.transform.localScale += new Vector3(0f, 0f, size);
        StartCoroutine(BulletDeletion());
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Player") || collision.transform.CompareTag("AI"))
        {
            collision.gameObject.GetComponent<BaseCharScript>().takeDamage(damage, shooter.tag);
        }
        Destroy(this.gameObject);
    }

    IEnumerator BulletDeletion()
    {
        yield return new WaitForSeconds(10);
        Destroy(this.gameObject);
    }
}
