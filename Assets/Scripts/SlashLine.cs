using UnityEngine;

public class SlashLine : MonoBehaviour
{
    public bool isVertical;

    private void OnTriggerEnter2D(Collider2D other)
    {
        FallingShape shape =
            other.GetComponent<FallingShape>();

        if (shape == null)
            return;

        if (GameManager.Instance.IsGameOver)
            return;

        // For WHITE shapes:
        //
        // If the object reaches a slash later,
        // that means the player slashed too early.
        //
        // White objects must be sliced while
        // the finger actually crosses the object.

        GameManager.Instance.GameOver();
    }
}
