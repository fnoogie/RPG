using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class CombatManagerScript : MonoBehaviour
{
    [SerializeField] private float timeNow = 0;
    public float timeBeforeTurn = 5;
    public List<BaseUnitScript> unitsInCombat; 
    [SerializeField] List<BaseUnitScript> unitsToTakeTurn ;
    public List<BaseUnitScript> players;
    public List<BaseUnitScript> enemies;
    [HideInInspector] 
    public bool someoneTurn;

    [Tooltip("The name of the scenes to load when combat is over")]
    public string winScene, loseScene;

    public TextMeshProUGUI textBox;
    [HideInInspector]
    public List<string> combatLog;

    // Start is called before the first frame update
    void Start()
    {
        timeNow = 0;
        unitsInCombat.Clear();
        unitsToTakeTurn.Clear();
        players.Clear();
        enemies.Clear();
        foreach (BaseUnitScript unit in GameObject.FindObjectsOfType(typeof(BaseUnitScript)))
        {
            unitsInCombat.Add(unit);
        }
        foreach (BaseUnitScript unit in unitsInCombat)
        {
            unit.combatManager = this;
        }

        for(int i = 0; i < unitsInCombat.Count; ++i)
        {
            if (unitsInCombat[i].isPlayer)
                players.Add(unitsInCombat[i]);
            else
                enemies.Add(unitsInCombat[i]);
        }
        combatLog.Add("Combat started");
    }

    // Update is called once per frame
    void Update()
    {
        if (enemies.Count <= 0)
            SceneManager.LoadScene(winScene);
        else
        {
            float total = 0;
            for(int i = 0; i < players.Count; ++i)
            {
                total += players[i].Health;
            }
            if (total <= 0.0f)
                SceneManager.LoadScene(loseScene);
        }


        if (unitsToTakeTurn.Count < 1)
        {
            for (int i = 0; i < unitsInCombat.Count; ++i)
            {
                if(unitsInCombat[i].Health > 0)
                    unitsInCombat[i].turnTracker += (Time.deltaTime * unitsInCombat[i].Speed);
            }

            for(int i = 0; i < unitsInCombat.Count; ++i)
            {
                if (unitsInCombat[i].turnTracker >= timeBeforeTurn)
                    unitsToTakeTurn.Add(unitsInCombat[i]);
            }
        }
        else
        {
            unitsToTakeTurn[0].myTurn = true;
            
        }


        unitEndTurn();
    }


    public void unitEndTurn()
    {
        /*
        BaseUnitScript theUnit = unitsToTakeTurn[0];
        theUnit.myTurn = false;
        Debug.Log(unitsToTakeTurn[0] + " took it's turn");
        theUnit.turnTracker = 0;
        Debug.Log("Removing: " + unitsToTakeTurn[0]);
        unitsToTakeTurn.RemoveAt(0);
        
        theUnit.actionDone = false;
        for (int i = 0; i < unitsInCombat.Count; ++i)
        {
            unitsInCombat[i].targeted = false;
        }
        */
        for(int i = 0; i < unitsToTakeTurn.Count; ++i)
        {
            if (unitsToTakeTurn[i].actionDone == true)
            {
                BaseUnitScript theUnit = unitsToTakeTurn[i];
                unitsToTakeTurn.RemoveAt(i);
                theUnit.myTurn = false;
                theUnit.actionDone = false;
                theUnit.turnTracker = 0;
                theUnit.managedOTEffects = false;
            }
        }

    }

    public void removeUnitFromCombat()
    {
        List<BaseUnitScript> deleteMe = new List<BaseUnitScript>();

        for(int i = 0; i < unitsInCombat.Count; i++)
        {
            if (unitsInCombat[i].Health <= 0)
            {
                deleteMe.Add(unitsInCombat[i]);
                unitsInCombat.RemoveAt(i);                
            }
        }

        enemies.Clear();
        for (int i = 0; i < unitsInCombat.Count; ++i)
        {
            if (!unitsInCombat[i].isPlayer)
                enemies.Add(unitsInCombat[i]);
        }

        unitsToTakeTurn.Clear();
        for(int i = 0; i < unitsInCombat.Count; ++i)
        {
            if (unitsInCombat[i].turnTracker >= timeBeforeTurn)
                unitsToTakeTurn.Add(unitsInCombat[i]);
        }

        if(deleteMe.Count > 0)
        for(int i = 0; i < deleteMe.Count; ++i)
        {
                Destroy(deleteMe[i].healthbar);
                Destroy(deleteMe[i].turnbar);
                Destroy(deleteMe[i].gameObject);
        }

    }

    public void updateCombatLog()
    {
        if (combatLog.Count > 3)
            combatLog.RemoveAt(0);

        textBox.text = "";

        for (int i = 0; i < combatLog.Count; ++i)
        {
            textBox.text += combatLog[i] + "\n";
        }
    }
    public void killAllPlayers()
    {
        for(int i  = 0; i < players.Count; ++i)
        {
            players[i].Health = 0;
        }
    }
}
