using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flower : MonoBehaviour
{
    float pollenAmount;
    [SerializeField] FlowerValue flowerValue;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	private void OnCollisionStay(Collision collision)
	{
        GameObject collided = collision.gameObject;
	}
}
