using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum CharacterName { 简单的敌人,中等的敌人,困难的敌人, 玩家};
public enum League {队伍1, 队伍2, 队伍3, 队伍4 };

public class LineWriter : MonoBehaviour
{
    static int LeagueSum = 4;
    [SerializeField] CharacterName characterName;
    [SerializeField] Text nameText;
    //
    [SerializeField] bool isPlayer;
    [SerializeField] Button[] buttons;
    [SerializeField] GameObject namePanel;
    //
    [SerializeField] League league;
    [SerializeField] Text leagueText;

    private void Start()
    {
        RefreshName();
        RefreshLeague();
    }
    public void OpenChooseNamePanel()
    {
        if (!isPlayer)
        {
            namePanel.SetActive(true);
        }
    }
    public void ChooseName(int id)
    {
        namePanel.SetActive(false);
        characterName = (CharacterName)id;
        RefreshName();
    }
    void RefreshName()
    {
        nameText.text = (characterName).ToString();
    }
    public void ChangeLeague()
    {
        league = (League)(((int)league + 1) % LeagueSum);
        RefreshLeague();
    }
    void RefreshLeague()
    {
        leagueText.text = league.ToString();
    }
    public CharacterName ReturnCharacterName()
    {
        return characterName;
    }
    public League ReturnLeague()
    {
        return league;
    }
}
