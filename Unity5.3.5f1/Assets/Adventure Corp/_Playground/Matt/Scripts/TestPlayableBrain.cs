using UnityEngine;
using System.Collections;

// Matt - we can use this namespace, but I feel it makes our code a little
// ambiguous with "player" now a class.  Having to use Rewired.Player helps
// with the readability there
//using Rewired;

public class TestPlayableBrain : PlayerBrain
{
    // ----   Shield/Parry test stuff ----
    public Transform shield;
    public float shieldMovementScale = 0.25f;

    public Material shieldMaterial;
    float shieldFlashCoolDownSpeed = 8f;
    float shieldFlashCurrent = 0;

    Color shieldColorHit = Color.blue;
    Color shieldColorParry = Color.green;
    Color shieldColorFlashCurrent = Color.black;

    public float shieldLastHoldTime;
    public float shieldParryTimeWindow = 0.2f;

    // ----------------------------------




    public LineRenderer lr;



    protected override void Awake()
    {
        shield = Helpers.InstantiateAndParent(shield, transform, true);
        shield.gameObject.SetActive(false);
        shieldMaterial = shield.GetComponentInChildren<MeshRenderer>().material;
        shieldLastHoldTime = -1f;
        base.Awake();
    }

    


    protected override void Update()
    {
        base.Update();
        UpdateRangedFocusing();
        UpdateShieldStuff();
    }


    // --------------------  SHIELD / PARRY TEST -------------------------------------

    void UpdateShieldStuff()
    {
        // Manages color of the shield for visual feedback
        shieldFlashCurrent -= Time.deltaTime * shieldFlashCoolDownSpeed;
        shieldFlashCurrent = Mathf.Clamp01(shieldFlashCurrent);
        shieldMaterial.SetColor("_EmissionColor", Color.Lerp(Color.yellow, shieldColorFlashCurrent, shieldFlashCurrent));

        // HACK - just set health to invincible whilst holding 
        // shield,  this is probably not what we want, but just to see some results
        agent.health.invincible = player.GetButton(INPUT_SHIELD);

        if (player.GetButtonDown(INPUT_SHIELD))
            shieldLastHoldTime = Time.time;

        if (player.GetButton(INPUT_SHIELD))
        {
            shield.gameObject.SetActive(true);
            agent.SetVelocityScaleThisFrame(shieldMovementScale);            
        }
        else
            shield.gameObject.SetActive(false);
    }

    protected override void OnHealthWasInvincible(Health.HealthChangedEventInfo info)
    {
        shieldFlashCurrent = 1f;
        if (player.GetButton(INPUT_SHIELD))
            agent.SetDesiredRotation(info.responsibleGameObject.transform);

        if ((Time.time - shieldLastHoldTime) <= shieldParryTimeWindow)
        {
            shieldColorFlashCurrent = shieldColorParry;

            // Test idea - attack when parry successful?
            AttackNowAsMelee(basicMelee);                        
        }
        else
            shieldColorFlashCurrent = shieldColorHit;

        base.OnHealthWasInvincible(info);
    }


    // ------------------------------------------------------------------------









    // --------------------  RANGED FOCUS TEST -------------------------------------

    float rangedFocusSpeed = 60f;
    float rangedStartAngle = 45f;
    float rangedDeFocusSpeed = 100f;
    float rangedCurrentAngle = 45;
    void UpdateRangedFocusing()
    {
        _inputAim.x = player.GetAxis(INPUT_AIM_HORIZONTAL);
        _inputAim.z = player.GetAxis(INPUT_AIM_VERTICAL);
        _inputAim = GrabMovementDirRelativeToCam(_inputAim);

        Vector3 p = Vector3.zero;
        if (_inputAim.magnitude > 0.01f)
        {
            agent.SetDesiredRotation(_inputAim);
            rangedCurrentAngle -= Time.deltaTime * rangedFocusSpeed;
            float x = Mathf.Sin(Mathf.Deg2Rad * rangedCurrentAngle);
            float z = Mathf.Cos(Mathf.Deg2Rad * rangedCurrentAngle);
            p = new Vector3(x, 0, z);
        }
        else
        {
            rangedCurrentAngle += Time.deltaTime * rangedDeFocusSpeed;
        }

        if (lr == null)
            return;

        float d = 5f;
        lr.SetPosition(0, p * d + Vector3.up);
        lr.SetPosition(1, Vector3.up);
        p.x = -p.x;
        lr.SetPosition(2, p * d + Vector3.up);

        rangedCurrentAngle = Mathf.Clamp(rangedCurrentAngle, 0f, rangedStartAngle);

        Color c = rangedCurrentAngle < 1f ? Color.green : Color.blue;
        lr.material.color = c;
    }

    // ------------------------------------------------------------------------





}
