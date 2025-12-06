using UnityEngine;
using System.Collections;

public class GoalCheck : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ball"))
        {
            BallPhysics ball = other.GetComponent<BallPhysics>();
            if (ball != null && ball.GetHeight() < 3.5f && !ball.isOutOfPlay)
            {
                Debug.Log("GOOOOL!");
                MatchManager.instance.OnStarPlayerGoal();
                MatchManager.instance.EndPlaySession();
            }
        }
    }
}