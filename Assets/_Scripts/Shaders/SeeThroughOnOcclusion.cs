using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class SeeThroughOnOcclusion : MonoBehaviour
{
    public Material overlayMaterial;
    public Camera targetCamera;
    public LayerMask occlusionMask = ~0;
    public bool supportSkinnedMesh = true;

    private List<Renderer> sourceRenderers = new List<Renderer>();
    private List<GameObject> overlayObjects = new List<GameObject>();

    void Awake()
    {
        if (targetCamera == null) targetCamera = Camera.main;
        if (overlayMaterial == null)
        {
            Debug.LogError("Overlay material missing");
            enabled = false;
            return;
        }

        var renderers = GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
        {
            sourceRenderers.Add(r);

            GameObject go = new GameObject(r.name + "_Overlay");
            go.transform.SetParent(r.transform, false);

            var mf = go.AddComponent<MeshFilter>();
            var mr = go.AddComponent<MeshRenderer>();
            mr.material = overlayMaterial;
            mr.enabled = false;

            var srcMF = r.GetComponent<MeshFilter>();
            if (srcMF != null) mf.sharedMesh = srcMF.sharedMesh;

            overlayObjects.Add(go);
        }
    }

    void Update()
    {
        if (targetCamera == null) return;

        for (int i = 0; i < sourceRenderers.Count; i++)
        {
            var r = sourceRenderers[i];
            if (r == null) continue;

            Vector3 dir = r.bounds.center - targetCamera.transform.position;
            float dist = dir.magnitude;

            bool blocked = Physics.Raycast(
                targetCamera.transform.position,
                dir.normalized,
                dist,
                occlusionMask
            );

            overlayObjects[i].GetComponent<MeshRenderer>().enabled = blocked;
        }
    }
}
