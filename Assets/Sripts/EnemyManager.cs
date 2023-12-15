using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Wave
{
    OneColumn,
    TwoColumn,
    WholeRow
}
[System.Serializable]
public struct WaveMonster
{
    public GameObject monsterPrefab;
    public int count;
    public float spanTime;
    public Wave wave;
    public Transform spanPosition;
    public Transform[] movePosition;
}
[System.Serializable]
public struct WaveBoss
{
    public GameObject bossPrefab;
    public Transform spanPosition;
    public Transform[] movePosition;
}
public class EnemyManager : MonoBehaviour
{
    int nowIndex = 0;
    int nowCount = 0;
    int allIndex = 0;
    int bossIndex = 0;
    bool isInBossAttack = false;
    bool isSpanBoss = false;
    List<GameObject> waveEnemy = new List<GameObject>();
    GameObject tempEnemy;
    public int WaveToBoss;
    public WaveMonster[] waveMonster;
    public WaveBoss[] waveBosses;
    //0 左上 1 右下
    public GameObject[] mapPosition;
    public IEnumerator CreateEnemy()
    {
        while (true)
        {
            if (FindObjectOfType<Player>() && nowCount < waveMonster[nowIndex].count && !isInBossAttack)
            {
                for (int i = 0; i < waveMonster[nowIndex].count; i++)
                {
                    string nowWave = System.Enum.GetName(typeof(Wave), waveMonster[nowIndex].wave);
                    tempEnemy = Instantiate(waveMonster[nowIndex].monsterPrefab, waveMonster[nowIndex].spanPosition.position, Quaternion.identity);
                    for (int j = 0; j < waveMonster[nowIndex].movePosition.Length; j++)
                    {
                        tempEnemy.GetComponent<Enemy>().Dot[j] = waveMonster[nowIndex].movePosition[j].position;
                    }
                    nowCount++;
                    StartCoroutine(nowWave);
                    waveEnemy.Add(tempEnemy);
                    GameManager.Instance.ChangeDifficulty(tempEnemy);
                    yield return new WaitForSeconds(waveMonster[nowIndex].spanTime);
                }
            }
            else if (isInBossAttack)
            {
                isSpanBoss = true;
                isInBossAttack= false;
                tempEnemy = Instantiate(waveBosses[bossIndex].bossPrefab, waveBosses[bossIndex].spanPosition.position, Quaternion.identity);
                for (int i = 0; i < waveBosses[bossIndex].movePosition.Length; i++)
                {
                    tempEnemy.GetComponent<Enemy>().Dot[i] = waveBosses[bossIndex].movePosition[i].position;
                }
                bossIndex++;
                nowCount = waveMonster[nowIndex].count + 1;
                waveEnemy.Add(tempEnemy);
                GameManager.Instance.ChangeDifficulty(tempEnemy);
                yield return new WaitForSeconds(0.1f);
            }
            else
            {
                bool allEnemyDie = true;
                if (nowCount >= waveMonster[nowIndex].count)
                {
                    for (int i = 0; i < waveEnemy.Count; i++)
                    {
                        if (waveEnemy[i] != null)
                        {
                            allEnemyDie = false;
                            continue;
                        }
                    }
                }
                if (allEnemyDie)
                {
                    nowCount = 0;
                    if (!isSpanBoss)
                        nowIndex++;
                    else
                        isSpanBoss = false;
                    allIndex++;
                    if (allIndex % (WaveToBoss + 1) == WaveToBoss)
                        isInBossAttack = true;
                    if (nowIndex >= waveMonster.Length)
                        nowIndex = 0;
                    waveEnemy.Clear();
                    if(bossIndex>=waveBosses.Length)
                    {
                        break;
                    }
                }
                yield return null;
            }
        }
    }
    void OneColumn() { }
    void TwoColumn()
    {
        for (int i = 0; i < waveMonster[nowIndex].movePosition.Length; i++)
        {
            float tempPosition = (mapPosition[0].transform.position.x + mapPosition[1].transform.position.x) / 2 - waveMonster[nowIndex].movePosition[i].position.x;
            waveMonster[nowIndex].movePosition[i].position = new Vector3(tempPosition, waveMonster[nowIndex].movePosition[i].position.y, waveMonster[nowIndex].movePosition[i].position.z);
        }
        float tempX = (mapPosition[0].transform.position.x + mapPosition[1].transform.position.x) / 2 - waveMonster[nowIndex].spanPosition.position.x;
        waveMonster[nowIndex].spanPosition.position = new Vector3(tempX, waveMonster[nowIndex].spanPosition.position.y, waveMonster[nowIndex].spanPosition.position.z);
    }
    void WholeRow()
    {
        float distance = 0.4f;
        if (waveMonster[nowIndex].spanPosition.position.x >= 0)
            distance *= -1;
        float tempX = waveMonster[nowIndex].spanPosition.position.x + distance;
        waveMonster[nowIndex].spanPosition.position = new Vector3(tempX, waveMonster[nowIndex].spanPosition.position.y, waveMonster[nowIndex].spanPosition.position.z);
        for (int i = 0; i < waveMonster[nowIndex].movePosition.Length; i++)
        {
            float tempPosition = waveMonster[nowIndex].movePosition[i].position.x + distance;
            waveMonster[nowIndex].movePosition[i].position = new Vector3(tempPosition, waveMonster[nowIndex].movePosition[i].position.y, waveMonster[nowIndex].movePosition[i].position.z);
        }
    }
}
