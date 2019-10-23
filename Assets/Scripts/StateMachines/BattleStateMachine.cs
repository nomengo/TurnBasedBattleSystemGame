﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleStateMachine : MonoBehaviour
{
    public enum PerformAction
    {
        Wait,
        TakeAction,
        PerformAction

    }
    public PerformAction BattleStates;

    public List<HandleTurn> PerformList = new List<HandleTurn>();
    public List<GameObject> HerosInBattle = new List<GameObject>();
    public List<GameObject> EnemysInBattle = new List<GameObject>();

    public GameObject enemyButton;
    public Transform Spacer;

    public GameObject AttackPanel;
    public GameObject EnemySelectPanel;
    public GameObject MagicPanel;

    // magic attacks
    public Transform actionSpacer;
    public Transform magicSpacer;
    public GameObject actionButton;
    public GameObject magicButton;
    private List<GameObject> atkBtns = new List<GameObject>();
    

    public enum HeroGUI
    {
        Activate,
        Waiting,
        Input1,
        Input2,
        Done
    }
    public HeroGUI HeroInput;

    public List<GameObject> HerosToManage = new List<GameObject>();
    private HandleTurn HeroChoise;


    void Start()
    {
        BattleStates = PerformAction.Wait;
        EnemysInBattle.AddRange(GameObject.FindGameObjectsWithTag("Enemy"));
        HerosInBattle.AddRange(GameObject.FindGameObjectsWithTag("Hero"));
        HeroInput = HeroGUI.Activate;
        AttackPanel.SetActive(false);
        EnemySelectPanel.SetActive(false);
        MagicPanel.SetActive(false);

        EnemyButtons();
    }

    // Update is called once per frame
    void Update()
    {
        switch (BattleStates)
        {
            case (PerformAction.Wait):
                if (PerformList.Count > 0)
                {
                    BattleStates = PerformAction.TakeAction;
                }
                break;
            case (PerformAction.TakeAction):
                GameObject performer = GameObject.Find(PerformList[0].Attacker);
                if (PerformList[0].Type == "Enemy")
                {
                    EnemyStateMachine ESM = performer.GetComponent<EnemyStateMachine>();
                    for (int i = 0; i < HerosInBattle.Count; i++)
                    {
                        if (PerformList[0].AttackersTarget == HerosInBattle[i])
                        {
                            ESM.HeroToAttack = PerformList[0].AttackersTarget;
                            ESM.currentState = EnemyStateMachine.TurnState.Action;
                            break;
                        }
                        else
                        {
                            PerformList[0].AttackersTarget = HerosInBattle[Random.Range(0, HerosInBattle.Count)];
                            ESM.HeroToAttack = PerformList[0].AttackersTarget;
                            ESM.currentState = EnemyStateMachine.TurnState.Action;
                        }
                    }
                }
                if (PerformList[0].Type == "Hero")
                {
                    HeroStateMachine HSM = performer.GetComponent<HeroStateMachine>();
                    HSM.EnemyToAttack = PerformList[0].AttackersTarget;
                    HSM.currentState = HeroStateMachine.TurnState.Action;
                }
                BattleStates = PerformAction.PerformAction;
                break;
            case (PerformAction.PerformAction):

                break;

        }
        switch (HeroInput)
        {
            case (HeroGUI.Activate):
                if (HerosToManage.Count > 0)
                {
                    HerosToManage[0].transform.Find("Selector").gameObject.SetActive(true);
                    //create new handleturn instance
                    HeroChoise = new HandleTurn();

                    AttackPanel.SetActive(true);
                    //populate action buttons
                    CreateAttackButtons();
                    HeroInput = HeroGUI.Waiting;
                }
                break;
            case (HeroGUI.Waiting):

                break;

            case (HeroGUI.Done):
                HeroInputDone();
                break;
        }
    }
    public void CollectActions(HandleTurn input)
    {
        PerformList.Add(input);
    }
    void EnemyButtons()
    {
        foreach (GameObject enemy in EnemysInBattle)
        {
            GameObject newButton = Instantiate(enemyButton) as GameObject;
            EnemySelectButton button = newButton.GetComponent<EnemySelectButton>();

            EnemyStateMachine cur_enemy = enemy.GetComponent<EnemyStateMachine>();

            Text buttonText = newButton.transform.Find("Text").gameObject.GetComponent<Text>();
            buttonText.text = cur_enemy.enemy.theName;

            button.EnemyPrefab = enemy;

            newButton.transform.SetParent(Spacer, false);

        }
    }
    public void Input1()//attack button
    {
        HeroChoise.Attacker = HerosToManage[0].name;
        HeroChoise.AttackersGameObject = HerosToManage[0];
        HeroChoise.Type = "Hero";

        AttackPanel.SetActive(false);
        EnemySelectPanel.SetActive(true);
    }
    public void Input2(GameObject choosenEnemy)// enemy select
    {
        HeroChoise.AttackersTarget = choosenEnemy;
        HeroInput = HeroGUI.Done;
    }
    void HeroInputDone()
    {
        PerformList.Add(HeroChoise);
        EnemySelectPanel.SetActive(false);
        //clean the attack panel
        foreach(GameObject atkbtn in atkBtns)
        {
            Destroy(atkbtn);
        }
        atkBtns.Clear();

        HerosToManage[0].transform.Find("Selector").gameObject.SetActive(false);
        HerosToManage.RemoveAt(0);
        HeroInput = HeroGUI.Activate;
    }
    //create action buttons
    void CreateAttackButtons()
    {
        GameObject AttackButton = Instantiate(actionButton) as GameObject;
        Text AttackButtonText = AttackButton.transform.Find("Text").gameObject.GetComponent<Text>();
        AttackButtonText.text = "Attack";
        AttackButton.GetComponent<Button>().onClick.AddListener(() => Input1());
        AttackButton.transform.SetParent(actionSpacer, false);
        atkBtns.Add(AttackButton);

        GameObject MagicAttackButton = Instantiate(actionButton) as GameObject;
        Text MagicAttackButtonText = MagicAttackButton.transform.Find("Text").gameObject.GetComponent<Text>();
        MagicAttackButtonText.text = "Magic";
        MagicAttackButton.GetComponent<Button>().onClick.AddListener(() => Input3());
        MagicAttackButton.transform.SetParent(actionSpacer, false);
        atkBtns.Add(MagicAttackButton);

        if (HerosToManage[0].GetComponent<HeroStateMachine>().hero.MagicAttacks.Count > 0)
        {
            foreach(BaseAttack magicAtk in HerosToManage[0].GetComponent<HeroStateMachine>().hero.MagicAttacks)
            {
                GameObject MagicButton = Instantiate(magicButton) as GameObject;
                Text MagicButtonText = MagicButton.transform.Find("Text").gameObject.GetComponent<Text>();
                MagicButtonText.text = magicAtk.attackName;
                AttackButton ATB = MagicButton.GetComponent<AttackButton>();
                ATB.magicAttackToPerform = magicAtk;
                MagicButton.transform.SetParent(magicSpacer, false);
                atkBtns.Add(MagicButton);

            }
        }
        else
        {
            MagicAttackButton.GetComponent<Button>().interactable = false;
        }
    }
    public void Input4(BaseAttack choosenMagic)//choosen magic attacks
    {
        HeroChoise.Attacker = HerosToManage[0].name;
        HeroChoise.AttackersGameObject = HerosToManage[0];
        HeroChoise.Type = "Hero";

        HeroChoise.choosenAttack = choosenMagic;
        MagicPanel.SetActive(false);
        EnemySelectPanel.SetActive(true);
    }
    public void Input3()//switching to magic attacks
    {
        AttackPanel.SetActive(false);
        MagicPanel.SetActive(true);
    }
}
