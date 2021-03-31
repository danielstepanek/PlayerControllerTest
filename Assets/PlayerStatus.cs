using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatus : MonoBehaviour
{
    public float playerHunger = 100f;
    public float playerWater = 100f;

    public float hungerSpeed;
    public float waterSpeed;

    public bool isPollinating = false;
    public bool isDrinking = false;

    [SerializeField] Image hungerBar;
    [SerializeField] Image waterBar;
    [SerializeField] float flower = 1f;

    void Start()
    {
        hungerBar.fillAmount = 1f;
        waterBar.fillAmount = 1f;
    }

    // Update is called once per frame
    void Update()
    {
		if (!isPollinating)
		{
            playerHunger -= hungerSpeed * Time.deltaTime;
            float fillAmount = Remap(playerHunger, 0f, 100f, 0f, 1f);
            hungerBar.fillAmount = fillAmount;
		}
		else
		{
            FeedPlayer(flower);
		}

		if (!isDrinking)
		{
            playerWater -= waterSpeed * Time.deltaTime;
            float fillAmount = Remap(playerWater, 0f, 100f, 0f, 1f);
            waterBar.fillAmount = fillAmount;
        }
		else
		{

		}
    }
    void FeedPlayer(float flowerValue)
	{
        playerHunger += flowerValue * Time.deltaTime;
	}
    public static float Remap(float input, float oldLow, float oldHigh, float newLow, float newHigh)
    {
        float t = Mathf.InverseLerp(oldLow, oldHigh, input);
        return Mathf.Lerp(newLow, newHigh, t);
    }
}
