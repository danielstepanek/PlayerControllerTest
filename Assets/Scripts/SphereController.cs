﻿using UnityEngine;

public class SphereController : MonoBehaviour
{

	[SerializeField]
	Transform playerInputSpace = default;

	[SerializeField, Range(0f, 100f)]
	float maxSpeed = 10f, maxAirSpeed = 10f;

	[SerializeField, Range(0f, 5f)]
	float flightSmoothing;

	[SerializeField, Range(0f, 100f)]
	float rotateSpeed = 10f, rotateSpeedAir = 10f;

	[SerializeField, Range(0f, 100f)]
	float maxAcceleration = 10f, maxAirAcceleration = 1f;

	[SerializeField, Range(0f, 10f)]
	float jumpHeight = 2f;

	[SerializeField, Range(0, 5)]
	int maxAirJumps = 0;

	[SerializeField, Range(0, 180)]
	float maxGroundAngle = 25f, maxStairsAngle = 50f;

	[SerializeField, Range(0f, 100f)]
	float maxSnapSpeed = 100f;

	[SerializeField, Min(0f)]
	float probeDistance = .2f;

	[SerializeField]
	LayerMask probeMask = -1, stairsMask = -1;


	Rigidbody body, connectedBody, previousConnectedBody;

	Vector3 velocity, desiredVelocity, connectionVelocity;

	Vector3 connectionWorldPosition, connectionLocalPosition;

	Vector3 upAxis, rightAxis, forwardAxis;

	bool desiredFlying;

	Vector3 contactNormal, steepNormal;

	int groundContactCount, steepContactCount;

	bool OnGround => groundContactCount > 0;

	bool OnSteep => steepContactCount > 0;

	int jumpPhase;

	float minGroundDotProduct, minStairsDotProduct;

	int stepsSinceLastGrounded, stepsSinceLastTakeoff;

	Vector3 playerInputUp;

	Quaternion lastRotation;
	[SerializeField] bool flying;

	LegAnimator legAnimator;

	void OnValidate()
	{
		minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
		minStairsDotProduct = Mathf.Cos(maxStairsAngle * Mathf.Deg2Rad);
	}

	void Awake()
	{
		body = GetComponent<Rigidbody>();
		legAnimator = FindObjectOfType<LegAnimator>();
		body.useGravity = false;
		OnValidate();
	}

	void Update()
	{
		Vector3 playerInput;
		playerInput.x = Input.GetAxis("Horizontal");
		playerInput.z = Input.GetAxis("Vertical");
		playerInput.y = Input.GetAxis("Fly");
		playerInput = Vector3.ClampMagnitude(playerInput, 1f);

		if (playerInputSpace)
		{
			rightAxis = ProjectDirectionOnPlane(playerInputSpace.right, upAxis);
			forwardAxis =
				ProjectDirectionOnPlane(playerInputSpace.forward, upAxis);
			
		
		}
		else
		{
			rightAxis = ProjectDirectionOnPlane(Vector3.right, upAxis);
			forwardAxis = ProjectDirectionOnPlane(Vector3.forward, upAxis);
		}
		float speed = flying ? maxAirSpeed : maxSpeed;
		desiredVelocity =
			new Vector3(playerInput.x, playerInput.y, playerInput.z) * speed;

		desiredFlying |= Input.GetButtonDown("Jump");
	}

	void FixedUpdate()
	{
		Vector3 gravity = CustomGravity.GetGravity(body.position, out upAxis);
		UpdateState();
		AdjustVelocity();
		AdjustRotation();

		if (desiredFlying)
		{
			desiredFlying = false;
			BeginFlying(gravity);
		}
		else if (flying)
		{
			velocity = Vector3.MoveTowards(velocity, Vector3.zero, flightSmoothing * Time.deltaTime);
		}
		else
		{
			velocity -= contactNormal * (maxAcceleration * Time.deltaTime);
		}

		
		body.velocity = velocity;
		ClearState();
	}

	void ClearState()
	{
		groundContactCount = steepContactCount = 0;
		contactNormal = steepNormal = connectionVelocity = Vector3.zero;
		previousConnectedBody = connectedBody;
		connectedBody = null;
	}

	void UpdateState()
	{
		stepsSinceLastGrounded += 1;
		stepsSinceLastTakeoff += 1;
		velocity = body.velocity;
		if (OnGround || SnapToGround() || CheckSteepContacts())
		{
			stepsSinceLastGrounded = 0;
			
			if (groundContactCount > 1)
			{
				contactNormal.Normalize();
				
			}
		}
		else
		{
			contactNormal = upAxis;
		}

		if (connectedBody)
		{
			if (connectedBody.isKinematic || connectedBody.mass >= body.mass)
			{
				UpdateConnectionState();
			}
		}
	}

	void UpdateConnectionState()
	{
		if (connectedBody == previousConnectedBody)
		{
			Vector3 connectionMovement =
				connectedBody.transform.TransformPoint(connectionLocalPosition) -
				connectionWorldPosition;
			connectionVelocity = connectionMovement / Time.deltaTime;
		}
		connectionWorldPosition = body.position;
		connectionLocalPosition = connectedBody.transform.InverseTransformPoint(
			connectionWorldPosition
		);
	}

	bool SnapToGround()
	{
		if (stepsSinceLastGrounded > 1 || stepsSinceLastTakeoff <= 5)
		{
			return false;
		}
		float speed = velocity.magnitude;
		if (speed > maxSnapSpeed)
		{
			return false;
		}
		if (!Physics.Raycast(body.position, -upAxis, out RaycastHit hit,probeDistance, probeMask))
		{
			return false;
		}

		float upDot = Vector3.Dot(upAxis, hit.normal);
		if (upDot < GetMinDot(hit.collider.gameObject.layer))
		{
			return false;
		}

		groundContactCount = 1;
		contactNormal = hit.normal;
		float dot = Vector3.Dot(velocity, hit.normal);
		if (dot > 0f)
		{
			velocity = (velocity - hit.normal * dot).normalized * speed;
		}
		connectedBody = hit.rigidbody;
		return true;
	}

	bool CheckSteepContacts()
	{
		if (steepContactCount > 1)
		{
			steepNormal.Normalize();
			float upDot = Vector3.Dot(upAxis, steepNormal);
			if (upDot >= minGroundDotProduct)
			{
				steepContactCount = 0;
				groundContactCount = 1;
				contactNormal = steepNormal;
				return true;
			}
		}
		return false;
	}

	void AdjustVelocity()
	{
		Vector3 xAxis = ProjectDirectionOnPlane(rightAxis, contactNormal);
		Vector3 zAxis = ProjectDirectionOnPlane(forwardAxis, contactNormal);
		Vector3 yAxis = upAxis;

		Vector3 relativeVelocity = velocity - connectionVelocity;
		float currentX = Vector3.Dot(relativeVelocity, xAxis);
		float currentZ = Vector3.Dot(relativeVelocity, zAxis);
		float currentY = Vector3.Dot(relativeVelocity, yAxis);

		float acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
		float maxSpeedChange = acceleration * Time.deltaTime;
		
		float newX =
			Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);
		float newZ =
			Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);
		float newY =
			Mathf.MoveTowards(currentY, desiredVelocity.y, maxSpeedChange);
		if (!flying)
		{
			velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
		}
		else
		{
			velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ) + yAxis * (newY - currentY);
		}
	}
	void AdjustRotation()
	{
		float speed = flying ? rotateSpeedAir : rotateSpeed;
		if (body.velocity.magnitude > 0.1f)
		{

			// need to rotate body and sphere different, to use sphere as a base for body rotation
			// find dot product of sphere forward and body forward
			// if less than .7, clamp the body forward.

			Vector3 forward = new Vector3(body.velocity.x, body.velocity.y, body.velocity.z);
			Quaternion forwardlook = Quaternion.LookRotation(forward, contactNormal);
			transform.rotation = Quaternion.RotateTowards(body.rotation, forwardlook, speed);
			lastRotation = transform.rotation;
		}
		else
		{
			transform.rotation = lastRotation;
		}


	}

	void BeginFlying(Vector3 gravity)
	{
		Vector3 jumpDirection;
		if (OnGround)
		{
			jumpDirection = contactNormal;
		}
		else
		{
			return;
		}
		stepsSinceLastTakeoff = 0;
		
		float jumpSpeed = Mathf.Sqrt(2f * gravity.magnitude * jumpHeight);
		jumpDirection = (jumpDirection + upAxis).normalized;
		float alignedSpeed = Vector3.Dot(velocity, jumpDirection);
		if (alignedSpeed > 0f)
		{
			jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
		}
		velocity += jumpDirection * jumpSpeed;
		flying = true;
	}

	void OnCollisionEnter(Collision collision)
	{
		EvaluateCollision(collision);
	}

	void OnCollisionStay(Collision collision)
	{
		EvaluateCollision(collision);
	}

	void EvaluateCollision(Collision collision)
	{
		float minDot = GetMinDot(collision.gameObject.layer);
		for (int i = 0; i < collision.contactCount; i++)
		{
			Vector3 normal = collision.GetContact(i).normal;
			float upDot = Vector3.Dot(upAxis, normal);
			if (upDot >= minDot)
			{
				groundContactCount += 1;
				contactNormal += normal;
				connectedBody = collision.rigidbody;
				if(stepsSinceLastTakeoff > 10)
				{
					flying = false;
				}
				
			}
			else if (upDot > -0.01f)
			{
				steepContactCount += 1;
				steepNormal += normal;
				if (groundContactCount == 0)
				{
					connectedBody = collision.rigidbody;
				}
			}
		}
	}

	Vector3 ProjectDirectionOnPlane(Vector3 direction, Vector3 normal)
	{
		return (direction - normal * Vector3.Dot(direction, normal)).normalized;
	}

	float GetMinDot(int layer)
	{
		return (stairsMask & (1 << layer)) == 0 ?
			minGroundDotProduct : minStairsDotProduct;
	}
	public bool GetFlying()
	{
		return flying;
	}
	public int GetStepsSinceLastTakeoff()
	{
		return stepsSinceLastTakeoff;
	}
	public int GetStepsSinceLastGrounded()
	{
		return stepsSinceLastGrounded;
	}
}
