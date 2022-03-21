using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStalk : MonoBehaviour
{
    [SerializeField] private GameObject Player;
    [SerializeField] private float distance, spacing = 1;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        distance = Vector3.Distance(Player.transform.position, transform.position);
        if(distance > spacing)
        {
            gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, Player.transform.position, Time.deltaTime);
        }
    }
}
