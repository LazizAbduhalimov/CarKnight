using System.Collections.Generic;
using System.ComponentModel;
using Unity.Collections;
using UnityEngine;


public class DistanceChecker : MonoBehaviour
{
    [SerializeField] private List<float> distances = new List<float>();
    [SerializeField] private List<Color> colors = new List<Color>();

    private void OnDrawGizmosSelected()
    {
        for (int i = 0; i < distances.Count; i++)
        {
            Gizmos.color = colors[i];
            Gizmos.DrawWireSphere(transform.position, distances[i]);
        }
    }
}
