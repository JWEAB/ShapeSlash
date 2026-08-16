using UnityEngine;

public class SlashLine : MonoBehaviour
{
    private bool isStillBeingDrawn = true;

    private SwipeController owner;
    private EdgeCollider2D edgeCollider;

    // Lower = allows more curved/angled basket bottoms.
    public float catchSlopeTolerance = 1f;

    public void Initialize(
        SwipeController swipeController)
    {
        owner = swipeController;

        edgeCollider =
            GetComponent<EdgeCollider2D>();
    }

    public void FinishDrawing()
    {
        isStillBeingDrawn = false;
    }

    private void OnTriggerEnter2D(
        Collider2D other)
    {
        HandleShapeContact(other);
    }

    private void OnTriggerStay2D(
        Collider2D other)
    {
        // Stay is useful for basket shapes.
        // A yellow object might first touch a side
        // and then move down toward the bottom.
        HandleShapeContact(other);
    }

    private void HandleShapeContact(
        Collider2D other)
    {
        FallingShape shape =
            other.GetComponent<FallingShape>();

        if (shape == null)
            return;

        if (GameManager.Instance.IsGameOver)
            return;

        if (shape !=
            GameManager.Instance.CurrentShape)
        {
            return;
        }

        // -------------------------
        // WHITE
        // -------------------------

        if (shape.behavior ==
            ShapeBehavior.WhiteCut)
        {
            // While still drawing,
            // touching white cuts it.
            if (isStillBeingDrawn)
            {
                if (owner != null)
                {
                    owner.CutShapeFromExistingLine(
                        shape,
                        this
                    );
                }
            }
            else
            {
                // Old abandoned line.
                GameManager.Instance.GameOver();
            }

            return;
        }

        // -------------------------
        // YELLOW
        // -------------------------

        if (shape.behavior ==
            ShapeBehavior.YellowCatch)
        {
            Vector2 catchPoint;
            Vector2 segmentDirection;

            bool canCatch =
                GetCatchSegment(
                    shape.transform.position,
                    out catchPoint,
                    out segmentDirection
                );

            if (!canCatch)
                return;

            // The yellow object needs to actually
            // be ABOVE the platform/basket segment.
            if (shape.transform.position.y <
                catchPoint.y)
            {
                return;
            }

            if (owner != null)
            {
                owner.CatchYellowShape(
                    shape,
                    gameObject
                );
            }
        }
    }

    private bool GetCatchSegment(
        Vector2 shapePosition,
        out Vector2 catchPoint,
        out Vector2 segmentDirection)
    {
        catchPoint = Vector2.zero;
        segmentDirection = Vector2.zero;

        if (edgeCollider == null)
        {
            edgeCollider =
                GetComponent<EdgeCollider2D>();
        }

        Vector2[] linePoints =
            edgeCollider.points;

        if (
            linePoints == null ||
            linePoints.Length < 2
        )
        {
            return false;
        }

        float closestDistance =
            float.MaxValue;

        for (
            int i = 0;
            i < linePoints.Length - 1;
            i++
        )
        {
            Vector2 start =
                transform.TransformPoint(
                    linePoints[i]
                );

            Vector2 end =
                transform.TransformPoint(
                    linePoints[i + 1]
                );

            Vector2 closest =
                ClosestPointOnSegment(
                    shapePosition,
                    start,
                    end
                );

            float distance =
                Vector2.Distance(
                    shapePosition,
                    closest
                );

            if (distance < closestDistance)
            {
                Vector2 direction =
                    (end - start).normalized;

                closestDistance =
                    distance;

                catchPoint =
                    closest;

                segmentDirection =
                    direction;
            }
        }

        if (segmentDirection ==
            Vector2.zero)
        {
            return false;
        }

        // Segment must be reasonably flat.
        bool sufficientlyHorizontal =
            Mathf.Abs(segmentDirection.x) >=
            Mathf.Abs(segmentDirection.y) *
            catchSlopeTolerance;

        return sufficientlyHorizontal;
    }

    private Vector2 ClosestPointOnSegment(
        Vector2 point,
        Vector2 start,
        Vector2 end)
    {
        Vector2 segment =
            end - start;

        float lengthSquared =
            segment.sqrMagnitude;

        if (lengthSquared == 0f)
            return start;

        float t =
            Vector2.Dot(
                point - start,
                segment
            ) / lengthSquared;

        t = Mathf.Clamp01(t);

        return start +
               segment * t;
    }

    public bool GetClosestCut(
        Vector2 shapePosition,
        out Vector2 cutPoint,
        out Vector2 cutDirection)
    {
        cutPoint = Vector2.zero;
        cutDirection = Vector2.zero;

        if (edgeCollider == null)
        {
            edgeCollider =
                GetComponent<EdgeCollider2D>();
        }

        Vector2[] linePoints =
            edgeCollider.points;

        if (linePoints == null ||
            linePoints.Length < 2)
        {
            return false;
        }

        float closestDistance =
            float.MaxValue;

        for (int i = 0;
            i < linePoints.Length - 1;
            i++)
        {
            Vector2 start =
                transform.TransformPoint(
                    linePoints[i]
                );

            Vector2 end =
                transform.TransformPoint(
                    linePoints[i + 1]
                );

            Vector2 closest =
                ClosestPointOnSegment(
                    shapePosition,
                    start,
                    end
                );

            float distance =
                Vector2.Distance(
                    shapePosition,
                    closest
                );

            if (distance < closestDistance)
            {
                closestDistance = distance;

                cutPoint = closest;

                cutDirection =
                    (end - start).normalized;
            }
        }

        return cutDirection != Vector2.zero;
    }
}
