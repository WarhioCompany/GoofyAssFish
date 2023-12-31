using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ObjectScript : MonoBehaviour
{
    public GameObject dissolveParticles;
    public GameObject finalDissolvePrefab;
    public float disolveTime;
    public float mass;
    public float gravity;
    public float baseMass = 1;
    public float massMultiplier;

    public bool isPlayerAttached;
    public bool isWeapon;

    private Vector3 orgScale;
    private bool destroyed;
    private Rigidbody rb;

    [Header("Randomization Settings")]
    public float minScale;
    public float maxScale;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<EnemyMovement>())
        {
            other.GetComponent<EnemyMovement>().concussedTimer = 1;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        gravity = GameValues.objGravity;
        rb = GetComponent<Rigidbody>();
        Randomize();
    }

    // Update is called once per frame
    void Update()
    {
        // check for player
        if (GetComponentInChildren<SHITSpikeManager>())
        {
            isPlayerAttached = true;
        }
        else
        {
            isPlayerAttached = false;
        }

        // movement
        if (rb.velocity.magnitude <= mass * gravity)
        {
            rb.AddForce(Vector3.down * mass * (transform.position.y > GameObject.FindGameObjectWithTag("Player").transform.position.y && !isPlayerAttached ? gravity * 2 : gravity) * Time.deltaTime);
        }

        if (isPlayerAttached)
        {
            disolveTime -= Time.deltaTime;
            dissolveParticles.SetActive(true);
        }
        else
        {
            dissolveParticles.SetActive(false);
        }

        if(disolveTime <= 0)
        {
            //Destroy object (with animation)?
            float dissolveSpeed = 5;

            if (!destroyed)
            {
                Instantiate(finalDissolvePrefab, transform);
                destroyed = true;
            }

            if (GetComponentInChildren<SHITSpikeManager>())
            {
                GetComponentInChildren<SHITSpikeManager>().gameObject.transform.parent = null;
                GetComponentInChildren<SHITSpikeManager>().attachedSpike = null;
            }

            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, dissolveSpeed * Time.deltaTime);
            Destroy(gameObject, 2);
        }

        if (transform.position.y > (GameObject.FindGameObjectWithTag("Player").transform.position.y) + GameValues.maxObjHeightOffset 
            || transform.position.y < (GameObject.FindGameObjectWithTag("Player").transform.position.y) - GameValues.maxObjHeightOffset)

        {
            //Debug.Log("Deleted Object: " + gameObject.name);
            //Debug.Log((GameValues.height / GameValues.heightMeterRatio) + GameValues.maxObjHeightOffset);

            Destroy(gameObject);
        }
    }

    public void Randomize()
    {
        transform.localScale = getRndScale();
    }

    public Vector3 getRndScale()
    {
        Vector3 retVal = Vector3.one;

        // randomize Scale
        float newScale = Random.Range(minScale * 100, maxScale * 100) / 100;
        retVal.x = newScale;
        retVal.y = newScale;

        // set mass
        mass = baseMass * newScale * massMultiplier;

        orgScale = retVal;
        return retVal;
    }
}
