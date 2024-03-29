using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum Difficulty
{
    easy,
    middle,
    Hard,
    VerryHard,
    Hell
}
public enum StatusType
{
    Pause,
    Lose,
    Win
}
public enum AwardType
{
    Bonus,
    Common,
    Failed
}
public class GameManager : MonoBehaviour
{
    static GameManager instance;
    public static GameManager Instance { get => instance; set => instance = value; }
    #region "Private"
    int totalExp = 100;
    bool isOnButton = false;
    int lifeCount = 0;
    int playerExp;
    float sideA = 0;
    float sideB = 0;
    #endregion
    #region "Public"
    public float playerScore;
    public Coroutine coroutine;
    [Header("復活秒數")]
    public float AllResurrectionTime;
    #endregion
    #region "Hide"
    public GameObject[] LightSide;
    public GameObject[] BonusScores;//0炸彈 1生命
    public Text[] MapBonusScores;//0 本關分數 1 加成分數
    [HideInInspector]
    public float thisMapScore = 0;
    [HideInInspector]
    public bool thisMapBomb = false;
    [HideInInspector]
    public bool thisMapHurt = false;
    [HideInInspector]
    public Image backTimeBarrage;
    [HideInInspector]
    public GameObject BackGround;
    public AwardType awardType = AwardType.Bonus;
    [HideInInspector]
    public int boumbCount = 0;
    [HideInInspector]
    public Sprite[] LifeImages;//0 空心 1 實心
    [HideInInspector]
    public Sprite[] bombImages;//0 空心 1 實心
    [HideInInspector]
    public Sprite[] bossImages;//0 空心 1 實心 
    [HideInInspector]
    public GameObject[] Triangles;
    [HideInInspector]
    public Animator BarUse;
    [HideInInspector]
    public Player playerScript;
    [HideInInspector]
    public GameObject[] bossStaire;
    [HideInInspector]
    //0 左上 1 右下
    public GameObject[] mapPosition;
    [HideInInspector]
    public int allBomb;
    [HideInInspector]
    public int allLife;
    //[HideInInspector]
    public GameObject StageClear;
    [HideInInspector]
    public GameObject StageBonus;
    [HideInInspector]
    public Slider BossBar;
    [HideInInspector]
    public StatusType statusType = StatusType.Pause;
    [HideInInspector]
    public Sprite[] playerFace;
    [HideInInspector]
    public GameObject playerStatus;
    [HideInInspector]
    public GameObject[] Lifes;
    [HideInInspector]
    public GameObject[] Bombs;
    [HideInInspector]
    public GameObject[] Menus;//0 暫停 1 輸 2贏
    [HideInInspector]
    public Text Title;
    [HideInInspector]
    public Transform playerSpan;
    [HideInInspector]
    public EnemyManager enemyManager;
    [HideInInspector]
    public GameObject Reciprocal;
    [HideInInspector]
    public int playerLevel = 0;
    [HideInInspector]
    public bool canTrack = false;
    [HideInInspector]
    public GameObject player;
    [HideInInspector]
    public Text scoreText;
    [HideInInspector]
    public Text Level;
    [HideInInspector]
    public Slider expBar;
    [HideInInspector]
    public Transform PlayerResurrectionPosition;
    #endregion
    #region "難度"
    [Header("調難度")]
    public Difficulty difficulty;
    public int playerBottom;
    public int playerLife;
    [Header("無敵時間")]
    public float AllInvincibleTime; //無敵秒數
    #endregion
    void Awake()
    {
        instance = this;
        backTimeBarrage = BackGround.GetComponent<Image>();
        AddBottom(playerBottom);
        AddLife(playerLife);
        coroutine = StartCoroutine(Begin());
    }
    IEnumerator Begin()
    {
        Title.gameObject.SetActive(true);
        yield return new WaitForSeconds(2);
        Title.gameObject.SetActive(false);
        playerScript = Instantiate(player, playerSpan.transform.position, Quaternion.identity).gameObject.GetComponent<Player>();
        playerScript.gameObject.GetComponent<CapsuleCollider2D>().isTrigger = true;
        while (playerScript.gameObject.transform.position != PlayerResurrectionPosition.position)
        {
            playerScript.gameObject.transform.position = Vector2.MoveTowards(playerScript.gameObject.transform.position, PlayerResurrectionPosition.position, playerScript.speed * Time.deltaTime);
            yield return 0f;
        }
        playerScript.gameObject.GetComponent<CapsuleCollider2D>().isTrigger = false;
        playerScript.canMove = true;
        yield return new WaitForSeconds(1);
        enemyManager.canGoNext = false;
        StartCoroutine(enemyManager.CreateEnemy());
    }
    void Update()
    {
        SideAdjustment();
        switch (statusType)
        {
            case StatusType.Pause:
                MenuUse();
                break;
            case StatusType.Win:
                WinGame();
                break;
            case StatusType.Lose:
                LoseGame();
                break;
        }
    }
    #region "復活"
    public void Resurrection()
    {
        if (lifeCount < 0)
        {
            statusType = StatusType.Lose;
        }
        else
        {
            Invoke("PlayerResurrection", AllResurrectionTime);
        }
    }
    void PlayerResurrection()
    {
        playerScript = Instantiate(player, PlayerResurrectionPosition.position, Quaternion.identity).GetComponent<Player>();
        playerScript.AddBro();
        playerScript.gameObject.GetComponent<Death>().isInvincible = true;
        playerScript.canMove = true;
        Invoke("PlayerNotInvincible", AllInvincibleTime);
    }
    void PlayerNotInvincible()
    {
        playerScript.gameObject.GetComponent<Death>().isInvincible = false;
    }
    #endregion
    #region "吃東西"
    public void EatItem(Item item)
    {
        switch (item.itemType)
        {
            case ItemType.Life:
                if (lifeCount < allLife)
                {
                    AddScore(item.score);
                    AddLife(1);
                }
                else
                {
                    AddScore(item.overflowScore);
                }
                break;
            case ItemType.Bomb:
                if (boumbCount < allBomb)
                {
                    AddScore(item.score);
                    AddBottom(1);
                }
                else
                {
                    AddScore(item.overflowScore);
                }
                break;
            case ItemType.Drone:
                if (playerLevel < 3)
                {
                    AddExp(500);
                    playerScript.AddBro();
                    AddScore(item.score);
                }
                else
                {
                    AddScore(item.overflowScore);
                }
                break;
            case ItemType.EXP:
                if (playerLevel < 3)
                {
                    AddExp(1);
                    playerScript.AddBro();
                    AddScore(item.score);
                }
                else
                {
                    AddScore(item.overflowScore);
                }
                break;
        }
    }
    public void AddScore(float value)
    {
        playerScore += value;
        thisMapScore = playerScore;
        scoreText.text = "     Score:" + playerScore.ToString();
    }
    public void AddLife(int value)
    {
        lifeCount += value;
        for (int i = 0; i < allLife; i++)
        {
            Lifes[i].gameObject.GetComponent<Image>().sprite = LifeImages[0];
        }
        for (int i = 0; i < lifeCount; i++)
        {
            Lifes[i].gameObject.GetComponent<Image>().sprite = LifeImages[1];
        }
    }
    public void AddBottom(int value)
    {
        boumbCount += value;
        for (int i = 0; i < allBomb; i++)
        {
            Bombs[i].gameObject.GetComponent<Image>().sprite = bombImages[0];
        }
        for (int i = 0; i < boumbCount; i++)
        {
            Bombs[i].gameObject.GetComponent<Image>().sprite = bombImages[1];
        }
    }
    void AddExp(int value)
    {
        playerExp += value;
        while (playerExp >= totalExp)
        {
            playerLevel += 1;
            playerStatus.gameObject.GetComponent<Image>().sprite = playerFace[playerLevel];
            if (playerLevel < 3)
            {
                playerExp -= totalExp;
                Level.text = "Levil " + playerLevel.ToString();
            }
            else
            {
                playerExp = totalExp;
                playerLevel = 3;
                Level.text = "Levil Max".ToString();
                break;
            }
        }
        expBar.value = (float)playerExp / totalExp;
    }
    #endregion
    void MinusLevel()
    {
        playerExp = 0;
        if (playerLevel > 0)
        {
            playerLevel -= 1;
            playerStatus.gameObject.GetComponent<Image>().sprite = playerFace[playerLevel];
            expBar.value = (float)playerExp / totalExp;
            Level.text = "Levil " + playerLevel.ToString();
        }
    }
    public void ClearBarrage()
    {
        var barrages = FindObjectsOfType<Bullet>();
        for (int i = 0; i < barrages.Length; i++)
            if (barrages[i] != null)
                barrages[i].Die();
    }
    public void ChangeDifficulty(GameObject gameObject = null)
    {
        //調難度
        float tempCountTime = 1;
        bool tempCanTrack = false;
        float tempProbability = 1; //0 生命 1 炸彈 2 小弟 4生命碎片
        bool[] tempCanAttract = { false, false, false, false, false }; //0 exp 1 生命 2 炸彈 3 小弟 4 生命碎片
        switch (difficulty)
        {
            case Difficulty.easy:
                tempCountTime *= 2;
                tempCanTrack = true;
                tempProbability = 2;
                for (int i = 0; i < tempCanAttract.Length; i++)
                {
                    tempCanAttract[i] = true;
                }
                break;
            case Difficulty.middle:
                tempCanAttract[0] = true;
                tempCanAttract[1] = true;
                break;
            case Difficulty.Hard:
                tempCountTime *= 0.5f;
                tempProbability = 0.5f;
                break;
        }
        canTrack = tempCanTrack;
        if (gameObject != null)
        {
            if (gameObject.GetComponent<Death>())
            {
                var death = gameObject.GetComponent<Death>();
                for (int i = 0; i < death.itemStruct.Length; i++)
                {
                    death.itemStruct[i].probability *= tempProbability;
                }
            }
            if (gameObject.GetComponent<Enemy>())
            {
                var enemy = gameObject.GetComponent<Enemy>();
                enemy.countTime *= tempCountTime;
            }
            if (gameObject.GetComponent<Item>())
            {
                var item = gameObject.GetComponent<Item>();
                switch (item.itemType)
                {
                    case ItemType.EXP:
                        item.CanAttract = tempCanAttract[0];
                        break;
                    case ItemType.Life:
                        item.CanAttract = tempCanAttract[1];
                        break;
                    case ItemType.Bomb:
                        item.CanAttract = tempCanAttract[2];
                        break;
                    case ItemType.Drone:
                        item.CanAttract = tempCanAttract[3];
                        break;
                }
            }
        }
    }
    #region "boss戰"
    public void BeginReciprocal()
    {
        Reciprocal.SetActive(true);
        Reciprocal.GetComponent<Reciprocal>().allTime = 60;
        Reciprocal.GetComponent<Reciprocal>().isDead = false;
    }
    public void ShowBossStaire(int count, int nowStage)
    {
        for (int i = 0; i < count - 1; i++)
        {
            bossStaire[i].SetActive(true);
            if (i < count - nowStage)
                bossStaire[i].GetComponent<Image>().sprite = bossImages[1];
        }
    }
    //階段顯示
    public void BossNext()
    {
        BossBar.value = 1;
        Reciprocal.GetComponent<Reciprocal>().gameObject.SetActive(false);
        //播放血條動畫<關>
        BarUse.Play("Close");
        for (int i = 0; i < bossStaire.Length; i++)
        {
            bossStaire[i].GetComponent<Image>().sprite = bossImages[0];
            bossStaire[i].SetActive(false);
            BossBar.gameObject.SetActive(false);
            Triangles[0].SetActive(false);
            Triangles[1].SetActive(false);
        }
        enemyManager.nowEveryStairTime = enemyManager.everyStairTime;
    }
    #endregion
    public void MenuUse()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isOnButton = !isOnButton;
            Menus[0].SetActive(isOnButton);
            if (playerScript)
                playerScript.enabled = !isOnButton;
            if (isOnButton)
                Time.timeScale = 0;
            else
                Time.timeScale = 1;
        }
    }
    public void Replay()
    {
        Time.timeScale = 1;
        statusType = StatusType.Pause;
        SceneManager.LoadScene("Game");
    }
    public void BackToMenu()
    {
        Time.timeScale = 1;
        statusType = StatusType.Pause;
        SceneManager.LoadScene("Main");
    }
    public void Return()
    {
        Menus[1].SetActive(false);
        statusType = StatusType.Pause;
        //MinusLevel();
        AddLife(playerLife);
        PlayerResurrection();
        Time.timeScale = 1;
    }
    void WinGame()
    {
        playerScript.enabled = false;
        Menus[2].SetActive(true);
        Time.timeScale = 0;
    }
    void LoseGame()
    {
        Menus[1].SetActive(true);
        Time.timeScale = 0;
    }
    void SideAdjustment()
    {
        if(!enemyManager.canGoNext)
        {
            if(LightSide[0].gameObject.GetComponent<Image>().color.a<=0.5&&!enemyManager.isSpanBoss&&enemyManager.bossIndex==0||enemyManager.isSpanBoss)
            {
                if(enemyManager.bossIndex!=0)
                {
                    sideB+=Time.deltaTime/25;
                }
                sideA+=Time.deltaTime/50;
            }
            else if(LightSide[0].gameObject.GetComponent<Image>().color.a>=0.5&&!enemyManager.isSpanBoss&&enemyManager.bossIndex==1)
            {
                sideA-=Time.deltaTime/50;
            }
            LightSide[0].gameObject.GetComponent<Image>().color = new Color(1,1,1,sideA);
            LightSide[1].gameObject.GetComponent<Image>().color = new Color(1,1,1,sideB);
        }
    }
}
