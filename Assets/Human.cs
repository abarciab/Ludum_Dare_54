using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Human : MonoBehaviour
{
    [SerializeField] List<Transform> patrolPoints = new List<Transform>();
    [SerializeField] Vector2 waitTimeRange;
    float waitCooldown;
    [SerializeField] NavMeshAgent agent;

    [Header("Bouncing")]
    [SerializeField] float bounceFrequency;
    [SerializeField] float bounceMagnitude, rotFrequency, rotMagnitude, rotOffset;
    [SerializeField] Transform model;
    Vector3 originalPos, waypointTarget;

    [Header("FireSense")]
    [SerializeField] float fireSenseRadius;
    [SerializeField] float targetdistanceAway = 3;
    bool seesFire;

    [Header("Fire behavior")]
    [SerializeField] bool runFromFire;
    [SerializeField] bool runToFire, putOutFires;
    [SerializeField] float putOutFireDist = 2, waterRadius = 4;

    [Header("Water")]
    [SerializeField] float waterResetTime;
    float waterCooldown;

    [Header("Home")]
    [SerializeField] Transform home;
    [SerializeField] float maxDistanceFromHome;
    bool goingHome;

    EnvironmentManager eMan;

    private void Start()
    {
        originalPos = model.localPosition;
        eMan = EnvironmentManager.i;
    }

    private void Update()
    {
        waterCooldown -= Time.deltaTime;

        if (home != null) {
            float homeDist = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(home.position.x, home.position.z));
            if (homeDist > maxDistanceFromHome) {
                GoHome();
                return;
            }
        }

        if (goingHome && agent.remainingDistance > 2) return;
        goingHome = false;

        float fireDist = eMan.GetClosestFireDist(transform.position);
        seesFire = fireDist < fireSenseRadius;
        if (seesFire) {
            if (runFromFire) RunFromFire(false);
            else if (runToFire) RunFromFire(true);
            if (putOutFires && fireDist < putOutFireDist) DouseNearestFires(); 
        } 
        else {
            GoHome();
        }

        if (agent.remainingDistance < 0.1f) {
            waitCooldown -= Time.deltaTime;
            StopBounce();
        }
        else {
            if (!seesFire) GotoWaypoint();
            Bounce();
        }
        if (!seesFire && waitCooldown <= 0) GotoNewPoint();
    }

    void GoHome()
    {
        agent.destination = home.position;
        waypointTarget = home.position;
        goingHome = true;
    }

    void DouseNearestFires()
    {
        if (waterCooldown > 0) return;
        waterCooldown = waterResetTime;

        var firePos = eMan.GetClosestFirePos(transform.position);
        var fires = eMan.GetFiresWithinRange(firePos, waterRadius);

        for (int i = 0; i < fires.Count; i++) {
            fires[i].GetComponent<Fire>().AttemptDouse(1);
        }
    }

    void GotoWaypoint()
    {
        agent.destination = waypointTarget;
    }

    void RunFromFire(bool inverse)
    {
        //print("RUNNING FROM FIRE");
        var firePos = eMan.GetClosestFirePos(transform.position);
        var dir = transform.position - firePos;
        if (inverse) dir *= -1;
        agent.destination = transform.position + dir * targetdistanceAway;
        Debug.DrawLine(transform.position, agent.destination);
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

    void GotoNewPoint()
    {
        if (patrolPoints.Count == 0 || patrolPoints[0] == null) return;

        waitCooldown = Random.Range(waitTimeRange.x, waitTimeRange.y);
        var pos = patrolPoints[Random.Range(0, patrolPoints.Count)].position;
        pos.y = transform.position.y;
        agent.destination = pos;
        waypointTarget = agent.destination;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, fireSenseRadius);
        if (putOutFires) {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, putOutFireDist);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, waterRadius);
        }
        if (home != null) {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(home.position, maxDistanceFromHome);
        }
    }
}
