using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;

public class Leg : MonoBehaviour
{
    // Start is called before the first frame update
    CCDIK ccdik;
    [SerializeField] LegPosition legPositon;
    [SerializeField] Transform pole;
    [SerializeField] Transform target;

    bool isStepping;
    Vector3 oldPosition;
    private void Awake()
    {
        ccdik = GetComponent<CCDIK>();
        IKSolverHeuristic.Bone[] bones = ccdik.solver.bones;
        foreach(IKSolverHeuristic.Bone bone in bones)
        {

        }
        oldPosition = gameObject.transform.position;
    }
    void Start()
    {
        isStepping = false;
    }
    public LegPosition GetLegPosition()
	{
        return legPositon;
	}
    public Transform GetPole()
    {
        return pole;
    }
    public Vector3 GetOldPosition()
    {
        return oldPosition;
    }
    public void SetOldPosition(Vector3 oldPos)
    {
        oldPosition = oldPos;
    }
    public bool GetIsStepping()
    {
        return isStepping;
    }
    public void SetIsStepping(bool isStep)
    {
        isStepping = isStep;
    }
    public Transform GetTarget()
    {
        return target;
    }
    public void SetTargetPosition(Vector3 setTarget)
    {
        target.position = setTarget;
    }
	private void OnDrawGizmos()
	{
		Gizmos.DrawWireSphere(pole.position, .2f);

	}
}
