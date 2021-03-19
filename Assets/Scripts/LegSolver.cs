using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;
using System;

public class LegSolver : MonoBehaviour
{
    Vector3 oldPosition;

    [SerializeField] float maxStepDistance;
    [SerializeField] AnimationCurve stepHeight;
    [SerializeField] float timeStepTakes;
    Vector3 stepHeightMagnitude;

    public Leg[] legs;

    void Start()
    {
        legs = FindObjectsOfType<Leg>();
        foreach (Leg leg in legs)
        {
            
        }
        
    }

    void Update()
    {

        foreach (Leg leg in legs)
        {
            LegPosition legPos = leg.GetLegPosition();
                switch (legPos)
                {
                case LegPosition.FrontLeft:
                    RaycastHit hitInfoFrontLeft;
                    Physics.Raycast(leg.GetPole().position, Vector3.down, out hitInfoFrontLeft, 5f);
                    float distanceFrontLeft = Vector3.Distance(leg.GetOldPosition(), hitInfoFrontLeft.point);
                    if (legs[3].GetIsStepping() || legs[0].GetIsStepping()) { return; }
                    else
                    {
                        if (distanceFrontLeft > maxStepDistance)
                        {
                            Vector3 newPosition = hitInfoFrontLeft.point;
                            StartCoroutine(Step(leg.GetOldPosition(), newPosition, timeStepTakes, leg));
                            leg.SetOldPosition(hitInfoFrontLeft.point);
                        }
                    }
                    break;
                case LegPosition.BackRight:

                    RaycastHit hitInfoBackRight;
                    Physics.Raycast(leg.GetPole().position, Vector3.down, out hitInfoBackRight, 5f);
                    float distanceBackRight = Vector3.Distance(leg.GetOldPosition(), hitInfoBackRight.point);
                    if (legs[3].GetIsStepping() || legs[0].GetIsStepping()) { return; }
                    else
                    {
                        if (distanceBackRight > maxStepDistance)
                        {
                            Vector3 newPosition = hitInfoBackRight.point;
                            StartCoroutine(Step(leg.GetOldPosition(), newPosition, timeStepTakes, leg));
                            leg.SetOldPosition(hitInfoBackRight.point);
                        }
                    }
                    break;


                case LegPosition.BackLeft:
                    RaycastHit hitInfoBackLeft;
                    Physics.Raycast(leg.GetPole().position, Vector3.down, out hitInfoBackLeft, 5f);
                    float distanceBackLeft = Vector3.Distance(leg.GetOldPosition(), hitInfoBackLeft.point);
                    if (legs[1].GetIsStepping() || legs[2].GetIsStepping()) { return; }
					else
					{
                        if (distanceBackLeft > maxStepDistance)
                        {
                            Vector3 newPosition = hitInfoBackLeft.point;
                            StartCoroutine(Step(leg.GetOldPosition(), newPosition, timeStepTakes, leg));
                            leg.SetOldPosition(hitInfoBackLeft.point);
                        }
                    }
                    break;

                case LegPosition.FrontRight:
                    RaycastHit hitInfoFrontRight;
                    Physics.Raycast(leg.GetPole().transform.position, Vector3.down, out hitInfoFrontRight, 5f);
                    float distanceFrontRight = Vector3.Distance(leg.GetOldPosition(), hitInfoFrontRight.point);
                    if (legs[1].GetIsStepping() || legs[2].GetIsStepping()) { return; }
					else
					{
                        if (distanceFrontRight > maxStepDistance)
                        {
                            Vector3 newPosition = hitInfoFrontRight.point;
                            StartCoroutine(Step(leg.GetOldPosition(), newPosition, timeStepTakes, leg));
                            leg.SetOldPosition(hitInfoFrontRight.point);
                        }
                    }
                    break;

            }
        }
    }


	IEnumerator Step(Vector3 source, Vector3 target, float overTime, Leg leg)
    {
        float startTime = Time.time;

        while (Time.time < startTime + overTime)
        {
			leg.SetIsStepping(true);

            float addY = stepHeight.Evaluate((Time.time - startTime) / overTime);
            stepHeightMagnitude = new Vector3(0, addY, 0);

            Vector3 updatedPos = Vector3.Lerp(source, target, (Time.time - startTime) / overTime);
            leg.SetTargetPosition(updatedPos + stepHeightMagnitude);

            yield return null;
        }
        leg.SetTargetPosition(target);
		leg.SetIsStepping(false);

    }
}
