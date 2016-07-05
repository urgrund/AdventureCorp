using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class Health : MonoBehaviour
{
    public bool invincible = false;
    public int startingHealth = 100;
    public int maxHealth = 100;

    public int _currentHealth;
    public int currentHealth { get { return _currentHealth; } }

    private float _lastDamageAmount;
    public float lastDamageAmount { get { return _lastDamageAmount; } }

    private float _lastDamageTime;
    public float lastDamageTime { get { return _lastDamageTime; } }

    public delegate void OnHealthGained(int gain);
    public event OnHealthGained onHealthGained;

    public delegate void OnHealthZero(int amountBelowZero);
    public event OnHealthZero onHealthZero;

    public delegate void OnHealthLost(int loss);
    public event OnHealthLost onHealthLost;


    // Matt
    // Idea for resistances here so its contained within a Health
    [System.Serializable]
    public class Resistance
    {
        public DamageType type = DamageType.Generic;
        public int resistance = 0;        
    }
    //public List<Resistance> resistances = new List<Resistance>();
    
    
    public void TakeDamage(Damage damage, GameObject attacker)
    {        
        if (damage.isDamageOverTime)
            StartCoroutine(DamageOverTime(damage));          
        else
            ApplyDamageToHealth(damage);
    }

    void ApplyDamageToHealth(Damage damage)
    {
        // Already below zero
        if (currentHealth <= 0)
            return;

        // If invincible or no amount
        if (invincible || damage.amount == 0)
            return;

        _lastDamageTime = Time.time;
        _lastDamageAmount = damage.amount;

        // Fire events
        if (damage.amount < 0)
        {
            if (onHealthGained != null)
                onHealthGained(damage.amount);
        }
        else
        {
            if (onHealthLost != null)
                onHealthLost(damage.amount);
        }

        _currentHealth -= damage.amount;

        if (_currentHealth <= 0)
        {
            _currentHealth = 0;
            if (onHealthZero != null)
                onHealthZero(_currentHealth);
        }
    }

    void Start()
    {
        if (startingHealth > maxHealth)
            startingHealth = maxHealth;

        _currentHealth = startingHealth;

        LevelManager.Register(this);
    }

    IEnumerator DamageOverTime(Damage damage)
    {
        float t = Time.time;
        while (Time.time - t < damage.damageOverTimeDuration)
        {
            ApplyDamageToHealth(damage);
            yield return new WaitForSeconds(damage.damageOverTimeFrequency);
        }
    }

    void OnDestroy()
    {
        if(LevelManager.instance)
            LevelManager.DeRegister(this);
    }
}
