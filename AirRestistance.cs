using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class AirRestistance : MonoBehaviour {

    private const float RHO = 1.2f;
    private const float CD = 1.5f;
    private const float DRAG_COEFFICIENT = RHO * CD; 

    [SerializeField] private Vector3 _windVelocity;
    [SerializeField] private Rigidbody _rigidBody;

    [SerializeField] private bool _windField;

    private Mesh _mesh;

    private void Awake() {
        _mesh = GetComponent<MeshFilter>().mesh;
    }

    private void FixedUpdate() {
        Matrix4x4 localToWorld = transform.localToWorldMatrix;
        
        //apply force for each traingle of the mesh
        for (int i = 0; i < _mesh.triangles.Length; i += 3) {
            Vector3[] triangle = new Vector3[] {
                localToWorld.MultiplyPoint(_mesh.vertices[_mesh.triangles[i + 0]]),
                localToWorld.MultiplyPoint(_mesh.vertices[_mesh.triangles[i + 1]]),
                localToWorld.MultiplyPoint(_mesh.vertices[_mesh.triangles[i + 2]])
            };

            Vector3 normal = GetNormalOfTriangle(triangle);
            Vector3 center = GetCenterOfTraingle(triangle);
            Vector3 realtiveVelocity = (_windField ? GetWindVelocityAtPoint(center) : Vector3.zero) - _rigidBody.GetPointVelocity(center);

            // if the triangle faces the relativ velocity.
            if (Vector3.Dot(normal, realtiveVelocity) < 0f) {
                float area = GetAreaOfTriangle(triangle);
                float projectedArea = area * Vector3.Dot(-normal, realtiveVelocity);

                Vector3 dragForce =  0.5f * DRAG_COEFFICIENT * Mathf.Pow(realtiveVelocity.magnitude, 2f) * projectedArea * realtiveVelocity.normalized;
                Vector3 effectiveForce = Vector3.Project(dragForce, -normal);
                
                _rigidBody.AddForceAtPosition(effectiveForce, center);
            }
        }
    }

    
    // returns the normalized normal of a traingle.
    private Vector3 GetNormalOfTriangle(Vector3[] triangle) {
        Vector3 normal = new Vector3();
        Vector3 U = triangle[1] - triangle[0];
        Vector3 V = triangle[2] - triangle[0];

        normal.x = U.y * V.z - U.z * V.y;
        normal.y = U.z * V.x - U.x * V.z;
        normal.z = U.x * V.y - U.y * V.x;

        return normal.normalized;
    }

    // returns the area of a given triangle.
    private float GetAreaOfTriangle(Vector3[] triangle) {
        Vector3 U = triangle[1] - triangle[0];
        Vector3 V = triangle[2] - triangle[0];

        return 0.5f * Vector3.Cross(U, V).magnitude;
    }

    private Vector3 GetCenterOfTraingle(Vector3[] triangle) {
        return (triangle[0] + triangle[1] + triangle[2]) / 3f;
    }
}
