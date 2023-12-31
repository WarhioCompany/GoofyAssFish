using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using UnityEngine;

public class SHITSpikeScipt : MonoBehaviour
{
    public Vector3 spikeStartPos;
    public Vector3 spikeTipOffset;
    
    public float rotationSpeed;
    public float shootingSpeed;

    public float hitWait;
    private float _hitWait;

    public float retrieveSpeed;
    public float retrieveRotationSpeed;

    private float _currentSpikeSpeed;
    private float _currentSpikeRotationSpeed;

    public float shootingRange;

    public float initialRotation;

    public SHITSpikeManager manager;

    public Sprite sprite;

    //public GameObject follow;

    public Collider hitCollider;

    Vector3 target;
    public Transform initialTipPosition;

    float targetLocalY;

    GameObject marker;

    public enum SpikeState
    {
        None, 
        Preparing,
        Shoot,
        Hit,
        Attached,
        Retrieving
    }
    public SpikeState state;

    private void OnDrawGizmos()
    {
        //Gizmos.color = Color.yellow;
        //Gizmos.DrawLine(transform.position, target);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, manager.getMousePos());

        //Gizmos.color = Color.yellow;
        //Gizmos.DrawWireSphere(follow.transform.position, 0.5f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(initialTipPosition.position, 0.3f);
    }

    // Start is called before the first frame update
    void Start()
    {
        //follow = Instantiate(follow);

        //marker = Instantiate(new GameObject());
        //marker.AddComponent<SpriteRenderer>().sprite = sprite;
        //marker.GetComponent<SpriteRenderer>().sortingOrder = 2;


        initialTipPosition.position = GetTipPosition();
        initialRotation = transform.localRotation.eulerAngles.z;
        transform.GetChild(0).transform.localPosition = spikeStartPos;
        targetLocalY = spikeStartPos.y;
        _currentSpikeSpeed = shootingSpeed;
        _currentSpikeRotationSpeed = rotationSpeed;
        _hitWait = hitWait;
    }

    public Vector3 GetTipPosition()
    {
        return transform.TransformPoint(spikeTipOffset + transform.GetChild(0).localPosition);
        //return spikeTipOffset + transform.GetChild(0).localPosition;
    }
    Vector3 TargetAngleFix(Vector3 target)
    {
        return Vector3.Normalize(target - transform.position) * shootingRange * 5;
    }   
    public void Shoot(Vector3 mousePos, Transform attachedObject)
    {
        if (attachedObject != null)
        {
            //TODO: apply reverse force to the object
        }

        state = SpikeState.Shoot;
        target = TargetAngleFix(mousePos);
        targetLocalY = spikeStartPos.y + shootingRange;
        manager.state = SHITSpikeManager.SpikeManagerState.Flying;
    }
    void Retrieve()
    {
        _currentSpikeSpeed = retrieveSpeed;
        _currentSpikeRotationSpeed = retrieveRotationSpeed;
        target = initialTipPosition.position;
        targetLocalY = spikeStartPos.y;
        state = SpikeState.Retrieving;
    }
    Vector2 getContactPosition (Collider collision)
    {
        //marker.transform.position = collision.ClosestPoint(transform.position);
        return collision.gameObject.GetComponent<Collider>().ClosestPoint(transform.position);
    }
    float CalculateLocalYByContactPoint(Vector2 contactPoint)
    {
        return Vector2.Distance(transform.position, contactPoint) + spikeStartPos.y - 2;
    }
    public void Hit(Collider hit)
    {
        if (state != SpikeState.Shoot) return;

        Debug.Log("Spike Hit: " + hit.gameObject.name + " | lastObj = " + manager.lastObj);

        if (hit.gameObject == manager.lastObj)
        {
            manager.Push(hit, this);
            return;
        }

        hitCollider = hit;
        state = SpikeState.Hit;
        manager.attachedSpike = this;
        manager.lastObj = hit.gameObject;
    }
    public void Detach()
    {
        Retrieve();
    }
    // Update is called once per frame
    void Update()
    {
        if (manager.state == SHITSpikeManager.SpikeManagerState.Prepare && manager.closestSpike == this)
        {
            //Debug.Log("O");
            target = TargetAngleFix(manager.getMousePos()); 
        }
        else if (state == SpikeState.Shoot && manager.closestSpike == this)
        {

            if (Vector3.Distance(transform.position, GetTipPosition()) > shootingRange)
            {
                //Miss
                Retrieve();
            }
        }
        else if (state == SpikeState.Hit && manager.attachedSpike == this)
        {
            if(hitCollider != null)
            {
                Vector2 contactPosition = getContactPosition(hitCollider);
                targetLocalY = CalculateLocalYByContactPoint(contactPosition);

                //Debug.Log("X1");
                target = TargetAngleFix(contactPosition);
                if (_hitWait <= 0)
                {
                    Vector2 dif = Vector3.Normalize(transform.parent.position - (Vector3)contactPosition) * manager.attachRadius;
                    transform.parent.position = Vector3.Lerp(transform.parent.position, contactPosition + dif, manager.playerPullSpeed * Time.deltaTime);
                    if (Vector3.Distance(transform.parent.position, contactPosition + dif) < .3)
                    {
                        state = SpikeState.Attached;
                        transform.parent.SetParent(hitCollider.transform);
                        manager.state = SHITSpikeManager.SpikeManagerState.None;
                    }
                }
                else
                {
                    _hitWait -= Time.deltaTime;
                }

            }

        }
        else if (state == SpikeState.Attached)
        {
            if (hitCollider == null)
            {
                Retrieve();
            }
            Vector2 contactPosition = getContactPosition(hitCollider);
            targetLocalY = CalculateLocalYByContactPoint(contactPosition);

            //Debug.Log("X2");
            target = TargetAngleFix(contactPosition);
        }
        else 
        {
            //Debug.Log("X3");
            target = initialTipPosition.position;
        }

        float rot = 0;
        if (target == initialTipPosition.position)
        {
            if(state == SpikeState.Attached) 
                print("setting init");
            rot = initialRotation;
        }
        else
        {
            Vector2 diff = target; // - GetTipPosition();
            rot = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg - 90;
        }
        if(Mathf.Abs(transform.GetChild(0).localPosition.y - targetLocalY) < 0.1 && state == SpikeState.Retrieving)
        {
            //Retrieved
            state = SpikeState.None;
            manager.state = SHITSpikeManager.SpikeManagerState.None;
            _currentSpikeSpeed = shootingSpeed;
            _currentSpikeRotationSpeed = rotationSpeed;
        }

        //follow.transform.position = target;

        transform.rotation =  Quaternion.Euler(0, 0, Mathf.LerpAngle(transform.rotation.eulerAngles.z, rot, Time.deltaTime * _currentSpikeRotationSpeed));
        transform.GetChild(0).localPosition = new Vector3(0, /*Mathf.Clamp(*/Mathf.Lerp(transform.GetChild(0).localPosition.y, targetLocalY, _currentSpikeSpeed * Time.deltaTime)/*, spikeStartPos.y, shootingRange)*/);
    }
}
