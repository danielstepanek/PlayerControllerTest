using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegSolver : MonoBehaviour
{
    [SerializeField] GameObject[] legs;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        foreach(GameObject leg in legs)
        {
            Gizmos.DrawSphere(leg.transform.position, .1f);
        }
    }
}
