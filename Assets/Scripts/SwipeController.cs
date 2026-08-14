using System.Collections.Generic;
using UnityEngine;

public class SwipeController : MonoBehaviour
{
    private Camera mainCamera;

    private bool isDrawing = false;

    private GameObject currentSlash;
    private LineRenderer lineRenderer;
    private EdgeCollider2D edgeCollider;

    private List<Vector2> points = new List<Vector2>();

    // How far the finger has to move before
    // another point is added to the line.
    public float directionCheckDistance = 0.4f;
    public float minimumPointDistance = 0.05f;

    public float lineWidth = 0.08f;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (GameManager.Instance == null)
            return;

        if (GameManager.Instance.IsGameOver)
            return;

        HandleMouseInput();
        HandleTouchInput();
    }

    // --------------------------------------------------
    // MOUSE INPUT
    // Used while testing the game on your Windows PC.
    // --------------------------------------------------

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartDrawing(Input.mousePosition);
        }

        if (Input.GetMouseButton(0) && isDrawing)
        {
            ContinueDrawing(Input.mousePosition);
        }

        if (Input.GetMouseButtonUp(0) && isDrawing)
        {
            FinishDrawing();
        }
    }

    // --------------------------------------------------
    // TOUCH INPUT
    // Used later on the iPhone.
    // --------------------------------------------------

    private void HandleTouchInput()
    {
        if (Input.touchCount == 0)
            return;

        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
        {
            StartDrawing(touch.position);
        }
        else if (
            touch.phase == TouchPhase.Moved ||
            touch.phase == TouchPhase.Stationary)
        {
            ContinueDrawing(touch.position);
        }
        else if (
            touch.phase == TouchPhase.Ended ||
            touch.phase == TouchPhase.Canceled)
        {
            FinishDrawing();
        }
    }

    // --------------------------------------------------
    // START DRAWING
    // --------------------------------------------------

    private void StartDrawing(Vector2 screenPosition)
    {
        // Don't allow another drawing if one already
        // exists waiting for the falling shape.
        if (!GameManager.Instance.CanDraw())
            return;

        isDrawing = true;

        points.Clear();

        currentSlash = new GameObject("Slash");

        // Create visible line.
        lineRenderer =
            currentSlash.AddComponent<LineRenderer>();

        lineRenderer.positionCount = 0;

        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;

        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;

        lineRenderer.numCapVertices = 5;
        lineRenderer.numCornerVertices = 5;

        lineRenderer.material =
            new Material(
                Shader.Find("Sprites/Default")
            );

        // Create collider that follows the drawing.
        edgeCollider =
            currentSlash.AddComponent<EdgeCollider2D>();

        edgeCollider.isTrigger = true;

        // Add our persistent slash behavior.
        currentSlash.AddComponent<SlashLine>();

        Vector2 worldPosition =
            ScreenToWorld(screenPosition);

        AddPoint(worldPosition);
    }

    // --------------------------------------------------
    // CONTINUE DRAWING
    // --------------------------------------------------

    private void ContinueDrawing(Vector2 screenPosition)
    {
        Vector2 newPoint =
            ScreenToWorld(screenPosition);

        if (points.Count == 0)
        {
            AddPoint(newPoint);
            return;
        }

        Vector2 previousPoint =
            points[points.Count - 1];

        // Don't create hundreds of points when the
        // finger barely moved.
        if (Vector2.Distance(
                previousPoint,
                newPoint) < minimumPointDistance)
        {
            return;
        }

        AddPoint(newPoint);

        // Check ONLY the newest little part of the line.
        CheckSegmentHit(
            previousPoint,
            newPoint
        );
    }

    // --------------------------------------------------
    // ADD POINT TO DRAWING
    // --------------------------------------------------

    private void AddPoint(Vector2 point)
    {
        points.Add(point);

        lineRenderer.positionCount =
            points.Count;

        lineRenderer.SetPosition(
            points.Count - 1,
            point
        );

        // EdgeCollider needs at least 2 points.
        if (points.Count >= 2)
        {
            edgeCollider.points =
                points.ToArray();
        }
    }

    // --------------------------------------------------
    // CHECK WHETHER THIS SMALL SEGMENT HIT A SHAPE
    // --------------------------------------------------

    private void CheckSegmentHit(
        Vector2 start,
        Vector2 end)
    {
        RaycastHit2D[] hits =
            Physics2D.LinecastAll(start, end);

        foreach (RaycastHit2D hit in hits)
        {
            FallingShape shape =
                hit.collider.GetComponent<FallingShape>();

            if (shape == null)
                continue;

            Vector2 direction = GetRecentDrawingDirection();

            bool segmentIsHorizontal =
                Mathf.Abs(direction.x) >
                Mathf.Abs(direction.y) * 1.25f;

            // WHITE SHAPE:
            // The exact section touching the object
            // must be horizontal.
            if (segmentIsHorizontal)
            {
                Vector2 cutDirection =
                    direction.normalized;

                ShapeSlicer slicer =
                    shape.GetComponent<ShapeSlicer>();

                if (slicer != null)
                {
                    slicer.Slice(
                        hit.point,
                        cutDirection
                    );
                }

                SuccessfulHit(shape);
            }
            else
            {
                GameManager.Instance.GameOver();
            }
        }
    }

    // --------------------------------------------------
    // CORRECT WHITE HIT
    // --------------------------------------------------

    private void SuccessfulHit(
        FallingShape shape)
    {
        isDrawing = false;

        // This slash hasn't been registered with the
        // GameManager yet, so remove it ourselves.
        if (currentSlash != null)
        {
            Destroy(currentSlash);
        }

        currentSlash = null;

        GameManager.Instance.SuccessfulCut(shape);
    }

    // --------------------------------------------------
    // FINISH DRAWING
    // --------------------------------------------------

    private void FinishDrawing()
    {
        if (!isDrawing)
            return;

        isDrawing = false;

        // A tap isn't a valid line.
        if (points.Count < 2)
        {
            Destroy(currentSlash);
            currentSlash = null;
            return;
        }

        // Keep the finished drawing on the screen.
        GameManager.Instance.RegisterSlash(
            currentSlash
        );

        currentSlash = null;
    }

    // --------------------------------------------------
    // SCREEN POSITION -> GAME WORLD POSITION
    // --------------------------------------------------

    private Vector2 ScreenToWorld(
        Vector2 screenPosition)
    {
        Vector3 position = new Vector3(
            screenPosition.x,
            screenPosition.y,
            -mainCamera.transform.position.z
        );

        Vector3 world =
            mainCamera.ScreenToWorldPoint(position);

        return new Vector2(
            world.x,
            world.y
        );
    }

    public void ClearSlash()
    {
        isDrawing = false;

        if (currentSlash != null)
        {
            Destroy(currentSlash);
            currentSlash = null;
        }

        points.Clear();

        lineRenderer = null;
        edgeCollider = null;
    }

    private Vector2 GetRecentDrawingDirection()
    {
        if (points.Count < 2)
            return Vector2.zero;

        Vector2 newestPoint = points[points.Count - 1];
        Vector2 oldestPoint = newestPoint;

        float distanceTravelled = 0f;

        for (int i = points.Count - 1; i > 0; i--)
        {
            distanceTravelled += Vector2.Distance(
                points[i],
                points[i - 1]
            );

            oldestPoint = points[i - 1];

            if (distanceTravelled >= directionCheckDistance)
                break;
        }

        return newestPoint - oldestPoint;
    }
}
