using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionCheck : MonoBehaviour
{
    [SerializeField] private GameObject parent;

    // Start is called before the first frame update
    void Start()
    {

    }

    void OnTriggerStay(Collider collision)
    {
        if(collision.tag == "Player")
        {
            parent.GetComponent<SceneTransition>().Triggered();
        }
    }

    void OnTriggerExit(Collider collision)
    {
        if (collision.tag == "Player")
        {
            parent.GetComponent<SceneTransition>().UnTriggered();
        }
    }
}
