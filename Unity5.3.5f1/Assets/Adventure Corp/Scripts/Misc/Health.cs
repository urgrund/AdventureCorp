using UnityEngine;
using System.Collections;

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


    // TODO
    // The 'attacker' could be a struct containing 
    // attack info (who was it, whats the damage, etc)
    // and the health component evaluates it
    public void TakeDamage(int amount, GameObject attacker)
    {
        if (currentHealth <= 0)
            return;
        // If invincible or no amount
        if (invincible || amount == 0)
            return;

        _lastDamageTime = Time.time;
        _lastDamageAmount = amount;

        // Fire events
        if (amount < 0)
        {
            if (onHealthGained != null)
                onHealthGained(amount);
        }
        else
        {
            if (onHealthLost != null)
                onHealthLost(amount);
        }

        _currentHealth -= amount;

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

    void OnDestroy()
    {
        LevelManager.DeRegister(this);
    }
}
