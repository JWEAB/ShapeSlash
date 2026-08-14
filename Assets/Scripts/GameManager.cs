using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Shape Spawning")]
    public GameObject[] whiteShapePrefabs;
    // Score Variables //
    public TMP_Text scoreText;
    public TMP_Text gameOverText;
    //----------------//
    public float spawnY = 5.7f;
    public float minimumSpawnX = -2.2f;
    public float maximumSpawnX = 2.2f;

    public float startingFallSpeed = 3.5f;
    public float speedIncreasePerPoint = 0.2f;
    public float maximumFallSpeed = 8f;
    public float spawnDelay = 0.4f;

    public int Score { get; private set; }

    public bool IsGameOver { get; private set; }

    public FallingShape CurrentShape { get; private set; }

    public GameObject CurrentSlash { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Score = 0;
        IsGameOver = false;

        UpdateScoreText();

        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
        }

        SpawnShape();
    }

    private void Update()
    {
        if (IsGameOver && Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = Score.ToString();
        }
    }

    private void SpawnShape()
    {
        if (IsGameOver)
            return;

        float randomX = Random.Range(minimumSpawnX, maximumSpawnX);

        Vector3 spawnPosition = new Vector3(
            randomX,
            spawnY,
            0f
        );

        int randomShapeIndex =
            Random.Range(0, whiteShapePrefabs.Length);

        GameObject selectedShape =
            whiteShapePrefabs[randomShapeIndex];

        GameObject newShape = Instantiate(
            selectedShape,
            spawnPosition,
            Quaternion.identity
        );

        CurrentShape = newShape.GetComponent<FallingShape>();

        float currentFallSpeed =
            startingFallSpeed + (Score * speedIncreasePerPoint);

        currentFallSpeed =
            Mathf.Min(currentFallSpeed, maximumFallSpeed);

        CurrentShape.SetFallSpeed(currentFallSpeed);
    }

    public bool CanDraw()
    {
        return !IsGameOver
            && CurrentShape != null
            && CurrentSlash == null;
    }

    public void RegisterSlash(GameObject slash)
    {
        CurrentSlash = slash;
    }

    public void SuccessfulCut(FallingShape shape)
    {
        if (IsGameOver)
            return;

        if (shape != CurrentShape)
            return;

        Score++;
        UpdateScoreText();

        Debug.Log("Score: " + Score);

        Destroy(CurrentShape.gameObject);

        if (CurrentSlash != null)
        {
            Destroy(CurrentSlash);
        }

        CurrentShape = null;
        CurrentSlash = null;

        Invoke(nameof(SpawnShape), spawnDelay);
    }

    public void GameOver()
    {
        if (IsGameOver)
            return;

        IsGameOver = true;

        if (CurrentShape != null)
        {
            CurrentShape.StopMoving();
        }

        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
        }

        Debug.Log("GAME OVER - Final Score: " + Score);
    }

    private void RestartGame()
    {
        SwipeController swipeController =
            FindFirstObjectByType<SwipeController>();

        if (swipeController != null)
        {
            swipeController.ClearSlash();
        }

        if (CurrentShape != null)
        {
            Destroy(CurrentShape.gameObject);
        }

        if (CurrentSlash != null)
        {
            Destroy(CurrentSlash);
        }

        CurrentShape = null;
        CurrentSlash = null;

        Score = 0;
        IsGameOver = false;

        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
        }

        UpdateScoreText();

        Debug.Log("Game Restarted");

        SpawnShape();
    }
}
