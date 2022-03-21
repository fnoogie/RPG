using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class turnSliderDefaultValues : MonoBehaviour
{
    public CombatManagerScript combatManager;
    // Start is called before the first frame update
    void Start()
    {
        gameObject.GetComponent<Slider>().maxValue = combatManager.timeBeforeTurn;
        gameObject.GetComponent<Slider>().value = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
