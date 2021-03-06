using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotManager : MonoBehaviour
{
    public GameObject botPrefab;
    public int botCount = 10;
    public List<GameObject> bots;
    public WaypointManager waypointManager;
    public PlayerMyself mainCharacter;
    public PlankManager plankManager;
    Color[] plankColors =
    {
        new Color(1, 0, 0, 0.4f),
        new Color(0, 1, 0, 0.4f),
        new Color(0, 0, 1, 0.4f),
        new Color(1, 1, 0, 0.4f),
        new Color(0.5f, 0.5f, 0.5f, 0.4f),
        new Color(0, 0, 0, 0.4f)
    };

    void Start()
    {
        for (int i = 0; i < botCount; i++)
        {
            GameObject bot = Instantiate(botPrefab, transform);
            bot.name = "Bot " + (i + 1);

            Bot currentBot = bot.GetComponent<Bot>();
            currentBot.rb = bot.GetComponent<Rigidbody>();
            currentBot.animator = bot.GetComponent<Animator>();
            currentBot.txtName.text = bot.name;
            currentBot.waypointManager = waypointManager;
            currentBot.plankManager = plankManager;
            currentBot.CreatePoolObjects();
            currentBot.SetPlankBagColor(plankColors[i]);

            bots.Add(bot);
        }

        foreach (GameObject bot in bots)
        {
            foreach (GameObject b in bots)
            {
                if (bot != b)
                    Physics.IgnoreCollision(bot.GetComponent<CapsuleCollider>(), b.GetComponent<CapsuleCollider>());
            }

            Physics.IgnoreCollision(bot.GetComponent<CapsuleCollider>(), mainCharacter.GetComponent<CapsuleCollider>());
        }

        ResetBots();
        mainCharacter.SetPlankBagColor(plankColors[5]);
    }

    public void UpdateCanMove(bool canMove)
    {
        foreach (GameObject bot in bots)
            bot.GetComponent<Bot>().UpdateCanMove(canMove);
    }

    public void ResetBots()
    {
        int spawnIndex = 0;
        foreach (GameObject bot in bots)
        {
            bot.transform.position = waypointManager.GetNextSpawnPoint(spawnIndex);
            bot.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));

            Bot currentBot = bot.GetComponent<Bot>();
            currentBot.rb.useGravity = true;
            currentBot.isAlive = true;
            currentBot.characterAnimState = AnimState.Idle;
            currentBot.animator.SetInteger("AnimState", (int)AnimState.Idle);
            currentBot.rb.velocity = Vector3.zero;
            currentBot.isFinished = false;
            currentBot.currentPlankCount = 0;
            currentBot.isJumped = false;
            currentBot.isClimbing = false;
            currentBot.goTriggerClimb.SetActive(false);
            currentBot.UpdateCanMove(false);
            currentBot.GetFirstWaypoint();
            currentBot.HidePoolPlanks();

            spawnIndex++;
        }
    }

    public void OutOfPoolObjects()
    {
        foreach (GameObject bot in bots)
            bot.GetComponent<Bot>().UpdateCanMove(false);
    }
}