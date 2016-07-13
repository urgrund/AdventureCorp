using UnityEngine;
using System.Collections;
using System.Collections.Generic;



/// <summary>
/// Health
/// Class that manages a health value by coupling itself with the Damage class
/// which is evaluated to determine how much health is lost at a particular event 
/// </summary>
public class Health : MonoBehaviour
{
    public bool invincible = false;
    public int startingHealth = 100;
    public int maxHealth = 100;

    [SerializeField]
    private int _currentHealth;
    public int currentHealth { get { return _currentHealth; } }

    private float _lastDamageAmount;
    public float lastDamageAmount { get { return _lastDamageAmount; } }

    private float _lastDamageTime;
    public float lastDamageTime { get { return _lastDamageTime; } }

    
    public bool isDead { get { return _currentHealth <= 0; } }


    public delegate void OnHealthChanged(HealthChangedEventInfo info);
    public event OnHealthChanged onHealthGained;
    public event OnHealthChanged onHealthZero;
    public event OnHealthChanged onHealthLost;


    // Matt - Just an idea for resistances here so its contained within a Health
    // Can leave here for now to consider later
    [System.Serializable]
    public class Resistance
    {
        public DamageType type = DamageType.Generic;
        public int resistance = 0;        
    }
    
    /// <summary>
    /// A class that is passed along with events related to Health
    /// </summary>
    public class HealthChangedEventInfo
    {
        /// <summary>
        /// The final value of damage the Health component calculated 
        /// </summary>
        public int value;
        /// <summary>
        /// The details of this damage
        /// </summary>
        public Damage damage;
        /// <summary>
        /// Object responsible for passing the damage object
        /// </summary>
        public GameObject responsibleGameObject; 
    }



    public void TakeDamage(Damage damage) { TakeDamage(damage, null); }
    public void TakeDamage(Damage damage, GameObject responsibleGameObject)
    {
        if (damage == null)
            return;

        HealthChangedEventInfo info = new HealthChangedEventInfo();
        info.damage = damage;
        info.responsibleGameObject = responsibleGameObject;
        info.value = damage.amount;

        if (damage.isDamageOverTime)
            StartCoroutine(ApplyDamageToHealthOverTime(info));          
        else
            ApplyDamageToHealth(info);
    }

    private void ApplyDamageToHealth(HealthChangedEventInfo info)
    {
        // Already below zero
        if (currentHealth <= 0)
            return;

        // If invincible or no amount
        if (invincible || info.damage.amount == 0)
            return;

        _lastDamageTime = Time.time;
        _lastDamageAmount = info.damage.amount;

        // Fire events
        if (info.damage.amount < 0)
        {
            if (onHealthGained != null)
                onHealthGained(info);
        }
        else
        {
            if (onHealthLost != null)
                onHealthLost(info);
        }

        _currentHealth -= info.damage.amount;

        if (_currentHealth <= 0)
        {
            _currentHealth = 0;
            if (onHealthZero != null)
                onHealthZero(info);
        }
    }

    void Start()
    {
        if (startingHealth > maxHealth)
            startingHealth = maxHealth;

        _currentHealth = startingHealth;

        LevelManager.Register(this);
    }

    IEnumerator ApplyDamageToHealthOverTime(HealthChangedEventInfo info)
    {
        float t = Time.time;
        while (Time.time - t < info.damage.damageOverTimeDuration)
        {
            ApplyDamageToHealth(info);
            yield return new WaitForSeconds(info.damage.damageOverTimeFrequency);
        }
    }

    void OnDestroy()
    {
        if(LevelManager.instance)
            LevelManager.DeRegister(this);
    }
}
