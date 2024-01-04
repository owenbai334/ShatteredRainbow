using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    public GameObject[] bossPrefab;
    public Transform spanPosition;
    public Transform[] movePosition;
}
public class EnemyManager : MonoBehaviour
{
    #region "Private"
    int nowIndex = 0;
    int nowCount = 0;
    int allIndex = 0;
    bool isInBossAttack = false;
    bool isSpanBoss = false;
    bool OtherStage = true;
    List<GameObject> waveEnemy = new List<GameObject>();
    GameObject tempEnemy;
    #endregion
    #region "Hide"
    public Slider BossBar;
    [HideInInspector]
    public bool isWin = false;
    [HideInInspector]
    public int nowBossStage = 1;
    [HideInInspector]
    public int bossIndex = 0;
    [HideInInspector]
    //0 左上 1 右下
    public GameObject[] mapPosition;
    #endregion
    public WaveMonster[] waveMonster;
    public WaveBoss[] waveBosses;
    public IEnumerator CreateEnemy()
    {
        while (true)
        {
            if (!isInBossAttack && nowCount < waveMonster[nowIndex].count)
            {
                for (int i = 0; i < waveMonster[nowIndex].count; i++)
                {
                    string nowWave = System.Enum.GetName(typeof(Wave), waveMonster[nowIndex].wave);
                    CreateNowEnemy(waveMonster[nowIndex].monsterPrefab, waveMonster[nowIndex].spanPosition, waveMonster[nowIndex].movePosition);
                    StartCoroutine(nowWave);
                    nowCount++;
                    yield return new WaitForSeconds(waveMonster[nowIndex].spanTime);
                }
            }
            else if (isInBossAttack)
            {
                isSpanBoss = true;
                isInBossAttack = false;
                CreateNowEnemy(waveBosses[bossIndex].bossPrefab[nowBossStage - 1], waveBosses[bossIndex].spanPosition, waveBosses[bossIndex].movePosition);
                if (nowBossStage >= waveBosses[bossIndex].bossPrefab.Length)
                    OtherStage = false;
                else
                    waveBosses[bossIndex].spanPosition = waveBosses[bossIndex].movePosition[0];
                nowCount = waveMonster[nowIndex].count + 1;
                yield return null;
            }
            else
            {
                bool allEnemyDie = true;
                //尋找敵人是否全部死亡
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
                    //是否可以前往下一階段
                    if ((isSpanBoss && !OtherStage) || !isSpanBoss)
                    {
                        allIndex++;
                        nowBossStage = 1;
                        if (isSpanBoss)
                            bossIndex++;
                    }
                    //boss進入二階段
                    if(isSpanBoss&&OtherStage)
                        nowBossStage++;
                    //完全勝利
                    if (bossIndex >= waveBosses.Length)
                    {
                        isWin = true;
                        yield return new WaitForSeconds(1f);
                        //出現stage clear或進入下一個stage
                        yield return new WaitForSeconds(1f);
                        //出現勝利介面
                        GameManager.Instance.WinGame();
                        break;
                    }
                    nowCount = 0;
                    //進入下一階段
                    if (!isSpanBoss)
                        nowIndex++;
                    else
                        isSpanBoss = false;
                    //防止溢出
                    if (nowIndex >= waveMonster.Length)
                        nowIndex = 0;
                    waveEnemy.Clear();
                    //開始進入boss戰
                    if (allIndex % (waveMonster.Length / 2 + 1) == waveMonster.Length / 2)
                    {
                        StartCoroutine(BossAppear());
                        break;
                    }
                }
                yield return null;
            }
        }
    }
    void CreateNowEnemy(GameObject prefab, Transform transform, Transform[] movePosition)
    {
        tempEnemy = Instantiate(prefab, transform.position, Quaternion.identity);
        if(isSpanBoss)
        {
            tempEnemy.GetComponent<Enemy>().canTouch = false;
            tempEnemy.GetComponent<Death>().isInvincible = true;
            tempEnemy.GetComponent<Death>().hpBar = BossBar;
        }    
        for (int i = 0; i < movePosition.Length; i++)
            tempEnemy.GetComponent<Enemy>().Dot[i] = movePosition[i].position;
        waveEnemy.Add(tempEnemy);
        GameManager.Instance.ChangeDifficulty(tempEnemy);
    }
    IEnumerator BossAppear()
    {     
        OtherStage = true;
        isInBossAttack = true;
        if (nowBossStage == 1)
        {
            var items = FindObjectsOfType<Item>();
            for (int i = 0; i < items.Length; i++)
                items[i].CanAttract = true;
            yield return new WaitForSeconds(3f);
        }
        StartCoroutine(CreateEnemy());
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