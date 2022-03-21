using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    public float Health = 100f;
    [SerializeField] GameObject Myself, healthbar, CombatManager;
    public Slider mSlider;
    // Start is called before the first frame update
    void Start()
    {
        mSlider.maxValue = 100.0f;
    }

    // Update is called once per frame
    void Update()
    {
        mSlider.value = Health;
        if (Health <= 0)
        {
            CombatManager.GetComponent<CombatManagerScript>().removeUnitFromCombat();
        }
    }
}
