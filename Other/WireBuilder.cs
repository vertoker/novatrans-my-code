using System.Collections.Generic;
using CreativeMode.Builders.Behaviours;
using CreativeMode.Builders.Data;
using UnityEngine;
using VRF.Utilities;
using VRF.Utilities.Extensions;

namespace CreativeMode.MeshWire
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class WireBuilder : BaseCallbackBuilder
    {
        [Space] [SerializeField] private bool updatePointsWithMesh = true;
        [SerializeField] private Transform[] points;
        [SerializeField] private int circleQuality = 5;
        [SerializeField] private float radius = 0.1f;

        private readonly List<Vector3> positionsCache = new();
        private Mesh mesh;
        private MeshFilter filter;

        public override void UpdateBuilder(UpdateBuilderFlag flag = UpdateBuilderFlag.Default)
        {
            if (!mesh)
                mesh = new Mesh();
            if (!filter)
                filter = GetComponent<MeshFilter>();
            if (filter.sharedMesh != mesh)
                filter.sharedMesh = mesh;
            if (updatePointsWithMesh)
                UpdatePoints();
            GenerateMesh(positionsCache);
        }

        private void GenerateMesh(List<Vector3> positions)
        {
            if (!mesh)
                return;
            if (circleQuality < 2)
                return;
            if (points == null)
                return;
            var count = points.Length;
            if (count < 2)
                return;

            positions.EnsureCapacity(count);
            positions.Clear();

            var countFull = count;
            for (var i = 0; i < countFull; i++)
            {
                var point = points[i];
                if (point == null)
                    count--;
                else
                    positions.Add(point.localPosition);
            }

            if (count < 2)
                return;

            //var sideQuality = circleQuality - 1;
            var verticesCount = circleQuality * count;
            var trianglesCount = circleQuality * 6 * (count - 1);
            var vertices = new Vector3[verticesCount];
            var triangles = new int[trianglesCount];

            var counterPoint = 0;
            var counterVertex = 0;
            var counterTriangle = 0;

            while (counterPoint < count)
            {
                // Calculate point
                var pos = positions[counterPoint];

                /*if (gizmos)
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawCube(pos, Vector3.one * 0.1f);
                }*/

                var normal = counterPoint == 0 ? (positions[counterPoint + 1] - pos).normalized :
                    counterPoint == count - 1 ? (pos - positions[counterPoint - 1]).normalized :
                    VrfVector3.Average((pos - positions[counterPoint - 1]).normalized,
                        (positions[counterPoint + 1] - pos).normalized);

                var counterLocalVertex = 0;
                while (counterLocalVertex < circleQuality)
                {
                    var angleRad = counterLocalVertex / (float)circleQuality * 2 * Mathf.PI;
                    vertices[counterVertex] = pos + VrfMath.RotateVector(normal, angleRad, radius);

                    /*if (gizmos)
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawCube(vertices[counterVertex], Vector3.one * 0.1f);
                    }*/

                    counterVertex++;
                    counterLocalVertex++;
                }

                counterPoint++;
            }

            counterPoint = 1;
            while (counterPoint < count)
            {
                var current = (counterPoint - 1) * circleQuality;
                var next = counterPoint * circleQuality;
                var delta = (positions[counterPoint] - positions[counterPoint - 1]).normalized;
                ClosestNextIndex(vertices, current, next, delta, out var counterLocalTriangleC1,
                    out var counterLocalTriangleN1);

                for (var i = 0; i < circleQuality; i++)
                {
                    var counterLocalTriangleC2 = VrfMath.ArrayOverlap(counterLocalTriangleC1 + 1, circleQuality);
                    var counterLocalTriangleN2 = VrfMath.ArrayOverlap(counterLocalTriangleN1 + 1, circleQuality);

                    triangles[counterTriangle + 0] = current + counterLocalTriangleC1;
                    triangles[counterTriangle + 2] = next + counterLocalTriangleN1;
                    triangles[counterTriangle + 1] = current + counterLocalTriangleC2;
                    triangles[counterTriangle + 3] = next + counterLocalTriangleN2;
                    triangles[counterTriangle + 5] = current + counterLocalTriangleC2;
                    triangles[counterTriangle + 4] = next + counterLocalTriangleN1;
                    counterTriangle += 6;

                    counterLocalTriangleC1 = counterLocalTriangleC2;
                    counterLocalTriangleN1 = counterLocalTriangleN2;
                }

                counterPoint++;
            }

            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
        }


        private void ClosestNextIndex(Vector3[] vertices, int currentStart, int nextStart, Vector3 delta,
            out int indexCurrent, out int indexNext)
        {
            indexCurrent = indexNext = 0;

            //Debug.Log(123123123);
            var maxDot = float.MinValue;
            var startPos = vertices[currentStart];
            //Debug.Log(delta);

            var counterLocalTriangleNext = 0;
            while (counterLocalTriangleNext < circleQuality)
            {
                var vertexDelta = (vertices[nextStart + counterLocalTriangleNext] - startPos).normalized;
                var dot = Vector3.Dot(delta, vertexDelta);
                //Debug.Log(vertexDelta);
                //Debug.Log(dot);

                if (dot > maxDot)
                {
                    maxDot = dot;
                    indexNext = counterLocalTriangleNext;
                }

                counterLocalTriangleNext++;
            }
            //Debug.Log(maxDot);
        }

        public void UpdatePoints()
        {
            var self = transform;
            var length = self.childCount;
            points = new Transform[length];

            for (var i = 0; i < length; i++)
            {
                points[i] = self.GetChild(i);
                points[i].gameObject.name = $"Point ({i + 1})";
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(points[i].gameObject);
#endif
            }
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}