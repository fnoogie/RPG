using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OverTimeEffect
{
    //name of effect
    public string effectName;
    //duration of effect
    public int duration;
    //strength of effect
    public float strength;
    //what stat to effect
    public StatEffect statEffect;
    //how the the strength scale over time
    public float effectScaling;
    //effect should undo itself when it times out
    public bool reverseAtEnd;

    //should the effect be applied only once or every turn
    public bool applyOnce;
    public bool applied; //bool for if it has already been applied

};

public class BaseUnitScript : MonoBehaviour
{
    [HideInInspector] public CombatManagerScript combatManager;

    //used for turn tracking
    [HideInInspector]
    public bool myTurn = false;
    //[HideInInspector]
    public float turnTracker = 0;
    public bool isPlayer = false, playerControlled = false;


    [Tooltip("The base stats of the unit")]
    public float Health = 100, Mana = 100, Speed = 1, Damage = 5, Defense = 2, Int = 5;

    [Tooltip("The healthbar GameObject this unit will connect to")]
    public GameObject healthbar;
    public GameObject manabar;
    Slider manaSlider;
    [Tooltip("The bar GameObject to track this units speed")]
    public GameObject turnbar;
    Slider turnSlider;

    //used for targeting enemies
    int target = 0;
    [HideInInspector]
    public bool targeted = false;
    GameObject targetingArrow;

    [HideInInspector]
    public List<UnitAbility> customAbilities;
    [HideInInspector]
    public List<string> Actions;
    int actionSelection = 0;
    bool actionSelected = false;
    bool delayed = false;

    [HideInInspector]
    public bool managedOTEffects = false;
    public List<OverTimeEffect> OverTimeEffects;

    [HideInInspector]
    public bool actionDone = false, decisionMade = false;

    public TextMeshProUGUI upperAction, currentAction, lowerAction;
    public Color actionTextColor;

    void Start()
    {
        turnTracker = 0;
        targetingArrow = gameObject.transform.GetChild(0).gameObject;
        targetingArrow.SetActive(targeted);

        turnSlider = turnbar.GetComponent<Slider>();
        manaSlider = manabar.GetComponent<Slider>();
        Actions.Add("Attack");
        UnitAbility theAbility;
        if (gameObject.TryGetComponent<UnitAbility>(out theAbility) != false)
        {
            UnitAbility[] abilitiesOnUnit = gameObject.GetComponents<UnitAbility>();
            for (int i = 0; i < abilitiesOnUnit.Length; ++i)
            {
                customAbilities.Add(abilitiesOnUnit[i]);
            }
            for (int i = 0; i < customAbilities.Count; ++i)
            {
                Actions.Add(customAbilities[i].AbilityName);
            }
        }
        Actions.Add("Pass");

        OverTimeEffects = new List<OverTimeEffect>();

    }

    // Update is called once per frame
    void Update()
    {
        //update the healthbar based on the unit's health
        if (isPlayer)
            healthbar.GetComponent<PlayerHealth>().Health = Health;
        else if (!isPlayer)
            healthbar.GetComponent<EnemyHealth>().Health = Health;

        targetingArrow.SetActive(targeted);

        //update the unit's turn tracker bar
        turnSlider.value = turnTracker;
        manaSlider.value = Mana;

        if(myTurn && Health > 0 && !managedOTEffects)
            manageOTEffects();

        Mana = Mathf.Clamp(Mana, 0, 100);

        if (myTurn && Health > 0 && actionDone == false && decisionMade == false)
        {
            gameObject.transform.localScale = new Vector3(90, 90, 90);
            if (!delayed && !playerControlled)
                StartCoroutine(delay());
            else
            {
                if (isPlayer && playerControlled)
                {
                    //check if an action has been selected
                    if (!actionSelected)
                    {
                        if (Input.GetKeyDown(KeyCode.W))
                        {
                            actionSelection--;
                        }
                        else if (Input.GetKeyDown(KeyCode.S))
                        {
                            actionSelection++;
                        }
                        actionSelection = (int)loopClamp(actionSelection, 0, Actions.Count - 1);
                        upperAction.GetComponent<TextMeshProUGUI>().color = new Color(upperAction.GetComponent<TextMeshProUGUI>().color.r, upperAction.GetComponent<TextMeshProUGUI>().color.g, upperAction.GetComponent<TextMeshProUGUI>().color.b, .7f);
                        currentAction.GetComponent<TextMeshProUGUI>().color = new Color(currentAction.GetComponent<TextMeshProUGUI>().color.r, currentAction.GetComponent<TextMeshProUGUI>().color.g, currentAction.GetComponent<TextMeshProUGUI>().color.b, 1);
                        lowerAction.GetComponent<TextMeshProUGUI>().color = new Color(lowerAction.GetComponent<TextMeshProUGUI>().color.r, lowerAction.GetComponent<TextMeshProUGUI>().color.g, lowerAction.GetComponent<TextMeshProUGUI>().color.b, .7f);

                        upperAction.text = Actions[(int)loopClamp(actionSelection - 1, 0, Actions.Count - 1)];
                        currentAction.text = Actions[actionSelection];
                        lowerAction.text = Actions[(int)loopClamp(actionSelection + 1, 0, Actions.Count - 1)];
                        if (Input.GetKeyDown(KeyCode.E))
                        {
                            actionSelected = true;
                            upperAction.GetComponent<TextMeshProUGUI>().color = new Color(upperAction.GetComponent<TextMeshProUGUI>().color.r, upperAction.GetComponent<TextMeshProUGUI>().color.g, upperAction.GetComponent<TextMeshProUGUI>().color.b, 0);
                            currentAction.GetComponent<TextMeshProUGUI>().color = new Color(currentAction.GetComponent<TextMeshProUGUI>().color.r, currentAction.GetComponent<TextMeshProUGUI>().color.g, currentAction.GetComponent<TextMeshProUGUI>().color.b, 0);
                            lowerAction.GetComponent<TextMeshProUGUI>().color = new Color(lowerAction.GetComponent<TextMeshProUGUI>().color.r, lowerAction.GetComponent<TextMeshProUGUI>().color.g, lowerAction.GetComponent<TextMeshProUGUI>().color.b, 0);
                        }
                    }
                    else
                    {
                        //if 'Attack' is selected
                        if (actionSelection == 0)
                        {
                            if (Input.GetKeyDown(KeyCode.W))
                            {
                                combatManager.enemies[target].targeted = false;
                                target--;
                            }
                            else if (Input.GetKeyDown(KeyCode.S))
                            {
                                combatManager.enemies[target].targeted = false;
                                target++;
                            }
                            target = (int)loopClamp(target, 0, combatManager.enemies.Count - 1);
                            combatManager.enemies[target].targeted = true;
                            if (Input.GetKeyDown(KeyCode.E))
                                confirmAction(Actions[actionSelection], combatManager.enemies[target]);
                        }
                        //if 'Pass' is selected
                        else if (actionSelection == Actions.Count - 1)
                            confirmAction(Actions[actionSelection], this);
                        //if any custom ability is selected
                        else
                        {
                            UnitAbility ability = customAbilities[actionSelection - 1];

                            if (Mana - ability.ManaCost < 0)
                            {
                                Destroy(ability);
                                actionSelected = false;
                                return;
                            }
                            switch (ability.AbilityTarget)
                            {
                                case Targeting.Self:
                                    {
                                        this.targeted = true;

                                        if (Input.GetKeyDown(KeyCode.E))
                                            confirmAction(ability, this);
                                        break;
                                    }
                                case Targeting.Ally:
                                    {
                                        if (Input.GetKeyDown(KeyCode.W))
                                        {
                                            combatManager.players[target].targeted = false;
                                            target--;
                                        }
                                        else if (Input.GetKeyDown(KeyCode.S))
                                        {
                                            combatManager.players[target].targeted = false;
                                            target++;
                                        }
                                        target = (int)loopClamp(target, 0, combatManager.enemies.Count - 1);
                                        combatManager.players[target].targeted = true;

                                        if (Input.GetKeyDown(KeyCode.E))
                                            confirmAction(ability, combatManager.players[target]);
                                        break;
                                    }
                                case Targeting.AllAllies:
                                    {
                                        foreach (BaseUnitScript unit in combatManager.players)
                                            unit.targeted = true;

                                        if (Input.GetKeyDown(KeyCode.E))
                                            confirmAction(ability, combatManager.players);
                                        break;
                                    }
                                case Targeting.Enemy:
                                    {
                                        if (Input.GetKeyDown(KeyCode.W))
                                        {
                                            combatManager.enemies[target].targeted = false;
                                            target--;
                                        }
                                        else if (Input.GetKeyDown(KeyCode.S))
                                        {
                                            combatManager.enemies[target].targeted = false;
                                            target++;
                                        }
                                        target = (int)loopClamp(target, 0, combatManager.enemies.Count - 1);
                                        combatManager.enemies[target].targeted = true;

                                        if (Input.GetKeyDown(KeyCode.E))
                                            confirmAction(ability, combatManager.enemies[target]);
                                        break;
                                    }
                                case Targeting.AllEnemies:
                                    {
                                        foreach (BaseUnitScript unit in combatManager.enemies)
                                            unit.targeted = true;

                                        if (Input.GetKeyDown(KeyCode.E))
                                            confirmAction(ability, combatManager.enemies);
                                        break;
                                    }
                                case Targeting.AllExceptCaster:
                                    {
                                        List<BaseUnitScript> allButMe = new List<BaseUnitScript>();
                                        foreach (BaseUnitScript unit in combatManager.unitsInCombat)
                                        {
                                            if (unit != this)
                                            {
                                                allButMe.Add(unit);
                                                unit.targeted = true;
                                            }
                                        }

                                        if (Input.GetKeyDown(KeyCode.E))
                                            confirmAction(ability, allButMe);
                                        break;
                                    }
                                case Targeting.AllUnits:
                                    {
                                        foreach (BaseUnitScript unit in combatManager.unitsInCombat)
                                            unit.targeted = true;

                                        if (Input.GetKeyDown(KeyCode.E))
                                            confirmAction(ability, combatManager.unitsInCombat);
                                        break;
                                    }
                            }

                        }

                        if (Input.GetKeyDown(KeyCode.Q))
                        {
                            foreach (BaseUnitScript unit in combatManager.unitsInCombat)
                                unit.targeted = false;
                            actionSelected = false;
                        }
                    }
                }
                //allies actions in combat
                else if (isPlayer)
                {
                    int randomTarget = Random.Range(0, combatManager.enemies.Count);
                    confirmAction(Actions[actionSelection], combatManager.enemies[randomTarget]);

                }
                //enemy actions in combat
                else
                {
                    int randomTarget = Random.Range(0, combatManager.players.Count);
                    confirmAction("Attack", combatManager.players[randomTarget]);
                    myTurn = false;
                }
            }
        }
    }
    //if action is basic attack or pass
    void confirmAction(string action, BaseUnitScript actionTarget)
    {
        foreach (BaseUnitScript unit in combatManager.unitsInCombat)
            unit.targeted = false;

        if (action.Equals("Attack"))
            basicAttack(actionTarget);
        else if (action.Equals("Pass"))
        {
        }

        combatManager.unitEndTurn();
        actionSelected = false;
        actionSelection = 0;
        actionDone = true;
        delayed = false;
    }

    void confirmAction(UnitAbility ability, BaseUnitScript unit, bool firstCall = true)
    {
        
        switch (ability.StatAffected)
        {
            case StatEffect.Health:
                {
                    if (ability.FlatValue)
                        unit.GetComponent<BaseUnitScript>().Health -= ability.FlatStrength;
                    else
                        switch (ability.StatBasedOn)
                        {
                            case StatBased.Health:
                                {
                                    unit.GetComponent<BaseUnitScript>().Health -= this.Health * ability.StatBasedMultiplier;
                                    break;
                                }
                            case StatBased.Damage:
                                {
                                    unit.GetComponent<BaseUnitScript>().Health -= this.Damage * ability.StatBasedMultiplier;
                                    break;
                                }
                            case StatBased.Defense:
                                {
                                    unit.GetComponent<BaseUnitScript>().Health -= this.Defense * ability.StatBasedMultiplier;
                                    break;
                                }
                            case StatBased.Speed:
                                {
                                    unit.GetComponent<BaseUnitScript>().Health -= this.Speed * ability.StatBasedMultiplier;
                                    break;
                                }
                        }
                    break;
                }
            case StatEffect.Damage:
                {
                    if (ability.FlatValue)
                        unit.GetComponent<BaseUnitScript>().Damage -= ability.FlatStrength;
                    else
                        switch (ability.StatBasedOn)
                        {
                            case StatBased.Health:
                                {
                                    unit.GetComponent<BaseUnitScript>().Damage -= this.Health * ability.StatBasedMultiplier;
                                    break;
                                }
                            case StatBased.Damage:
                                {
                                    unit.GetComponent<BaseUnitScript>().Damage -= this.Damage * ability.StatBasedMultiplier;
                                    break;
                                }
                            case StatBased.Defense:
                                {
                                    unit.GetComponent<BaseUnitScript>().Damage -= this.Defense * ability.StatBasedMultiplier;
                                    break;
                                }
                            case StatBased.Speed:
                                {
                                    unit.GetComponent<BaseUnitScript>().Damage -= this.Speed * ability.StatBasedMultiplier;
                                    break;
                                }
                        }
                    break;
                }
            case StatEffect.Defense:
                {
                    if (ability.FlatValue)
                        unit.GetComponent<BaseUnitScript>().Defense -= ability.FlatStrength;
                    else
                        switch (ability.StatBasedOn)
                        {
                            case StatBased.Health:
                                {
                                    unit.GetComponent<BaseUnitScript>().Defense -= this.Health * ability.StatBasedMultiplier;
                                    break;
                                }
                            case StatBased.Damage:
                                {
                                    unit.GetComponent<BaseUnitScript>().Defense -= this.Damage * ability.StatBasedMultiplier;
                                    break;
                                }
                            case StatBased.Defense:
                                {
                                    unit.GetComponent<BaseUnitScript>().Defense -= this.Defense * ability.StatBasedMultiplier;
                                    break;
                                }
                            case StatBased.Speed:
                                {
                                    unit.GetComponent<BaseUnitScript>().Defense -= this.Speed * ability.StatBasedMultiplier;
                                    break;
                                }
                        }
                    break;
                }
            case StatEffect.Speed:
                {
                    if (ability.FlatValue)
                        unit.GetComponent<BaseUnitScript>().Speed -= ability.FlatStrength;
                    else
                        switch (ability.StatBasedOn)
                        {
                            case StatBased.Health:
                                {
                                    unit.GetComponent<BaseUnitScript>().Speed -= this.Health * ability.StatBasedMultiplier;
                                    break;
                                }
                            case StatBased.Damage:
                                {
                                    unit.GetComponent<BaseUnitScript>().Speed -= this.Damage * ability.StatBasedMultiplier;
                                    break;
                                }
                            case StatBased.Defense:
                                {
                                    unit.GetComponent<BaseUnitScript>().Speed -= this.Defense * ability.StatBasedMultiplier;
                                    break;
                                }
                            case StatBased.Speed:
                                {
                                    unit.GetComponent<BaseUnitScript>().Speed -= this.Speed * ability.StatBasedMultiplier;
                                    break;
                                }
                        }
                    break;
                }
            case StatEffect.CurrentTurnTracker:
                {
                    if (ability.FlatValue)
                        unit.GetComponent<BaseUnitScript>().turnTracker -= ability.FlatStrength;
                    else
                        switch (ability.StatBasedOn)
                        {
                            case StatBased.Health:
                                {
                                    unit.GetComponent<BaseUnitScript>().turnTracker -= this.Health * ability.StatBasedMultiplier;
                                    break;
                                }
                            case StatBased.Damage:
                                {
                                    unit.GetComponent<BaseUnitScript>().turnTracker -= this.Damage * ability.StatBasedMultiplier;
                                    break;
                                }
                            case StatBased.Defense:
                                {
                                    unit.GetComponent<BaseUnitScript>().turnTracker -= this.Defense * ability.StatBasedMultiplier;
                                    break;
                                }
                            case StatBased.Speed:
                                {
                                    unit.GetComponent<BaseUnitScript>().turnTracker -= this.Speed * ability.StatBasedMultiplier;
                                    break;
                                }
                        }
                    break;
                }
        }

        //check if ability applies an OT
        if (firstCall)
        {
            Mana -= ability.ManaCost;
            if (ability.OverTimeEffect)
            {
                switch (ability.OTTarget)
                {
                    case OTTargeting.Caster:
                        {
                            OverTimeEffect effect = new OverTimeEffect();
                            effect.effectName = ability.AbilityName;
                            effect.duration = ability.OTDuration;
                            effect.strength = ability.OTStrength;
                            effect.effectScaling = ability.OTStrengthScaling;
                            effect.statEffect = ability.OTStatEffect;
                            effect.applyOnce = ability.applyOnce;
                            effect.reverseAtEnd = ability.reverseAtEnd;
                            this.OverTimeEffects.Add(effect);
                            break;
                        }
                    case OTTargeting.Target:
                        {
                            OverTimeEffect effect = new OverTimeEffect();
                            effect.effectName = ability.AbilityName;
                            effect.duration = ability.OTDuration;
                            effect.strength = ability.OTStrength;
                            effect.effectScaling = ability.OTStrengthScaling;
                            effect.statEffect = ability.OTStatEffect;
                            effect.applyOnce = ability.applyOnce;
                            effect.reverseAtEnd = ability.reverseAtEnd;
                            unit.OverTimeEffects.Add(effect);
                            break;
                        }
                    case OTTargeting.AllAllies:
                        {
                            foreach (BaseUnitScript ally in combatManager.players)
                            {
                                OverTimeEffect effect = new OverTimeEffect();
                                effect.effectName = ability.AbilityName;
                                effect.duration = ability.OTDuration;
                                effect.strength = ability.OTStrength;
                                effect.effectScaling = ability.OTStrengthScaling;
                                effect.statEffect = ability.OTStatEffect;
                                effect.applyOnce = ability.applyOnce;
                                effect.reverseAtEnd = ability.reverseAtEnd;
                                ally.OverTimeEffects.Add(effect);
                            }
                            break;
                        }
                    case OTTargeting.AllEnemies:
                        {
                            foreach (BaseUnitScript enemy in combatManager.enemies)
                            {
                                OverTimeEffect effect = new OverTimeEffect();
                                effect.effectName = ability.AbilityName;
                                effect.duration = ability.OTDuration;
                                effect.strength = ability.OTStrength;
                                effect.effectScaling = ability.OTStrengthScaling;
                                effect.statEffect = ability.OTStatEffect;
                                effect.applyOnce = ability.applyOnce;
                                effect.reverseAtEnd = ability.reverseAtEnd;
                                enemy.OverTimeEffects.Add(effect);
                            }
                            break;
                        }
                    case OTTargeting.AllUnits:
                        {
                            foreach (BaseUnitScript baseUnit in combatManager.unitsInCombat)
                            {
                                OverTimeEffect effect = new OverTimeEffect();
                                effect.effectName = ability.AbilityName;
                                effect.duration = ability.OTDuration;
                                effect.strength = ability.OTStrength;
                                effect.effectScaling = ability.OTStrengthScaling;
                                effect.statEffect = ability.OTStatEffect;
                                effect.applyOnce = ability.applyOnce;
                                effect.reverseAtEnd = ability.reverseAtEnd;
                                baseUnit.OverTimeEffects.Add(effect);
                            }
                            break;
                        }
                }
            }
        }

        combatManager.unitEndTurn();
        actionSelected = false;
        actionSelection = 0;
        actionDone = true;
        managedOTEffects = false;
        delayed = false;
    }
    void confirmAction(UnitAbility ability, List<BaseUnitScript> actionTarget)
    {
        bool firstCall = true;
        foreach(BaseUnitScript theUnit in actionTarget)
        {
            confirmAction(ability, theUnit, firstCall);
            firstCall = false;
        }

        combatManager.unitEndTurn();
        actionSelected = false;
        actionSelection = 0;
        actionDone = true;
        managedOTEffects = false;
        delayed = false;
    }


    void basicAttack(BaseUnitScript target)
    {
        StartCoroutine(attackAnimation(target.gameObject));
    }
    void waitThenBasicAttack(BaseUnitScript target)
    {
        StartCoroutine(waitThenAttackAnimation(target.gameObject));
    }

    IEnumerator delay()
    {
        yield return new WaitForSeconds(Random.Range(1,3));
        delayed = true;
    }
    IEnumerator waitThenAttackAnimation(GameObject target)
    {
        yield return new WaitForSeconds(Random.Range(2, 5));
        attackAnimation(target);
    }

    IEnumerator attackAnimation(GameObject target)
    {
        combatManager.combatLog.Add(gameObject.name + " attacked " + target.name + " for " + Mathf.Max(0, (Damage - target.GetComponent<BaseUnitScript>().Defense)) + " damage");
        combatManager.updateCombatLog();

        Vector3 direction = new Vector3(target.transform.position.x - this.gameObject.transform.position.x, target.transform.position.y - this.gameObject.transform.position.y, target.transform.position.z - this.gameObject.transform.position.z);
        direction.Normalize();
        direction *= 20;
        gameObject.transform.position += direction;
        yield return new WaitForSeconds(0.1f);
        gameObject.transform.position -= direction;
        target.GetComponent<BaseUnitScript>().Health -= Mathf.Max(0, (Damage - target.GetComponent<BaseUnitScript>().Defense));
        target.GetComponent<BaseUnitScript>().targeted = false;

        gameObject.transform.localScale = new Vector3(75, 75, 75);
        actionDone = true;
        decisionMade = false;
    }

    IEnumerator allAttackAnimation(GameObject target)
    {
        if(isPlayer)
            combatManager.combatLog.Add(gameObject.name + " attacked all enemies");
        else
            combatManager.combatLog.Add(gameObject.name + " attacked all allies");
        combatManager.updateCombatLog();

        Vector3 direction = new Vector3(target.transform.position.x - this.gameObject.transform.position.x, target.transform.position.y - this.gameObject.transform.position.y, target.transform.position.z - this.gameObject.transform.position.z);
        direction.Normalize();
        direction *= 20;
        gameObject.transform.position += direction;
        yield return new WaitForSeconds(0.1f);
        gameObject.transform.position -= direction;

        for (int i = 0; i < combatManager.enemies.Count; ++i)
        {
            combatManager.enemies[i].Health -= Mathf.Max(0, ( (Damage * 0.75f) - combatManager.enemies[i].Defense) );
        }

        target.GetComponent<BaseUnitScript>().targeted = false;
        gameObject.transform.localScale = new Vector3(75, 75, 75);
        actionDone = true;
        decisionMade = false;
    }

    IEnumerator staminaAttackAnimation(GameObject target)
    {
        combatManager.combatLog.Add(gameObject.name + " winded " + target.name + " for " + Mathf.Max(1.5f, Mathf.Max(0, (Damage - target.GetComponent<BaseUnitScript>().Speed))) + " seconds");
        combatManager.updateCombatLog();

        Vector3 direction = new Vector3(target.transform.position.x - this.gameObject.transform.position.x, target.transform.position.y - this.gameObject.transform.position.y, target.transform.position.z - this.gameObject.transform.position.z);
        direction.Normalize();
        direction *= 20;
        gameObject.transform.position += direction;
        yield return new WaitForSeconds(0.1f);
        gameObject.transform.position -= direction;
        target.GetComponent<BaseUnitScript>().turnTracker -= Mathf.Max(1.5f, Mathf.Max(0,(Damage - target.GetComponent<BaseUnitScript>().Speed)));
        target.GetComponent<BaseUnitScript>().targeted = false;

        if (target.GetComponent<BaseUnitScript>().turnTracker < 0)
            target.GetComponent<BaseUnitScript>().turnTracker = 0;

        gameObject.transform.localScale = new Vector3(75, 75, 75);

        actionDone = true;
        decisionMade = false;
    }

    IEnumerator healAnimation(GameObject target)
    {
        combatManager.combatLog.Add(gameObject.name + " healed " + target.name + " for " + " " + Mathf.Max(5f, (2 * Defense)) + " health");
        combatManager.updateCombatLog();

        Vector3 direction = new Vector3(target.transform.position.x - this.gameObject.transform.position.x, target.transform.position.y - this.gameObject.transform.position.y, target.transform.position.z - this.gameObject.transform.position.z);
        direction.Normalize();
        direction *= 20;
        gameObject.transform.position += direction;
        yield return new WaitForSeconds(0.1f);
        gameObject.transform.position -= direction;
        target.GetComponent<BaseUnitScript>().Health += Mathf.Max(5f, (2 * Defense));
        target.GetComponent<BaseUnitScript>().targeted = false;

        if (target.GetComponent<BaseUnitScript>().Health > 100)
            target.GetComponent<BaseUnitScript>().Health = 100;

        gameObject.transform.localScale = new Vector3(75, 75, 75);
        actionDone = true;
        decisionMade = false;
    }

    void manageOTEffects()
    {
        Mana += Int;

        if (OverTimeEffects.Count < 0)
            return;

        for (int i = 0; i < OverTimeEffects.Count; ++i)
        {
            if (!OverTimeEffects[i].applyOnce || (OverTimeEffects[i].applyOnce && !OverTimeEffects[i].applied))
                switch (OverTimeEffects[i].statEffect)
                {
                    case StatEffect.Health:
                        {
                            Health -= OverTimeEffects[i].strength;
                            break;
                        }
                    case StatEffect.Damage:
                        {
                            Damage -= OverTimeEffects[i].strength;
                            break;
                        }
                    case StatEffect.Speed:
                        {
                            Speed -= OverTimeEffects[i].strength;
                            break;
                        }
                    case StatEffect.Defense:
                        {
                            Defense -= OverTimeEffects[i].strength;
                            break;
                        }
                    case StatEffect.CurrentTurnTracker:
                        {
                            turnTracker -= OverTimeEffects[i].strength;
                            break;
                        }
                }

            if (OverTimeEffects[i].applyOnce)
                OverTimeEffects[i].applied = true;
            OverTimeEffects[i].strength *= OverTimeEffects[i].effectScaling;

            //update duration of effect, and remove expired effects
            --OverTimeEffects[i].duration;
            Debug.Log(this.gameObject.name + ": " + OverTimeEffects[i].effectName + " has " + OverTimeEffects[i].duration + " turns left");
            if (OverTimeEffects[i].duration <= 0)
            {
                if (OverTimeEffects[i].reverseAtEnd)
                    switch (OverTimeEffects[i].statEffect)
                    {
                        case StatEffect.Health:
                            {
                                Health += OverTimeEffects[i].strength;
                                break;
                            }
                        case StatEffect.Damage:
                            {
                                Damage += OverTimeEffects[i].strength;
                                break;
                            }
                        case StatEffect.Speed:
                            {
                                Speed += OverTimeEffects[i].strength;
                                break;
                            }
                        case StatEffect.Defense:
                            {
                                Defense += OverTimeEffects[i].strength;
                                break;
                            }
                        case StatEffect.CurrentTurnTracker:
                            {
                                turnTracker += OverTimeEffects[i].strength;
                                break;
                            }
                    }


                OverTimeEffects.RemoveAt(i);
                --i;
            }
        }
        managedOTEffects = true;
        
    }


    float loopClamp(float num, float min, float max)
    {
        if (num < min)
            num = max;
        else if (num > max)
            num = min;

        return num;
    }

}
