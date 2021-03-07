using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SCR_BossHealthScaling : MonoBehaviour
{
    // Start is called before the first frame update
    Slider UI_bosshp;
    static float bossHp;
    [SerializeField]
    static float bossMaxHP = 100;
	static float bossInitialMaxHP;
    [SerializeField]
    static int bossHPLimit = 250;
    [SerializeField]
    int bossSceneIndex = 3;
    [SerializeField]
    int baseLevelIndex = 1;
    [SerializeField]
    bool bossRoom;

    float healthIncreaseRate = 1;

    void OnEnable()
    {
        if (GameObject.FindGameObjectsWithTag("GameManager").Length <= 1)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            InvokeRepeating("IncreaseHealth", 1f, 1f);
        }
    }

    void Start()
    {
        if (GameObject.FindGameObjectsWithTag("GameManager").Length <= 1)
        {
            bossHp = BossStats.TransferBossInitialMaxHP() * 0.5f;
            BossStats.UpdateBossHP(Mathf.RoundToInt(bossHp));
            healthIncreaseRate = BossStats.TransferHealthIncreaseRate();
        }
    }

    void IncreaseHealth()
    {
        if(bossHp == bossMaxHP && (!bossRoom || SCR_Boss_SM.ritualEnemiesChanneling > 0))
        {
            if (bossMaxHP < bossHPLimit)
            {
                bossMaxHP += healthIncreaseRate;
                bossHp += healthIncreaseRate;
                BossStats.IncreaseMaxHealth(Mathf.RoundToInt(bossMaxHP));
                BossStats.BossDamage(-healthIncreaseRate, false);
            }
            else
            {
                CancelInvoke();
            }
        }
        else if (!bossRoom)
        {
            bossHp++;
			BossStats.UpdateBossHP(Mathf.RoundToInt(bossHp));
			if(bossHp == bossMaxHP)
			{
				BossStats.initStatIncrease = true;
			}
        }
        //Debug.Log("Boss Health is at: " + bossHp);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(scene.buildIndex == 7)
        {
            bossRoom = true;
            UI_bosshp = GameObject.Find("BossHealthSlider").GetComponent<Slider>();
            UI_bosshp.maxValue = bossMaxHP;
            GameManager.UpdateBossHealth(Mathf.RoundToInt(Mathf.Clamp(bossHp, 0, bossMaxHP)));
        }
        else
        {
            bossRoom = false;
        }
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public static void OnRetry()
    {
        bossMaxHP = BossStats.TransferBossInitialMaxHP();
		bossHp = bossMaxHP * 0.25f;
		BossStats.initStatIncrease = false;
    }

    public void Update()
    {
        //Debug.Log("boss health (not-slider):" + bossHp);
    }
}
