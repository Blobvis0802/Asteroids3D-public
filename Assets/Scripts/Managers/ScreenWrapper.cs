using UnityEngine;
using System.Collections.Generic;

public class ScreenWrapper : MonoBehaviour
{
    [Header("Wrap Settings")]
    public float buffer = 0.5f; // extra space beyond screen bounds
    public string[] wrapTags = new string[] { "Asteroid", "Player" }; // objects that can wrap

    private Camera mainCamera;
    private float leftBound, rightBound, topBound, bottomBound;

    private List<WrappedObject> wrappedObjects = new List<WrappedObject>();

    void Start()
    {
        mainCamera = Camera.main;
        CalculateScreenBounds();
    }

    void LateUpdate()
    {
        // Update screen bounds in case camera moves
        CalculateScreenBounds();
        // Detect new objects to wrap dynamically
        DetectNewObjects();
        // Draw duplicates at screen edges for visual wrap effect
        DrawEdgeDuplicates();
    }

    void FixedUpdate()
    {
        // Actually wrap objects that cross screen edges
        WrapObjects();
    }

    void CalculateScreenBounds()
    {
        float yDistance = Mathf.Abs(mainCamera.transform.position.y);
        Vector3 bottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, yDistance));
        Vector3 topRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, yDistance));

        leftBound = bottomLeft.x;
        rightBound = topRight.x;
        bottomBound = bottomLeft.z;
        topBound = topRight.z;
    }

    void DetectNewObjects()
    {
        // Find all objects with specified tags
        foreach (string tag in wrapTags)
        {
            GameObject[] objs = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject obj in objs)
            {
                // Skip if already tracked
                if (wrappedObjects.Exists(w => w.obj == obj)) continue;

                Rigidbody rb = obj.GetComponent<Rigidbody>();
                if (rb == null) continue;

                // Add new object to tracking list
                WrappedObject wObj = new WrappedObject
                {
                    obj = obj,
                    rb = rb,
                    renderers = obj.GetComponentsInChildren<MeshRenderer>()
                };
                wrappedObjects.Add(wObj);
            }
        }

        // Remove destroyed objects from list
        wrappedObjects.RemoveAll(w => w.obj == null);
    }

    void WrapObjects()
    {
        // Move objects to opposite side if they go past screen bounds
        foreach (var wObj in wrappedObjects)
        {
            if (wObj.rb == null) continue;

            Vector3 pos = wObj.rb.position;

            if (pos.x < leftBound - buffer) pos.x += (rightBound - leftBound) + 2 * buffer;
            if (pos.x > rightBound + buffer) pos.x -= (rightBound - leftBound) + 2 * buffer;
            if (pos.z < bottomBound - buffer) pos.z += (topBound - bottomBound) + 2 * buffer;
            if (pos.z > topBound + buffer) pos.z -= (topBound - bottomBound) + 2 * buffer;

            wObj.rb.MovePosition(pos);
        }
    }

    void DrawEdgeDuplicates()
    {
        // Draw extra meshes at edges for smooth wrap visuals
        foreach (var wObj in wrappedObjects)
        {
            if (wObj.obj == null) continue;

            foreach (var r in wObj.renderers)
            {
                Mesh mesh = r.GetComponent<MeshFilter>().sharedMesh;
                Material[] materials = r.sharedMaterials;
                Bounds bounds = r.bounds;
                Vector3 objCenter = bounds.center;
                Vector3 objExtents = bounds.extents;

                List<Vector3> duplicateOffsets = new List<Vector3>();

                // Check for wrapping on X-axis
                if (objCenter.x + objExtents.x > rightBound)
                    duplicateOffsets.Add(new Vector3(-(rightBound - leftBound) - 2 * buffer, 0, 0));
                else if (objCenter.x - objExtents.x < leftBound)
                    duplicateOffsets.Add(new Vector3((rightBound - leftBound) + 2 * buffer, 0, 0));

                // Check for wrapping on Z-axis
                if (objCenter.z + objExtents.z > topBound)
                    duplicateOffsets.Add(new Vector3(0, 0, -(topBound - bottomBound) - 2 * buffer));
                else if (objCenter.z - objExtents.z < bottomBound)
                    duplicateOffsets.Add(new Vector3(0, 0, (topBound - bottomBound) + 2 * buffer));

                // Combine offsets if wrapping on both axes
                List<Vector3> finalOffsets = new List<Vector3>();
                if (duplicateOffsets.Count == 1) finalOffsets.Add(duplicateOffsets[0]);
                else if (duplicateOffsets.Count == 2)
                    finalOffsets.Add(new Vector3(duplicateOffsets[0].x, 0, duplicateOffsets[1].z));

                // Draw the duplicated meshes
                foreach (var offset in finalOffsets)
                {
                    Vector3 worldPos = r.transform.position + offset;
                    Matrix4x4 matrix = Matrix4x4.TRS(worldPos, r.transform.rotation, r.transform.lossyScale);

                    for (int sub = 0; sub < materials.Length; sub++)
                    {
                        Graphics.DrawMesh(mesh, matrix, materials[sub], r.gameObject.layer, null, sub);
                    }
                }
            }
        }
    }

    private class WrappedObject
    {
        public GameObject obj;
        public Rigidbody rb;
        public MeshRenderer[] renderers;
    }
}