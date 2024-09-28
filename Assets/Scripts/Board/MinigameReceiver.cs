using UnityEngine;

public class MinigameReceiver : ReceiverBase
{
    [SerializeField] private Minigame.Type minigameType;

    protected override void OnEnter(Collider2D collision)
    {
        base.OnEnter(collision);
        GameManager.Instance.StartMinigame(minigameType, OnEnd);
    }

    private void OnEnd()
    {
        isWaiting = false;
    }
}