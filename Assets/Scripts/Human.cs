using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Apple;

public class Human : MonoBehaviour
{
    [Header("safe behavior")]
    [SerializeField] bool patrol;
    [SerializeField, ConditionalHide(nameof(patrol))] List<Transform> patrolPoints = new List<Transform>();
    [SerializeField, ConditionalHide(nameof(patrol))] Vector2 waitTimeRange;
    float waitCooldown;

    [SerializeField] Transform home;

    [Header("fire behavior")]
    [SerializeField] float runFromFireDist;
    [SerializeField] bool freakOut, putOutFires;
    [SerializeField, ConditionalHide(nameof(putOutFires))] float putOutFireDist = 2, waterRadius = 4, useWaterResetTime;
    float useWaterCooldown;


    [Header("FireSense")]
    [SerializeField] float fireSenseRadius;
    [SerializeField] bool fireSenseFromSelf = true;
    [SerializeField, ConditionalHide(nameof(fireSenseFromSelf), inverse:true)] Transform fireSenseSource;
    bool seesFire;
    
    /*
    [Header("Water")]
    [SerializeField] float waterResetTime;
    float waterCooldown;*/

    [Header("Movement")]
    [SerializeField] float bounceFrequency;
    [SerializeField] float bounceMagnitude, rotFrequency, rotMagnitude, rotOffset, destinationThreshold = 1.5f;
    [SerializeField] Transform model;
    Vector3 originalPos;

    [Header("Misc")]
    [SerializeField] NavMeshAgent agent;

    EnvironmentManager eMan;

    private void Start()
    {
        originalPos = model.localPosition;
        eMan = EnvironmentManager.i;
        agent.destination = transform.position;

        if (fireSenseFromSelf) fireSenseSource = transform;
    }

    private void Update()
    {
        float fireDist = eMan.GetClosestFireDist(fireSenseSource.position);
        if (fireDist < fireSenseRadius || fireDist < runFromFireDist) DoFireBehavior();
        else DoSafeBehavior();

        bool freakingOut = freakOut && fireDist != Mathf.Infinity;
        if (freakingOut || agent.remainingDistance > destinationThreshold) Bounce();
        else StopBounce();

        if (freakingOut && agent.remainingDistance < destinationThreshold) LookAtFire();
    }

    void LookAtFire()
    {
        var firePos = eMan.GetClosestFirePos(transform.position);
        transform.LookAt(firePos);
        var rot = transform.localEulerAngles;
        rot.x = rot.z = 0;
        transform.localEulerAngles = rot;
    }

    void DoFireBehavior()
    {
        float fireDist = eMan.GetClosestFireDist(transform.position);
        if (fireDist < runFromFireDist) RunFromFire();
        else if (putOutFires) TryToPutOutFire(fireDist);
    }

    void TryToPutOutFire(float fireDist)
    {
        if (fireDist > putOutFireDist) GoToFire(fireDist);
        else UseWaterOnFire();
    }

    void GoToFire(float fireDist)
    {
        agent.destination = eMan.GetClosestFirePos(transform.position);
    }

    void UseWaterOnFire()
    {
        agent.destination = transform.position;

        useWaterCooldown -= Time.deltaTime;
        if (useWaterCooldown > 0) return;
        useWaterCooldown = useWaterResetTime;

        var waterTargetPos = eMan.GetClosestFirePos(transform.position);
        Debug.DrawLine(transform.position, waterTargetPos, Color.blue);
        var affectedFires = eMan.GetFiresWithinRange(waterTargetPos, waterRadius);

        for (int i = 0; i < affectedFires.Count; i++) {
            affectedFires[i].GetComponent<Fire>().AttemptDouse(1);
        }
    }

    void DoSafeBehavior()
    {
        if (patrol) DoPatrol();
        else if (home != null) GoHome();
    }

    void DoPatrol()
    {
        if (agent.remainingDistance <= destinationThreshold) waitCooldown -= Time.deltaTime;
        if (waitCooldown > 0) return;

        GotoNewPatrolPoint();
    }

    void GoHome()
    {
        agent.destination = home.position;
    }

    void RunFromFire ()
    {
        var firePos = eMan.GetClosestFirePos(transform.position);
        var dir = transform.position - firePos;
        agent.destination = transform.position + dir;
    }

    void Bounce()
    {
        model.localPosition = originalPos + transform.up * Mathf.Sin(Time.time * bounceFrequency) * bounceMagnitude * Time.deltaTime;
        float xRot = Mathf.Sin(Time.time * rotFrequency) * rotMagnitude * Time.deltaTime;
        model.localEulerAngles.Set(90 + rotOffset + xRot, 0, 0);
    }

    void StopBounce()
    {
        model.localPosition = Vector3.Lerp(model.localPosition, originalPos, 0.1f);
        model.localEulerAngles.Set(90, 0, 0);
    }

    void GotoNewPatrolPoint()
    {
        if (patrolPoints.Count == 0 || patrolPoints[0] == null) return;

        waitCooldown = Random.Range(waitTimeRange.x, waitTimeRange.y);
        var pos = patrolPoints[Random.Range(0, patrolPoints.Count)].position;
        pos.y = transform.position.y;
        agent.destination = pos;
    }

    private void OnDrawGizmosSelected()
    {
        if (fireSenseFromSelf) Gizmos.DrawWireSphere(transform.position, fireSenseRadius);
        else if (fireSenseSource != null) Gizmos.DrawWireSphere(fireSenseSource.position, fireSenseRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, runFromFireDist);

        if (putOutFires) {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, putOutFireDist);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position + transform.forward * putOutFireDist, waterRadius);
        }

        if (patrol) {
            Gizmos.color = Color.green;
            foreach (var point in patrolPoints) if (point) Gizmos.DrawWireSphere(point.position, 1f);
        }
    }
}
