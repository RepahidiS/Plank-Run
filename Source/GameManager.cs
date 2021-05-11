using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public UIManager uiManager;
    public BotManager botManager;
    public PlayerMyself mainCharacter;
    public WaypointManager waypointManager;
    public PlankManager plankManager;
    public Transform plankPileSpawnerParent;
    public List<PlankPileSpawner> plankPileSpawners = new List<PlankPileSpawner>();
    public ScoreMultiplierPlatformsManager scoreMultiplierPlatformsManager;
    public List<Rank> rank = new List<Rank>();
    public List<string> finishedRank = new List<string>();
    public int gameStartCooldown = 3;
    public Material mtTransparent;
    public int firstPlaceGold = 100;

    bool isGameStarted = false;
    bool showCountdown = true;
    float currentStartCooldown;

    void Awake()
    {
        uiManager.gameManager = this;
        plankManager.gameManager = this;
        mainCharacter.gameManager = this;

        plankPileSpawners = plankPileSpawnerParent.GetComponentsInChildren<PlankPileSpawner>().ToList();
    }

    void Start()
    {
        ResetGame();
    }

    void Update()
    {
        if(!mainCharacter.isAlive)
        {
            mainCharacter.UpdateCanMove(false);
            uiManager.endGameUI.UpdateStatistics("You died!");
            uiManager.endGameUI.Show();
        }

        if (showCountdown)
            UpdateCountdown();

        if (isGameStarted)
            UpdateRank();
    }

    public void ResetGame()
    {
        isGameStarted = false;
        showCountdown = true;

        currentStartCooldown = Time.time + gameStartCooldown;

        botManager.ResetBots();
        mainCharacter.ResetMainCharacter();

        uiManager.ResetUIs();
        finishedRank.Clear();

        plankManager.HidePlanks();
        scoreMultiplierPlatformsManager.ResetPlatforms();

        Camera.main.transform.parent = mainCharacter.transform;
        Camera.main.transform.localPosition = new Vector3(0.0f, 8.0f, -8.0f);
        Camera.main.transform.rotation = Quaternion.Euler(new Vector3(25.0f, 0.0f, 0.0f));

        uiManager.endGameUI.Hide();

        foreach (PlankPileSpawner spawner in plankPileSpawners)
            spawner.ResetPile();
    }

    void UpdateCountdown()
    {
        float remainingTime = currentStartCooldown - Time.time;

        if (remainingTime <= 0.0f)
        {
            botManager.UpdateCanMove(true);
            mainCharacter.UpdateCanMove(true);
            isGameStarted = true;

            if (remainingTime <= -1.0f)
            {
                uiManager.startGameUI.Hide();
                showCountdown = false;
            }
            else uiManager.UpdateCountdownText("Go!");
        }
        else uiManager.UpdateCountdownText(Mathf.Ceil(remainingTime).ToString());
    }

    public void PlayerFinished()
    {
        if(mainCharacter.isFinished)
        {
            int index = finishedRank.FindIndex(player => player == "You");

            if (finishedRank.Count == 0)
            {
                finishedRank.Add("You");
                mainCharacter.runningToMultipliers = true;
                scoreMultiplierPlatformsManager.movePlatforms = true;
            }
            else if(index == -1)
            {
                finishedRank.Add("You");
                index = finishedRank.FindIndex(player => player == "You");
                uiManager.endGameUI.UpdateStatistics("You finished at \n" + (index + 1) + ". place!\nYour reward is " + mainCharacter.currentPlankCount + " gold!");
                uiManager.endGameUI.Show();
            }
        }
    }

    void UpdateRank()
    {
        rank.Clear();

        float distance = float.MaxValue;

        if (mainCharacter.isFinished)
        {
            if(!mainCharacter.runningToMultipliers || (mainCharacter.runningToMultipliers && mainCharacter.isMultiplierFinished))
            {
                uiManager.endGameUI.UpdateStatistics("You finished at 1st place!\nYour reward is " + firstPlaceGold * mainCharacter.currentMultiplier + " gold!");
                uiManager.endGameUI.Show();
            }
        }
        else
        {
            distance = Vector3.Distance(mainCharacter.transform.position, waypointManager.GetFinishPos());
            rank.Add(new Rank("You", distance));
        }

        foreach (GameObject bot in botManager.bots)
        {
            if (bot.GetComponent<Bot>().isFinished)
            {
                int index = finishedRank.FindIndex(player => player == bot.name);
                if (index == -1)
                    finishedRank.Add(bot.name);
            }
            else
            {
                distance = Vector3.Distance(bot.transform.position, waypointManager.GetFinishPos());
                rank.Add(new Rank(bot.name, distance));
            }
        }

        rank.Sort((player1, player2) =>
        {
            return player1.distance.CompareTo(player2.distance);
        });

        uiManager.rankUI.UpdateRank(finishedRank, rank);
    }

    public void OutOfPoolObjects()
    {
        mainCharacter.UpdateCanMove(false);
        uiManager.endGameUI.UpdateStatistics("There is no planks at object pool!\nWhy haven't you finished the game yet!?");
        uiManager.endGameUI.Show();
        botManager.OutOfPoolObjects();
    }
}

public class Rank
{
    public string name;
    public float distance;

    public Rank(string _name, float _distance)
    {
        name = _name;
        distance = _distance;
    }
}