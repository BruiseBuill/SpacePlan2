using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using BF;

public class GameManager :Single<GameManager>
{
    static int leagueSum = 4;
    GameObject linePanel;
    string playerLeague = "League0";
    string[] leagues = new string[] { "League0", "League1", "League2", "League3" };
    List<LineWriter> allLines = new List<LineWriter>();
    List<LineWriter> lines0 = new List<LineWriter>();
    List<LineWriter> lines1 = new List<LineWriter>();
    List<LineWriter> lines2 = new List<LineWriter>();
    List<LineWriter> lines3 = new List<LineWriter>();
    //
    int[] aliveSum = new int[] { 0, 0, 0, 0 };

    protected void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        linePanel = GameObject.FindGameObjectWithTag("linePanel");
#if UNITY_EDITOR
        if (linePanel == null)
        {
            return;
        }
#endif
        
    }
    public string PlayerLeague 
    {
        get { return playerLeague; }
    }
    public void StartGame()
    {
        if (Check())
        {
            SceneManager.LoadScene(1);
            StartCoroutine("wait");
        }
    }
    public void StartTeaching()
    {
        SceneManager.LoadScene(2);
    }
    bool Check()
    {
        allLines.Clear();
        lines0.Clear();
        lines1.Clear();
        lines2.Clear();
        lines3.Clear();
        linePanel = GameObject.FindGameObjectWithTag("linePanel");
        for (int i = 0; i < linePanel.transform.childCount; i++) 
        {
            var a = linePanel.transform.GetChild(i).GetComponent<LineWriter>();
            allLines.Add(a);
            switch (a.ReturnLeague())
            {
                case League.队伍1:
                    lines0.Add(a);
                    break;
                case League.队伍2:
                    lines1.Add(a);
                    break;
                case League.队伍3:
                    lines2.Add(a);
                    break;
                case League.队伍4:
                    lines3.Add(a);
                    break;
            }
        }
        aliveSum[0] = lines0.Count;
        aliveSum[1] = lines1.Count;
        aliveSum[2] = lines2.Count;
        aliveSum[3] = lines3.Count;
        bool canStart = false;
        for(int i = 0; i < leagueSum; i++)
        {
            for (int j = i + 1; j < leagueSum; j++) 
            {
                if (aliveSum[i] != 0 && aliveSum[j] != 0) 
                {
                    canStart = true;
                }
            }
        }
        return canStart;
    }
    IEnumerator wait()
    {
        yield return null;
        InitializeBrains();
    }
    void InitializeBrains()
    {
        var brains = GameObject.FindObjectsOfType<PlayerBrain>();
        int index = 0;
        int i ;
        for (i = 0; i < allLines.Count; i++)
        {
            InitializeBrain(allLines[i].ReturnCharacterName(), brains[index++],allLines[i].ReturnLeague());
        }
    }
    void InitializeBrain(CharacterName character, PlayerBrain playerBrain,League league)
    {
        playerBrain.tag = leagues[(int)league];
        switch (character)
        {
            case CharacterName.玩家:
                playerBrain.enabled = true;
                playerBrain.GetComponent<AIBrain>().enabled = false;
                playerLeague = leagues[(int)league];
                break;
            case CharacterName.简单的敌人:
                playerBrain.enabled = false;
                playerBrain.GetComponent<AIBrain>().enabled = true;
                playerBrain.GetComponent<AIBrain>().SetDifficulty(Difficulty.Easy);
                break;
            case CharacterName.中等的敌人:
                playerBrain.enabled = false;
                playerBrain.GetComponent<AIBrain>().enabled = true;
                playerBrain.GetComponent<AIBrain>().SetDifficulty(Difficulty.Normal);
                break;
            case CharacterName.困难的敌人:
                playerBrain.enabled = false;
                playerBrain.GetComponent<AIBrain>().enabled = true;
                playerBrain.GetComponent<AIBrain>().SetDifficulty(Difficulty.Hard);
                break;
        }
    }
    public void PlayerDefeat()
    {
        UIManager.Instance().SetWinText("战役失败");
        StartCoroutine(ReturnMenu());
    }
    public void AIDefeat(string league)
    {
        if (league == "League0") 
        {
            aliveSum[0]--;
        }
        else if (league == "League1")
        {
            aliveSum[1]--;
        }
        else if (league == "League2")
        {
            aliveSum[2]--;
        }
        else if (league == "League3")
        {
            aliveSum[3]--;
        }
        bool canWin = true;
        for (int i = 0; i < leagueSum - 1; i++)
        {
            for (int j = i + 1; j < leagueSum; j++)
            {
                if (aliveSum[i] > 0 && aliveSum[j] > 0) 
                {
                    canWin = false;
                }
            }
        }
        if(canWin)
        {
            UIManager.Instance().SetWinText("战役胜利");
            StartCoroutine(ReturnMenu());
        }   
    }
    IEnumerator ReturnMenu()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(0);
    }
}
