using UnityEngine;
using System.Collections;


/// <summary>
/// LightFlicker.cs
/// A small component to attach to a light that will enable an assortment of 
/// properties to be changed at run-time.  Perfect for lights representing
/// fires and flames!
/// (c) Flame Shark Pty Ltd 2012
/// </summary>
[RequireComponent(typeof(Light))]
[AddComponentMenu("Scripts/FlameShark/LightFlicker")]
public class LightFlicker : MonoBehaviour
{    

    // intensity
    public float intensityVariance;
    public float intensitySpeed;    

    // range
    public float rangeVariance;
    public float rangeSpeed;
    
    // position with xyz locks
    public float positionVariance;
    public float positionSpeed;    
    public bool moveAlongX;
    public bool moveAlongY;
    public bool moveAlongZ;

    // color array
    public Color[] colorVariances;
    public float colorSpeed;

    




    // original values of the light    
    private float originalIntensity;
    private float originalRange;
    private Vector3 originalPosition;

    // The next set of values to move toward
    private float newIntensity;
    private Color newColor;
    private float newRange;
    private Vector3 newPosition;

    // The last time a value was changed
    private float lastIntensity = -999;
    private float lastColor = -999;
    private uint lastColorIndex = 0;
    private float lastRange = -999;
    private float lastPosition = -999;

    // for cheaper reference
    private Transform myTransform;

    void Start()
    {
        //originalColor = light.color;
        originalIntensity = GetComponent<Light>().intensity;
        originalPosition = transform.position;
        originalRange = GetComponent<Light>().range;

        myTransform = transform;
    }

    void OnBecameVisible() { enabled = true; }
    void OnBecameInvisible() { enabled = false; }

    void Update()
    {
        // Intensity
        if (intensitySpeed > 0f)
        {
            if ((Time.time - lastIntensity) > (1f / intensitySpeed))
            {
                newIntensity = Random.Range(originalIntensity - intensityVariance, originalIntensity + intensityVariance);
                lastIntensity = Time.time;
            }
            GetComponent<Light>().intensity = Mathf.Lerp(GetComponent<Light>().intensity, newIntensity, Time.deltaTime * intensitySpeed);
        }

        // Range
        if (rangeSpeed > 0f)
        {
            if ((Time.time - lastRange) > (1f / rangeSpeed))
            {
                newRange = Random.Range(originalRange - rangeVariance, originalRange + rangeVariance);
                lastRange = Time.time;
            }
            GetComponent<Light>().range = Mathf.Lerp(GetComponent<Light>().range, newRange, Time.deltaTime * rangeSpeed);
        }

        // Color
        if (colorSpeed > 0f && colorVariances.Length > 0)
        {
            if ((Time.time - lastColor) > (1f / colorSpeed))
            {
                newColor = colorVariances[lastColorIndex++ % colorVariances.Length];
                lastColor = Time.time;
            }
            GetComponent<Light>().color = Color.Lerp(GetComponent<Light>().color, newColor, Time.deltaTime * colorSpeed);
        }

        // Position
        if (positionSpeed > 0f)
        {
            if ((Time.time - lastPosition) > (1f / positionSpeed))
            {
                newPosition = new Vector3(Random.Range(originalPosition.x - positionVariance, originalPosition.x + positionVariance),
                                            Random.Range(originalPosition.y - positionVariance, originalPosition.y + positionVariance),
                                            Random.Range(originalPosition.z - positionVariance, originalPosition.z + positionVariance));
                newPosition.x = moveAlongX ? newPosition.x : originalPosition.x;
                newPosition.y = moveAlongY ? newPosition.y : originalPosition.y;
                newPosition.z = moveAlongZ ? newPosition.z : originalPosition.z;
                lastPosition = Time.time;
            }
            myTransform.position = Vector3.Lerp(myTransform.position, newPosition, Time.deltaTime * positionSpeed);
        }
    }
}
