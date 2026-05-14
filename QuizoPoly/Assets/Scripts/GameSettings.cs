using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Quizopoly/Game Settings")]
public class GameSettings : ScriptableObject
{
    [Header("Economy")]
    public int startingMoney = 5_000_000;
    public int passStartBonus = 200_000;
    public int bonusTileReward = 250_000;
    public int taxRate = 7;
    public int jailExitFee = 100_000;

    [Header("Rent")]
    public int emptyLandRent = 25;
    public int builtLandRent = 50;
    public float colorSetMultiplier = 2f;
    public float buyFromOpponentMultiplier = 2f;

    [Header("Time")]
    public int beginnerQuestionTime = 90;
    public int easyQuestionTime = 60;
    public int mediumQuestionTime = 45;
    public int hardQuestionTime = 30;
    public int impossibleQuestionTime = 20;

    [Header("Jail")]
    public int jailDuration = 3;
    public int jailExitDifficulty = 1; // 0=Easy, 1=Medium, 2=Hard

    [Header("Game End")]
    public int vacationVictoryCount = 3;
    public bool vacationVictoryEnabled = true;
    public int maxGameDuration = 0; // 0=Unlimited, 1=60min, 2=90min, 3=120min
    public int endGameTiebreaker = 0; // 0=Richest, 1=Most Properties

    [Header("Other")]
    public bool chanceCardsEnabled = true;
    public int buildingLevelSelection = 0; // 0=Player Chooses, 1=Sequential
}