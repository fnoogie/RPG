using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public float Health = 100f;
    [SerializeField] GameObject Myself, healthbar;
    public Slider mSlider;
    [SerializeField] bool alive = true;
    // Start is called before the first frame update
    void Start()
    {
        mSlider.maxValue = 100.0f;
    }

    // Update is called once per frame
    void Update()
    {
        mSlider.value = Health;
        if (Health <= 0 && alive)
        {
            Myself.transform.Rotate(0.0f, 0.0f, 90.0f, Space.World);
            alive = false;
        }
        if (Health > 0 && alive == false)
        {
            Myself.transform.Rotate(0.0f, 0.0f, -90.0f, Space.World);
            alive = true;
        }
    }
}
