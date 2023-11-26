using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum BulletType
{
    Player,
    Enemy
}
public class Bullet : MonoBehaviour
{
    public float speed;
    //調難度
    public bool canTrackEnemy = false;
    public BulletType bulletType;
    void Update()
    {
        if ((GameManager.Instance.canTrack||canTrackEnemy) && bulletType == BulletType.Player)
            Track();
        else
            Move();
    }
    protected void Move()
    {
        transform.Translate(Vector3.up * speed * Time.deltaTime, Space.Self);
    }
    void Track()
    {
        var enemy = GameObject.FindWithTag("Enemy");
        if (enemy)
            gameObject.transform.position = Vector3.MoveTowards(this.gameObject.transform.position, enemy.transform.position, speed * Time.deltaTime);
        else
            transform.Translate(Vector3.up * speed * Time.deltaTime, Space.Self);
    }
    void Die()
    {
        Destroy(this.gameObject);
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<Death>() && other.gameObject.tag != bulletType.GetType().GetEnumName(bulletType))
        {
            other.gameObject.GetComponent<Death>().Hurt();
            Die();
        }
        if (other.gameObject.tag == "Barrier")
        {
            Die();
        }
    }
}
