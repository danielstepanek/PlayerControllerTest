using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;

public class Leg : MonoBehaviour
{
    // Start is called before the first frame update
    CCDIK ccdik;

    private void Awake()
    {
        ccdik = GetComponent<CCDIK>();

        IKSolverHeuristic.Bone[] bones = ccdik.solver.bones;
        print(bones.Length);
        foreach(IKSolverHeuristic.Bone bone in bones)
        {
            print(bone.transform.position);
        }


    }
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }
}
