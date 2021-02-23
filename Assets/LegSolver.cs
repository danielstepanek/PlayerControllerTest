using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegSolver : MonoBehaviour
{
    [SerializeField] Transform RFTarget;
    [SerializeField] Transform RFPole;
    [SerializeField] Transform RBTarget;
    [SerializeField] Transform RBPole;

    [SerializeField] AnimationCurve stepHeight;
    Vector3 stepHeightMagnitude;
    RaycastHit hitInfo;
    Vector3 oldPosition;
    [SerializeField] float maxStepDistance;
    void Start()
    {
        Physics.Raycast(RFPole.position, Vector3.down, out hitInfo, 5f);
		oldPosition = hitInfo.point;

    }

    void Update()
    {
        Physics.Raycast(RFPole.position, Vector3.down, out hitInfo, 5f);
        float distance = Vector3.Distance(oldPosition, hitInfo.point);

        if (distance > maxStepDistance)
		{
            Vector3 newPosition = hitInfo.point;
            StartCoroutine(Step(oldPosition, newPosition, .1f));
            oldPosition = hitInfo.point;
        }

    }

    IEnumerator Step(Vector3 source, Vector3 target, float overTime)
    {
        float startTime = Time.time;
        while (Time.time < startTime + overTime)
        {
            float addY = stepHeight.Evaluate((Time.time - startTime) / overTime);
			stepHeightMagnitude = new Vector3(0, addY, 0);


            RFTarget.position = Vector3.Lerp(source, target, (Time.time - startTime) / overTime);
            RFTarget.position = RFTarget.position + stepHeightMagnitude;
            yield return null;
        }
        RFTarget.position = target;
    }
}
