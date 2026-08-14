using System.Collections.Generic;
using UnityEngine;

public class ShapeSlicer : MonoBehaviour
{
    public float splitSpeed = 2.5f;
    public float fragmentGravity = 2f;
    public float fragmentLifetime = 3f;

    public int circleResolution = 32;

    public void Slice(Vector2 worldCutPoint, Vector2 worldCutDirection)
    {
        List<Vector2> originalPolygon = GetShapePolygon();

        if (originalPolygon == null || originalPolygon.Count < 3)
            return;

        // Convert cut information into this object's local space.
        Vector2 localCutPoint =
            transform.InverseTransformPoint(worldCutPoint);

        Vector2 secondWorldPoint =
            worldCutPoint + worldCutDirection;

        Vector2 localSecondPoint =
            transform.InverseTransformPoint(secondWorldPoint);

        Vector2 localCutDirection =
            (localSecondPoint - localCutPoint).normalized;

        List<Vector2> sideA =
            ClipPolygon(
                originalPolygon,
                localCutPoint,
                localCutDirection,
                true
            );

        List<Vector2> sideB =
            ClipPolygon(
                originalPolygon,
                localCutPoint,
                localCutDirection,
                false
            );

        if (sideA.Count < 3 || sideB.Count < 3)
            return;

        Vector2 worldNormal =
            new Vector2(
                -worldCutDirection.y,
                worldCutDirection.x
            ).normalized;

        Rigidbody2D originalRb =
            GetComponent<Rigidbody2D>();

        Vector2 inheritedVelocity = Vector2.zero;

        if (originalRb != null)
        {
            inheritedVelocity =
                originalRb.linearVelocity;
        }

        CreateFragment(
            sideA,
            worldNormal,
            inheritedVelocity
        );

        CreateFragment(
            sideB,
            -worldNormal,
            inheritedVelocity
        );
    }

    private List<Vector2> GetShapePolygon()
    {
        PolygonCollider2D polygon =
            GetComponent<PolygonCollider2D>();

        if (polygon != null)
        {
            return new List<Vector2>(
                polygon.points
            );
        }

        BoxCollider2D box =
            GetComponent<BoxCollider2D>();

        if (box != null)
        {
            Vector2 half =
                box.size * 0.5f;

            Vector2 offset =
                box.offset;

            return new List<Vector2>
            {
                offset + new Vector2(-half.x, -half.y),
                offset + new Vector2( half.x, -half.y),
                offset + new Vector2( half.x,  half.y),
                offset + new Vector2(-half.x,  half.y)
            };
        }

        CircleCollider2D circle =
            GetComponent<CircleCollider2D>();

        if (circle != null)
        {
            List<Vector2> points =
                new List<Vector2>();

            for (int i = 0; i < circleResolution; i++)
            {
                float angle =
                    Mathf.PI * 2f *
                    i /
                    circleResolution;

                Vector2 point =
                    circle.offset +
                    new Vector2(
                        Mathf.Cos(angle),
                        Mathf.Sin(angle)
                    ) * circle.radius;

                points.Add(point);
            }

            return points;
        }

        return null;
    }

    private List<Vector2> ClipPolygon(
        List<Vector2> polygon,
        Vector2 cutPoint,
        Vector2 cutDirection,
        bool keepPositive)
    {
        List<Vector2> result =
            new List<Vector2>();

        for (int i = 0; i < polygon.Count; i++)
        {
            Vector2 current =
                polygon[i];

            Vector2 next =
                polygon[(i + 1) % polygon.Count];

            float currentSide =
                SideOfLine(
                    current,
                    cutPoint,
                    cutDirection
                );

            float nextSide =
                SideOfLine(
                    next,
                    cutPoint,
                    cutDirection
                );

            bool currentInside =
                keepPositive
                    ? currentSide >= 0f
                    : currentSide <= 0f;

            bool nextInside =
                keepPositive
                    ? nextSide >= 0f
                    : nextSide <= 0f;

            if (currentInside)
            {
                result.Add(current);
            }

            if (currentInside != nextInside)
            {
                Vector2 intersection =
                    LineIntersection(
                        current,
                        next,
                        cutPoint,
                        cutDirection
                    );

                result.Add(intersection);
            }
        }

        return result;
    }

    private float SideOfLine(
        Vector2 point,
        Vector2 linePoint,
        Vector2 direction)
    {
        Vector2 relative =
            point - linePoint;

        return
            direction.x * relative.y -
            direction.y * relative.x;
    }

    private Vector2 LineIntersection(
        Vector2 segmentStart,
        Vector2 segmentEnd,
        Vector2 linePoint,
        Vector2 lineDirection)
    {
        Vector2 segmentDirection =
            segmentEnd - segmentStart;

        float denominator =
            Cross(
                segmentDirection,
                lineDirection
            );

        if (Mathf.Abs(denominator) < 0.0001f)
            return segmentStart;

        float t =
            Cross(
                linePoint - segmentStart,
                lineDirection
            ) / denominator;

        return
            segmentStart +
            segmentDirection * t;
    }

    private float Cross(
        Vector2 a,
        Vector2 b)
    {
        return a.x * b.y -
               a.y * b.x;
    }

    private void CreateFragment(
        List<Vector2> polygon,
        Vector2 pushDirection,
        Vector2 inheritedVelocity)
    {
        Vector2 center =
            GetCenter(polygon);

        GameObject fragment =
            new GameObject("ShapeFragment");

        fragment.transform.position =
            transform.TransformPoint(center);

        fragment.transform.rotation =
            transform.rotation;

        fragment.transform.localScale =
            transform.lossyScale;

        List<Vector2> centeredPoints =
            new List<Vector2>();

        foreach (Vector2 point in polygon)
        {
            centeredPoints.Add(
                point - center
            );
        }

        Mesh mesh =
            CreateMesh(centeredPoints);

        MeshFilter filter =
            fragment.AddComponent<MeshFilter>();

        filter.mesh = mesh;

        MeshRenderer meshRenderer =
            fragment.AddComponent<MeshRenderer>();

        Material material =
            new Material(
                Shader.Find("Sprites/Default")
            );

        SpriteRenderer originalRenderer =
            GetComponent<SpriteRenderer>();

        if (originalRenderer != null)
        {
            material.color =
                originalRenderer.color;

            meshRenderer.sortingLayerID =
                originalRenderer.sortingLayerID;

            meshRenderer.sortingOrder =
                originalRenderer.sortingOrder;
        }
        else
        {
            material.color =
                Color.white;
        }

        meshRenderer.material =
            material;

        PolygonCollider2D collider =
            fragment.AddComponent<PolygonCollider2D>();

        collider.points =
            centeredPoints.ToArray();

        Rigidbody2D rb =
            fragment.AddComponent<Rigidbody2D>();

        rb.gravityScale =
            fragmentGravity;

        rb.linearVelocity =
            inheritedVelocity +
            pushDirection * splitSpeed;

        rb.angularVelocity =
            Random.Range(-150f, 150f);

        Destroy(
            fragment,
            fragmentLifetime
        );
    }

    private Vector2 GetCenter(
        List<Vector2> polygon)
    {
        Vector2 center =
            Vector2.zero;

        foreach (Vector2 point in polygon)
        {
            center += point;
        }

        return center / polygon.Count;
    }

    private Mesh CreateMesh(
        List<Vector2> points)
    {
        Mesh mesh =
            new Mesh();

        Vector3[] vertices =
            new Vector3[points.Count];

        for (int i = 0; i < points.Count; i++)
        {
            vertices[i] =
                new Vector3(
                    points[i].x,
                    points[i].y,
                    0f
                );
        }

        int triangleCount =
            points.Count - 2;

        int[] triangles =
            new int[triangleCount * 3];

        for (int i = 0; i < triangleCount; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        mesh.vertices =
            vertices;

        mesh.triangles =
            triangles;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        return mesh;
    }
}
