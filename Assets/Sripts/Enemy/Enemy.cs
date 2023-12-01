using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BarrageType
{
    Shotgun,
    TrackShotgun,
    CircleBarrage,
}
public enum AttackType
{
    useBarrage,
    suicideAttack,
    nonAttack
}
public enum MoveType
{
    MoveToTarget,
    SomeTimesMove,
    StayAttackMove,
    ToPlayerMove,
}
[System.Serializable]
public struct EnemyBarrageCount
{
    //0 生成的彈幕個數,1 生成的彈幕波數
    public int[] count;
    public BarrageType barrageType;
}
public class Enemy : MonoBehaviour
{
    #region "private"
    int nowCountBarrage = 0;
    Vector3 targetPosition;
    Coroutine coroutine;
    bool canChooseBarrage = true;
    bool canAttack = false;
    int nowIndex = 0;
    #endregion
    #region  "public"
    public AttackType useBarrage;
    public MoveType moveType;
    public GameObject[] bullet;
    public Transform bulletTransform;
    public float Speed;
    public bool deadUseBarrage;
    #endregion
    #region "Hide"
    //[HideInInspector]
    public Transform[] Dot;
    [HideInInspector]
    public List<GameObject> Allbullet = new List<GameObject>();
    #endregion
    #region "調難度"
    [Header("調難度")]
    public EnemyBarrageCount[] enemyBarrageCounts;
    public float countTime;
    public int indexMax = 1;
    #endregion
    void Start()
    {
        if (Dot.Length != 0)
            targetPosition = Dot[0].position;
    }
    void Update()
    {
        Move();
    }
    void ReturnMove()
    {
        for (int i = 0; i < Dot.Length; i++)
        {
            if (targetPosition == Dot[i].position)
            {
                if (i == Dot.Length - 1)
                    targetPosition = Dot[0].position;
                else
                    targetPosition = Dot[i + 1].position;
                break;
            }
        }
    }
    void changeBarrage()
    {
        if (nowIndex >= indexMax-1)
            nowIndex = 0;
        else
            nowIndex++;
    }
    void Move()
    {
        if (!canAttack)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Speed * Time.deltaTime);
            if (transform.position == targetPosition)
            {
                ReturnMove();
                canAttack = true;
                canChooseBarrage = false;
                if (useBarrage == AttackType.useBarrage)
                    Attack();
            }
        }
        else
        {
            switch (moveType)
            {
                case MoveType.SomeTimesMove:
                    if (transform.position == targetPosition && canChooseBarrage)
                    {
                        canChooseBarrage = false;
                        ReturnMove();
                        Attack();
                    }
                    if (canChooseBarrage)
                        transform.position = Vector3.MoveTowards(transform.position, targetPosition, Speed * Time.deltaTime);
                    break;
                case MoveType.StayAttackMove:
                    if (transform.position != targetPosition)
                        transform.position = Vector3.MoveTowards(transform.position, targetPosition, Speed * Time.deltaTime);
                    else
                        ReturnMove();
                    break;
                case MoveType.ToPlayerMove:
                    if (FindObjectOfType<Player>())
                    {
                        var temp = FindObjectOfType<Player>().gameObject;
                        transform.position = Vector3.MoveTowards(transform.position, temp.transform.position, Speed * Time.deltaTime);
                    }
                    break;
            }
        }
    }
    public void Attack()
    {
        coroutine = StartCoroutine(UseBarrage());
    }
    IEnumerator UseBarrage()
    {
        while (FindObjectOfType<Player>() && !canChooseBarrage)
        {
            string nowBarrage = System.Enum.GetName(typeof(BarrageType), enemyBarrageCounts[nowIndex].barrageType);
            StartCoroutine(nowBarrage, enemyBarrageCounts[nowIndex].count);
            if (indexMax >= 1)
            {
                nowCountBarrage += 1;
                if (nowCountBarrage >= enemyBarrageCounts[nowIndex].count[1])
                {
                    nowCountBarrage = 0;
                    changeBarrage();
                    if (moveType == MoveType.SomeTimesMove)
                        canChooseBarrage = true;
                }
            }
            yield return new WaitForSeconds(countTime);
        }
    }
    #region "所有彈幕方法"
    void Shotgun(int[] count)
    {
        float angle = Random.Range(90, 220);
        for (int j = 0; j < count[0]; j++)
        {
            GameObject temp = Instantiate(bullet[0], bulletTransform.position, Quaternion.Euler(0, 0, angle));
            Allbullet.Add(temp);
            angle += 12;
        }
    }
    void TrackShotgun(int[] count)
    {
        if (FindObjectOfType<Player>())
        {
            var player = FindObjectOfType<Player>();
            Vector3 eulerAngle = GetAngle(transform.position, player.transform.position);
            eulerAngle.z -= 24;
            for (int j = 0; j < count[0]; j++)
            {
                GameObject temp = Instantiate(bullet[0], bulletTransform.position, Quaternion.Euler(0, 0, eulerAngle.z));
                Allbullet.Add(temp);
                eulerAngle.z += 12;
            }
        }
    }
    void CircleBarrage(int[] count)
    {
        int indexz = 0;
        for (int j = 0; j <= count[0]; j++)
        {
            indexz += 360 / count[0];
            GameObject temp = Instantiate(bullet[0], bulletTransform.position, Quaternion.Euler(0, 0, indexz));
            Allbullet.Add(temp);
        }
    }
    Vector3 GetAngle(Vector3 aPoint, Vector3 bPoint)
    {
        Vector3 direct = bPoint - aPoint;
        Vector3 normal = Vector3.Cross(Vector2.up, direct.normalized);
        float zAngle = Vector2.Angle(Vector2.up, direct.normalized);
        zAngle = normal.z > 0 ? zAngle : -zAngle;
        return new Vector3(0, 0, zAngle);
    }
    #endregion
}
