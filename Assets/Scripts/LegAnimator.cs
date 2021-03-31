using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class LegAnimator : MonoBehaviour
{
    public Transform[] legTargets;
    public Transform[] legTargetsFlying;
    public float stepSize = 1f;
    public float smoothness = 1f;
    public float stepHeight = 0.1f;

    private float raycastRange = .2f;
    private Vector3[] defaultLegPositions;
    private Vector3[] lastLegPositions;
    private Vector3 lastBodyUp;
    private bool[] legMoving;
    private int nbLegs;

    private Vector3 velocity;
    private Vector3 lastVelocity;
    private Vector3 lastBodyPos;

    public float velocityOffset = 15f;

    public LayerMask IgnoreMe;

    [SerializeField] SphereController sphereController;
    [SerializeField] Animator animator;
    [SerializeField] Collider extendedCollider;
    [SerializeField] Rig rigWalking;
    [SerializeField] Rig rigFlying;

    [SerializeField] float xScale = .5f;
    [SerializeField] float yScale = .5f;

    bool isSlowed = true;

    static Vector3[] MatchToSurfaceFromAbove(Vector3 point, float halfRange, Vector3 up, LayerMask ignoreLayer)
    {
        Vector3[] res = new Vector3[2];
        RaycastHit hit;
        Ray ray = new Ray(point + halfRange * up, -up);


        if (Physics.Raycast(ray, out hit, 2f * halfRange, ~ignoreLayer))
        {
            res[0] = hit.point;
            res[1] = hit.normal;
        }
        else
        {
            res[0] = point;
        }
        return res;
    }

    void Start()
    {
        lastBodyUp = transform.up;

        nbLegs = legTargets.Length;
        defaultLegPositions = new Vector3[nbLegs];
        lastLegPositions = new Vector3[nbLegs];
        legMoving = new bool[nbLegs];
        for (int i = 0; i < nbLegs; ++i)
        {
            defaultLegPositions[i] = legTargets[i].localPosition;
            lastLegPositions[i] = legTargets[i].position;
            legMoving[i] = false;
        }
        lastBodyPos = transform.position;
    }

    IEnumerator PerformStep(int index, Vector3 targetPoint)
    {
        Vector3 startPos = lastLegPositions[index];
        for (int i = 1; i <= smoothness; ++i)
        {
            legTargets[index].position = Vector3.Lerp(startPos, targetPoint, i / (float)(smoothness + 1f));
            legTargets[index].position += transform.up * Mathf.Sin(i / (float)(smoothness + 1f) * Mathf.PI) * stepHeight;
            yield return new WaitForFixedUpdate();
        }
        legTargets[index].position = targetPoint;
        lastLegPositions[index] = legTargets[index].position;
        legMoving[0] = false;
    }

	private void Update()
	{
        bool isFlying = sphereController.GetFlying();
        if (isFlying)
        {
            animator.SetBool("isFlying", true);
            return;
        }
        animator.SetBool("isFlying", false);
    }

	void FixedUpdate()
    {
        
        bool isFlying = sphereController.GetFlying();
		if (isFlying)
		{
            smoothness = 1f;
            velocityOffset = 1f;
            rigWalking.weight = Mathf.MoveTowards(rigWalking.weight, 0f, .1f);
            rigFlying.weight = Mathf.MoveTowards(rigFlying.weight, 1f, .1f);

            for (int i = 0; i < nbLegs; i++)
            {
                float translate;
                Vector3 pos = legTargetsFlying[i].localPosition;
                if (pos.x > 0)
				{
                    translate = .2f * Mathf.PerlinNoise(Time.time * xScale + i * .5f, .5f) + .3f;
                }
				else
				{
                    translate = -.2f * Mathf.PerlinNoise(Time.time * xScale + i * .5f, .5f) - .3f;
                }
                float height = .2f * Mathf.PerlinNoise(Time.time * yScale + i, 0.0f);
                
                pos.y = height - .4f;
                pos.x = translate;
                
                legTargetsFlying[i].localPosition = pos;
            }


        }
        else if (!isFlying && smoothness < 5f)
        {
            smoothness = Mathf.MoveTowards(smoothness, 5f, .05f);
            velocityOffset = Mathf.MoveTowards(velocityOffset, 15f, .2f);

            rigWalking.weight = Mathf.MoveTowards(rigWalking.weight, 1f, .1f);
            rigFlying.weight = Mathf.MoveTowards(rigFlying.weight, 0f, .1f);
        }


        





        velocity = transform.position - lastBodyPos;
        velocity = (velocity + smoothness * lastVelocity) / (smoothness + 1f);

        if (velocity.magnitude < 0.000025f)
            velocity = lastVelocity;
        else
            lastVelocity = velocity;


        Vector3[] desiredPositions = new Vector3[nbLegs];
        int indexToMove = -1;
        float maxDistance = stepSize;
        for (int i = 0; i < nbLegs; ++i)
        {
            desiredPositions[i] = transform.TransformPoint(defaultLegPositions[i]);

            float distance = Vector3.ProjectOnPlane(desiredPositions[i] + velocity * velocityOffset - lastLegPositions[i], transform.up).magnitude;
            if (distance > maxDistance)
            {
                maxDistance = distance;
                indexToMove = i;
            }
        }
        for (int i = 0; i < nbLegs; ++i)
            if (i != indexToMove)
                legTargets[i].position = lastLegPositions[i];
        if (indexToMove != -1 && !legMoving[indexToMove])
        {
            Vector3 targetPoint = desiredPositions[indexToMove] + Mathf.Clamp(velocity.magnitude * velocityOffset, 0.0f, 1.5f) * (desiredPositions[indexToMove] - legTargets[indexToMove].position) + velocity * velocityOffset;
            Vector3[] positionAndNormal = MatchToSurfaceFromAbove(targetPoint, raycastRange, transform.up, IgnoreMe);
            legMoving[0] = true;
            StartCoroutine(PerformStep(indexToMove, positionAndNormal[0]));
        }

        lastBodyPos = transform.position;
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < nbLegs; ++i)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(legTargets[i].position, 0.05f);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.TransformPoint(defaultLegPositions[i]), stepSize);
        }
    }
}
