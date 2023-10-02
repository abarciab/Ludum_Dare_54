using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RendererCoordinator : MonoBehaviour
{
    List<Renderer> renderers = new List<Renderer>();
    Transform cam;
    [SerializeField] float raycastFrequency = 0.5f;
    float raycastCooldown;
    [SerializeField] List<Transform> raycastOrigins = new List<Transform>();
    [SerializeField] bool raycast;
    float originalYScale;


    [SerializeField] float targetScale;

    public List<Renderer> GetRenderers()
    {
        if (renderers.Count == 0) renderers = gameObject.GetComponentsInChildren<Renderer>().ToList();
        return renderers;
    }

    private void Start()
    {
        cam = Camera.main.transform;
        originalYScale = transform.localScale.y;
    }

    private void Update()
    {
        raycastCooldown -= Time.deltaTime;
        if (raycastCooldown <= 0) CheckVisible();

        //if (!renderers[0].gameObject.activeInHierarchy) return;
        
        setScale();
    }

    void setScale()
    {
        float dist = GetDist();
        float maxDist = GameManager.i.detailRenderDist;
        float fadeLength = GameManager.i.detailRenderDist;
        float progress = 1 - Mathf.Min(dist, maxDist) / maxDist;
        
        targetScale = Mathf.Lerp(targetScale, progress, 0.1f);

        transform.localScale = new Vector3(transform.localScale.x, Mathf.Lerp(0, originalYScale, targetScale), transform.localScale.z);
    }

    void CheckVisible()
    {
        raycastCooldown = raycastFrequency;
        bool show = false;
        
        if (!InRange()) {
            foreach (var r in renderers) r.gameObject.SetActive(false);
            return;
        }
        if (!raycast) {
            foreach (var r in renderers) r.gameObject.SetActive(true);
            return;
        }
        
        foreach (var t in raycastOrigins) {
            if (DoRaycast(t.position)) {
                show = true;
                break;
            }
        }
        foreach (var r in renderers) r.gameObject.SetActive(show);
    }

    void SetMaterialTransparency()
    {
        float dist = GetDist();
        float renderDist = GameManager.i.detailRenderDist;
        float fadeDist = GameManager.i.detailFadeDist;
        if (dist > renderDist) return;

        if (dist < renderDist - fadeDist) targetScale = 1;
        else targetScale = 1 - (dist - (renderDist - fadeDist)) / Mathf.Abs(renderDist - fadeDist);
    }

    bool InRange()
    {
        float dist = GetDist();
        float maxDist = GameManager.i.detailRenderDist;
        return dist < maxDist;
    }

    float GetDist()
    {
        return Vector3.Distance(transform.position, cam.position);

        float topDownDist = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(cam.position.x, cam.position.z));
        float vertDist = Mathf.Abs(cam.position.y - transform.position.y);
        return Mathf.Max(topDownDist, vertDist);
    }

    bool DoRaycast(Vector3 origin)
    {
        var dir = cam.position - transform.position;
        var didHit = Physics.Raycast(origin, dir, out var hit, GameManager.i.detailRenderDist);

        return didHit && hit.transform == cam;
    }

}
