using UnityEngine;

public class MissZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        FallingShape shape = other.GetComponent<FallingShape>();

        if (shape != null)
        {
            GameManager.Instance.GameOver();
        }
    }
}