using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private string scene;
    [SerializeField] private int triggerChance = 5;
    [SerializeField] private float frequencyProc = 0.5f;
    [SerializeField] private bool hunt = false;
    [SerializeField] private int MercyCap = 4;
    [SerializeField] private int Mercy = 0;
    private float number;
    private bool huntRoutine = false;
    private bool mercyRoutine = false;
    [SerializeField] private GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        Mercy = MercyCap;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    

    public void Triggered()
    {
        if(hunt == false)
        {
            hunt = true;
            //Debug.Log("Triggered");
            if(huntRoutine == false)
            {
                huntRoutine = true;
                StartCoroutine(EnemyChance());
            }
        }
    }

    public void UnTriggered()
    {
        if (hunt)
        {
            //Debug.Log("UnTriggered");
            hunt = false;
            if(mercyRoutine == false)
            {
                mercyRoutine = true;
                StartCoroutine(HuntCheck());
            }
        }
    }

    IEnumerator EnemyChance()
    {
        while (hunt)
        {
            //yield return new WaitForSeconds(frequencyProc);
            number = Random.Range(0.0f, 100.0f);
            if (player.GetComponent<Movement>().moving)
            {
            
                if (number < triggerChance && Mercy < 0)
                {
                    Debug.Log(number.ToString() + " StartEncounter");
                    SceneManager.LoadScene(scene);
                }
                else
                {
                    Debug.Log("EnemiesGone");
                    Mercy--;
                }
            }
            yield return new WaitForSeconds(frequencyProc);
        }
        //Debug.Log("EndHunt");
        huntRoutine = false;
    }

    IEnumerator HuntCheck()
    {
        yield return new WaitForSeconds(frequencyProc);
        if (hunt == false)
        {
            Mercy = MercyCap;
            Debug.Log("Mercy");
        }
        mercyRoutine = false;
    }
}
