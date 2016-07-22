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
    public event OnHealthChanged onHealthWasInvincible;     // Health was invincible but raise an event anyway
    public event OnHealthChanged onHealthGained;            // Health calculated that health raised
    public event OnHealthChanged onHealthZero;              // Health value reached zero 
    public event OnHealthChanged onHealthLost;              // Health lost value


    // Matt - Just an idea for resistances here so its contained within a Health
    // Can leave here for now to consider later as this will be important for 
    // shields and 'partial' damages like this 
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
        /// The final value of damage the Health component calculated from the damage
		/// this is different to the damage value itself
        /// </summary>
        public int value;

        /// <summary>
        /// The details of this damage
        /// </summary>		 
        public Damage damage;
      
		/// <summary>
        /// Game object from this attack
        /// </summary>
        public GameObject responsibleGameObject;

		/// <summary>
		/// Attack Controller from this attack
		/// </summary>
		public AttackController responsibleAttackController;
    }


	/// <summary>
	/// Call this to pass damage to the Health component. Optional to pass in
	/// a responsible GameObject or AttackController 
	/// </summary>
	public void TakeDamage(Damage damage, GameObject responsibleGameObject = null, AttackController responsibleAttackController = null)
    {
        if (damage == null)
            return;
		
        HealthChangedEventInfo info = new HealthChangedEventInfo();
        info.damage = damage;
        info.responsibleGameObject = responsibleGameObject;
		info.responsibleAttackController = responsibleAttackController;
        info.value = damage.amount;

        if (damage.isDamageOverTime)
            StartCoroutine(ApplyDamageToHealthOverTime(info));          
        else
            ApplyDamageToHealth(info);
    }

    private void ApplyDamageToHealth(HealthChangedEventInfo info)
    {
        // Time registers
        _lastDamageTime = Time.time;
        _lastDamageAmount = info.damage.amount;

        // Already below zero
        if (currentHealth <= 0)
            return;

        // No damage 
        if (info.damage.amount == 0)
            return;

        // Invincible
        if (invincible)
        {
            if (onHealthWasInvincible != null)
                onHealthWasInvincible(info);
            return;
        }


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

        // Adjust health value
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

    void OnEnable()
    {
        if(LevelManager.instance)
            LevelManager.Register(this);
    }

    void OnDestroy()
    {
        if(LevelManager.instance)
            LevelManager.DeRegister(this);
    }
}
