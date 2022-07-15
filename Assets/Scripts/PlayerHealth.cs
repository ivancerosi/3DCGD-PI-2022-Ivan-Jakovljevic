using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour, IHitable
{
    public int maxHealth = 200;

    private int health;
    RectTransform healthbar;

    public void Hit(int damage)
    {
        health -= damage;
        if (health<=0)
        {
            health = 0;
            Reset();
        }
        healthbar.localScale = new Vector3((float)health/(float)maxHealth, healthbar.localScale.y, healthbar.localScale.z);
    }

    void Reset()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        healthbar = GameObject.Find("PlayerCanvas").transform.Find("Background").Find("Health").GetComponent<RectTransform>();
        health = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
    }
}
