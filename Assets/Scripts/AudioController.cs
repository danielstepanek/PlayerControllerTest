using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    [SerializeField] AudioSource playerAudio;
    [SerializeField] AudioClip wingsBuzz;
    [SerializeField] SphereController controller;

    bool isFlying = false;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
     isFlying = controller.GetFlying();

		if (isFlying)
		{
            playerAudio.loop = isFlying;
			if (playerAudio.isPlaying)
			{
                return;
			}
			else
			{
                playerAudio.Play();
            }
		}
		else
		{
            playerAudio.Stop();
		}
    }
}
