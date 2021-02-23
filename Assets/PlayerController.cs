using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Rigidbody rb;

    float verticalInput;
    
    Vector3 velocity, desiredVelocity;
    Vector3 playerInput;

    float flightStartTime;
    bool isFlying;

    [SerializeField] float maxAcceleration, maxSpeed;

    [SerializeField, Range(0f, 90f)] float maxGroundAngle = 25f;
    [SerializeField] GameObject lookPoint;
    float minGroundDotProduct;

    Vector3 contactNormal;

    bool OnGround => groundContactCount > 0;
    int groundContactCount;


    // Start is called before the first frame update
    private void Awake()
	{
        
        rb = GetComponent<Rigidbody>();
        isFlying = false;
        OnValidate();
    }
	void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    void OnValidate()
    {
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
    }
    // Update is called once per frame
    void Update()
	{
		CheckForInput();



        desiredVelocity =
            new Vector3(playerInput.x, 0f, playerInput.z) * maxSpeed;

        if (Input.GetKeyDown(KeyCode.Space))
		{
			flightStartTime = Time.time;
		}
		else if (Input.GetKey(KeyCode.Space))
		{
			if ((Time.time - flightStartTime > 1f))
			{
				flightStartTime = float.PositiveInfinity; // used to catch subsequent runs of this code in another update
				print("Flight started!");
				isFlying = true;
			}
		}
		else
		{
			flightStartTime = float.PositiveInfinity;
		}

	}

	private void CheckForInput()
	{
		playerInput.x = Input.GetAxis("Horizontal");
		playerInput.z = Input.GetAxis("Vertical");
		if (isFlying)
		{
			playerInput.y = Input.GetAxis("Jump");
		}
		else
		{
			playerInput.y = 0f;

		}
        Vector3.ClampMagnitude(playerInput, 1f);
    }

	void FixedUpdate()
	{

        velocity = rb.velocity;
        float maxSpeedChange = maxAcceleration * Time.deltaTime;
        velocity.x =
            Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
        velocity.z =
            Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);
        rb.velocity = velocity;

        if (isFlying)
		{
            CancelYVelocity();
        }



        Vector3 targetPosition = new Vector3(lookPoint.transform.position.x, transform.position.y, lookPoint.transform.position.z);
        transform.LookAt(targetPosition, Vector3.up);
    }

	private void CancelYVelocity()
	{
		float yTotalVelocity = velocity.y + Physics.gravity.y;
		rb.AddForce(0, -yTotalVelocity, 0, ForceMode.Acceleration);
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
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;
            if (normal.y >= minGroundDotProduct)
            {
                groundContactCount += 1;
                contactNormal += normal;
            }
        }
    }
}
