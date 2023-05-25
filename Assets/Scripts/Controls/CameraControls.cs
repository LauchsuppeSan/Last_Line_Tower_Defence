using System.Collections;
using System.Collections.Generic;
using TGS;
using UnityEngine;

public class CameraControls : MonoBehaviour
{
    [Header("Control Settings")]
    [SerializeField]
    bool noMovementClamp = false;
    [SerializeField]
    float minPositionX;
    [SerializeField]
    float maxPositionX;
    [SerializeField]
    float minPositionZ;
    [SerializeField]
    float maxPositionZ;
    [SerializeField]
    float minZoomDistance;
    [SerializeField]
    float maxZoomDistance;
    [SerializeField, Range(0, 50)]
    int screenBorderThicknessPx;
    [SerializeField, Range(0.1f, 100.0f)]
    float scrollSpeed;
    [SerializeField, Range(1.5f, 5.0f)]
    float scrollSpeedMultiplier;
    [SerializeField, Range(0.1f, 10.0f)]
    float rotationSpeed;
    [SerializeField, Range(1000.0f, 10000.0f)]
    float zoomSpeed;
    [SerializeField]
    bool invertHorizontalRotation = false;
    [SerializeField]
    bool invertVerticalRotation = false;

    [Header("Grid Graphics")]
    [SerializeField]
    TerrainGridSystem tgs;
    [SerializeField]
    bool circularFadingActiv = true;
    [SerializeField]
    float circularFadeRadius;
    [SerializeField]
    float fadingStrength;

    [Header("Camera Collision Settings")]
    [SerializeField]
    LayerMask collisionLayer;
    [SerializeField, Range(0, 2), Tooltip("Has a major impact on performance. Usually 1 is enough")]
    int accuracyMultiplier = 1;
    [SerializeField]
    bool showEmptyDistanceBehindCamera = false;
    [SerializeField, Range(0.001f, 2f)]
    float emptyDistanceBehindCamera;
    [SerializeField, Range(0.5f, 10f)]
    float backwardScrollSpeed = 7;

    private float horizontalRota;
    private float verticalRota;
    private float currentMaxZoomDist;
    private new Camera camera;

    public int ScreenBorderThicknessPx => screenBorderThicknessPx;

    public enum MovementDirections
    {
        Forward,
        Backward,
        Leftward,
        Rightward
    }

    // Start is called before the first frame update
    void Start()
    {
        try { camera = transform.GetComponentInChildren<Camera>(); }
        catch { camera = null; }

        SetCircularGridFade();
        SetCameraStartValues();
    }

    private void SetCircularGridFade()
    {
        tgs.circularFadeTarget = transform;
        tgs.circularFadeDistance = circularFadeRadius;
        tgs.circularFadeFallOff = fadingStrength;
        tgs.circularFadeEnabled = circularFadingActiv;
    }

    private void SetCameraStartValues()
    {
        currentMaxZoomDist = maxZoomDistance;
        horizontalRota = 0.0f;
        verticalRota = -45.0f;
        transform.localEulerAngles = new Vector3(verticalRota, horizontalRota, 0.0f);
        camera.transform.localPosition = Vector3.up * maxZoomDistance;
    }

    private void FixedUpdate()
    {
        CameraCollisionHandling();
        Rotate();

        List<MovementDirections> cameraScrollDirectionsForThisFrame = new List<MovementDirections>();

        if (Input.GetKey(KeyCode.W))
        {
            cameraScrollDirectionsForThisFrame.Add(MovementDirections.Forward);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            cameraScrollDirectionsForThisFrame.Add(MovementDirections.Backward);
        }

        if (Input.GetKey(KeyCode.A))
        {
            cameraScrollDirectionsForThisFrame.Add(MovementDirections.Leftward);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            cameraScrollDirectionsForThisFrame.Add(MovementDirections.Rightward);
        }

        if (cameraScrollDirectionsForThisFrame.Count > 0)
        {
            Move(cameraScrollDirectionsForThisFrame.ToArray());
        }

    }

    private void Update()
    {
        Zoom(Input.GetAxis("Mouse ScrollWheel"));
    }

    private void Move(MovementDirections[] movementDirectionsSequenz)
    {
        Vector3 movementDirN = Vector3.zero;
        Vector3 cameraGroundCrawlerPosition = transform.position;

        for (int sequenzIndex = 0; sequenzIndex < movementDirectionsSequenz.Length; sequenzIndex++)
        {
            switch (movementDirectionsSequenz[sequenzIndex])
            {
                case MovementDirections.Forward:
                    movementDirN += MoveForwardVector();
                    break;
                case MovementDirections.Backward:
                    movementDirN += MoveBackwardVector();
                    break;
                case MovementDirections.Leftward:
                    movementDirN += MoveLeftwardVector();
                    break;
                case MovementDirections.Rightward:
                    movementDirN += MoveRightwardVector();
                    break;
            }
        }

        // Include speed, multiplier und framerate
        cameraGroundCrawlerPosition.x += movementDirN.x * scrollSpeed * GetScrollSpeedMultiplier() * Time.deltaTime;
        cameraGroundCrawlerPosition.z += movementDirN.z * scrollSpeed * GetScrollSpeedMultiplier() * Time.deltaTime;

        if (!noMovementClamp)
        {
            // Clamp camera  on map borders
            cameraGroundCrawlerPosition.x = Mathf.Clamp(cameraGroundCrawlerPosition.x, minPositionX, maxPositionX);
            cameraGroundCrawlerPosition.z = Mathf.Clamp(cameraGroundCrawlerPosition.z, minPositionZ, maxPositionZ);
        }

        RaycastHit hit;
        if (Physics.Linecast(transform.position, Utils.CrawlOverY(transform.position, Vector3.up, -10000), out hit))
        {
            cameraGroundCrawlerPosition.y = hit.point.y + 3;
        }

        // Set new position
        transform.position = cameraGroundCrawlerPosition;
    }

    private void Rotate()
    {
        if (Input.GetKey(KeyCode.Mouse1))
        {
            float mouseHorizontal = Input.GetAxis("Mouse X") * rotationSpeed * Time.timeScale;
            float mouseVertical = Input.GetAxis("Mouse Y") * rotationSpeed * Time.timeScale;

            if (invertHorizontalRotation)
            { mouseHorizontal *= (-1.0f); }

            if (invertVerticalRotation)
            { mouseVertical *= (-1.0f); }

            horizontalRota += mouseHorizontal;
            verticalRota = Mathf.Clamp(verticalRota + mouseVertical, (-85.0f), 0.0f);

            transform.localEulerAngles = new Vector3(verticalRota, horizontalRota, 0);
        }
    }

    public void Zoom(float zoomInput)
    {
        camera.transform.Translate(camera.transform.forward * zoomInput * zoomSpeed * Time.deltaTime, Space.World);
        Vector3 newLocalCameraPosition = camera.transform.localPosition;
        newLocalCameraPosition.y = Mathf.Clamp(newLocalCameraPosition.y, minZoomDistance, maxZoomDistance);
        camera.transform.localPosition = newLocalCameraPosition;
        currentMaxZoomDist = newLocalCameraPosition.y;
    }

    private float GetScrollSpeedMultiplier()
    {
        return
            Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)
            ? scrollSpeedMultiplier
            : 1;
    }

    private Vector3 MoveForwardVector()
    {
        return Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
    }

    private Vector3 MoveLeftwardVector()
    {
        return Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized * (-1.0f);
    }

    private Vector3 MoveBackwardVector()
    {
        return Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized * (-1.0f);
    }

    private Vector3 MoveRightwardVector()
    {
        return Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;
    }

    private void CameraCollisionHandling()
    {
        (Vector3[] corners, List<Vector3> RayTargetPoints) nearClipPlaneInfos = GetNearClipPlaneMidpointAndRayPoints();
        CameraCollisionResult collisionCheckResult = CheckForCameraViewCollision(nearClipPlaneInfos.RayTargetPoints);
        bool collisionBehind = CheckForCameraBackVolision(nearClipPlaneInfos.corners);

        // Midpoint of diagonal between topLeft and bottomRight corner
        Vector3 currentNearClipPlaneCenter = Utils.GetMidpoint(nearClipPlaneInfos.corners[0], nearClipPlaneInfos.corners[3]);

        if (collisionCheckResult.IsColliding)
        {
            Vector3 offsetNearClipPlaneMidN = (currentNearClipPlaneCenter - collisionCheckResult.clipPlanePoint).normalized;
            float offsetOnPlaneDist = Vector3.Distance(currentNearClipPlaneCenter, collisionCheckResult.clipPlanePoint);
            Vector3 newCameraPosition = collisionCheckResult.HitPoint + (offsetNearClipPlaneMidN * offsetOnPlaneDist);
            camera.transform.position = newCameraPosition;
        }
        else if (!collisionBehind && camera.transform.localPosition.y < currentMaxZoomDist)
        {
            camera.transform.position -= camera.transform.forward * backwardScrollSpeed;

            nearClipPlaneInfos = GetNearClipPlaneMidpointAndRayPoints();
            collisionCheckResult = CheckForCameraViewCollision(nearClipPlaneInfos.RayTargetPoints);

            // Midpoint of diagonal between topLeft and bottomRight corner
            currentNearClipPlaneCenter = Utils.GetMidpoint(nearClipPlaneInfos.corners[0], nearClipPlaneInfos.corners[3]);

            if (collisionCheckResult.IsColliding)
            {
                Vector3 offsetNearClipPlaneMidN = (currentNearClipPlaneCenter - collisionCheckResult.clipPlanePoint).normalized;
                float offsetOnPlaneDist = Vector3.Distance(currentNearClipPlaneCenter, collisionCheckResult.clipPlanePoint);
                Vector3 newCameraPosition = collisionCheckResult.HitPoint + (offsetNearClipPlaneMidN * offsetOnPlaneDist);
                camera.transform.position = newCameraPosition;
            }
        }
    }

    private (Vector3[], List<Vector3>) GetNearClipPlaneMidpointAndRayPoints()
    {
        float distCameraToNearClipPlane = camera.nearClipPlane;
        float distNearClipPlaneCenterToLeftAndRightPlaneEdge = Mathf.Tan(camera.fieldOfView / 3.41f) * distCameraToNearClipPlane;
        float distNearClipPlaneCenterToTopAndBottomPLaneEdge = distNearClipPlaneCenterToLeftAndRightPlaneEdge / camera.aspect;

        Vector3 upLeftCorner = (camera.transform.rotation * new Vector3(-distNearClipPlaneCenterToLeftAndRightPlaneEdge, -distNearClipPlaneCenterToTopAndBottomPLaneEdge, distCameraToNearClipPlane)) + camera.transform.position;
        Vector3 upRightCorner = (camera.transform.rotation * new Vector3(distNearClipPlaneCenterToLeftAndRightPlaneEdge, -distNearClipPlaneCenterToTopAndBottomPLaneEdge, distCameraToNearClipPlane)) + camera.transform.position;
        Vector3 downLeftCorner = (camera.transform.rotation * new Vector3(-distNearClipPlaneCenterToLeftAndRightPlaneEdge, distNearClipPlaneCenterToTopAndBottomPLaneEdge, distCameraToNearClipPlane)) + camera.transform.position;
        Vector3 downRightCorner = (camera.transform.rotation * new Vector3(distNearClipPlaneCenterToLeftAndRightPlaneEdge, distNearClipPlaneCenterToTopAndBottomPLaneEdge, distCameraToNearClipPlane)) + camera.transform.position;

        List<Vector3> nearClipPlaneRayPoints = new List<Vector3>() { upLeftCorner, upRightCorner, downLeftCorner, downRightCorner };

        for (int x = 0; x < accuracyMultiplier; x++)
        {
            List<List<Vector3>> pointConnections = Utils.GetAllCombinations(nearClipPlaneRayPoints, 2);
            for (int i = 0; i < pointConnections.Count; i++)
            {
                Vector3 newPoint = Utils.GetMidpoint(pointConnections[i][0], pointConnections[i][1]);
                nearClipPlaneRayPoints.Add(newPoint);
            }
        }

        Vector3[] corners =
            new Vector3[4]
            {
                upLeftCorner,
                upRightCorner,
                downLeftCorner,
                downRightCorner
            };

        return (corners, nearClipPlaneRayPoints);
    }

    private CameraCollisionResult CheckForCameraViewCollision(List<Vector3> nearClipPlaneRayPoints)
    {
        bool collisionFound = false;

        Vector3 clipPlaneRaypointWithCollision = new Vector3(float.NaN, float.NaN, float.NaN);
        Vector3 collisionPoint = new Vector3(float.NaN, float.NaN, float.NaN);
        Vector3 crawlerPos = transform.position;

        float shortetsDist = int.MaxValue;
        float backwardsDist = currentMaxZoomDist - camera.transform.localPosition.y;
        bool isFrontCollision = true;

        for (int rayPointIndex = 0; rayPointIndex < nearClipPlaneRayPoints.Count; rayPointIndex++)
        {
            RaycastHit hit;

            if (Physics.Linecast(crawlerPos, nearClipPlaneRayPoints[rayPointIndex], out hit, collisionLayer))
            {
                if (hit.distance < shortetsDist)
                {
                    shortetsDist = hit.distance;
                    clipPlaneRaypointWithCollision = nearClipPlaneRayPoints[rayPointIndex];
                    collisionPoint = hit.point;
                    collisionFound = true;
                }
            }
        }

        return new CameraCollisionResult(collisionFound, shortetsDist, clipPlaneRaypointWithCollision, collisionPoint);
    }

    private bool CheckForCameraBackVolision(Vector3[] corners)
    {
        // Midpoint of diagonal between topLeft and bottomRight corner
        Vector3 currentNearClipPlaneCenter = Utils.GetMidpoint(corners[0], corners[3]);
        RaycastHit hit;

        // Creates a box made of Rays
        (Vector3 start, Vector3 end) rayFrontHorizontalUp = (corners[0], corners[1]);
        (Vector3 start, Vector3 end) rayFrontHorizontalDown = (corners[2], corners[3]);
        (Vector3 start, Vector3 end) rayFrontVerticalLeft = (corners[0], corners[2]);
        (Vector3 start, Vector3 end) rayFrontVerticalRight = (corners[1], corners[3]);

        (Vector3 start, Vector3 end) rayThicknessTopLeft = (corners[0], corners[0] + camera.transform.forward * (-1) * emptyDistanceBehindCamera);
        (Vector3 start, Vector3 end) rayThicknessTopRight = (corners[1], corners[1] + camera.transform.forward * (-1) * emptyDistanceBehindCamera);
        (Vector3 start, Vector3 end) rayThicknessDownLeft = (corners[2], corners[2] + camera.transform.forward * (-1) * emptyDistanceBehindCamera);
        (Vector3 start, Vector3 end) rayThicknessDownRight = (corners[3], corners[3] + camera.transform.forward * (-1) * emptyDistanceBehindCamera);

        (Vector3 start, Vector3 end) rayBackHorizontalUp = (corners[0] + camera.transform.forward * (-1) * emptyDistanceBehindCamera, corners[1] + camera.transform.forward * (-1) * emptyDistanceBehindCamera);
        (Vector3 start, Vector3 end) rayBackHorizontalDown = (corners[2] + camera.transform.forward * (-1) * emptyDistanceBehindCamera, corners[3] + camera.transform.forward * (-1) * emptyDistanceBehindCamera);
        (Vector3 start, Vector3 end) rayBackVerticalLeft = (corners[0] + camera.transform.forward * (-1) * emptyDistanceBehindCamera, corners[2] + camera.transform.forward * (-1) * emptyDistanceBehindCamera);
        (Vector3 start, Vector3 end) rayBackVerticalRight = (corners[1] + camera.transform.forward * (-1) * emptyDistanceBehindCamera, corners[3] + camera.transform.forward * (-1) * emptyDistanceBehindCamera);

        if (showEmptyDistanceBehindCamera)
        {
            Debug.DrawLine(rayFrontHorizontalUp.start, rayFrontHorizontalUp.end, Color.red);
            Debug.DrawLine(rayFrontHorizontalDown.start, rayFrontHorizontalDown.end, Color.red);
            Debug.DrawLine(rayFrontVerticalLeft.start, rayFrontVerticalLeft.end, Color.red);
            Debug.DrawLine(rayFrontVerticalRight.start, rayFrontVerticalRight.end, Color.red);

            Debug.DrawLine(rayThicknessTopLeft.start, rayThicknessTopLeft.end, Color.red);
            Debug.DrawLine(rayThicknessTopRight.start, rayThicknessTopRight.end, Color.red);
            Debug.DrawLine(rayThicknessDownLeft.start, rayThicknessDownLeft.end, Color.red);
            Debug.DrawLine(rayThicknessDownRight.start, rayThicknessDownRight.end, Color.red);

            Debug.DrawLine(rayBackHorizontalUp.start, rayBackHorizontalUp.end, Color.red);
            Debug.DrawLine(rayBackHorizontalDown.start, rayBackHorizontalDown.end, Color.red);
            Debug.DrawLine(rayBackVerticalLeft.start, rayBackVerticalLeft.end, Color.red);
            Debug.DrawLine(rayBackVerticalRight.start, rayBackVerticalRight.end, Color.red);
        }

        return
            Physics.Linecast(rayFrontHorizontalUp.start, rayFrontHorizontalUp.end)
            || Physics.Linecast(rayFrontHorizontalDown.start, rayFrontHorizontalDown.end)
            || Physics.Linecast(rayFrontVerticalLeft.start, rayFrontVerticalLeft.end)
            || Physics.Linecast(rayFrontVerticalRight.start, rayFrontVerticalRight.end)
            || Physics.Linecast(rayThicknessTopLeft.start, rayThicknessTopLeft.end)
            || Physics.Linecast(rayThicknessTopRight.start, rayThicknessTopRight.end)
            || Physics.Linecast(rayThicknessDownLeft.start, rayThicknessDownLeft.end)
            || Physics.Linecast(rayThicknessDownRight.start, rayThicknessDownRight.end)
            || Physics.Linecast(rayBackHorizontalUp.start, rayBackHorizontalUp.end)
            || Physics.Linecast(rayBackHorizontalDown.start, rayBackHorizontalDown.end)
            || Physics.Linecast(rayBackVerticalLeft.start, rayBackVerticalLeft.end)
            || Physics.Linecast(rayBackVerticalRight.start, rayBackVerticalRight.end);
    }


    private struct CameraCollisionResult
    {
        private bool detected;
        float dist;
        Vector3 correspondingClipPlanePoint;
        Vector3 collisionPoint;

        public CameraCollisionResult(bool collisionDetected, float targetToCollisionPointDist, Vector3 correspondingClipPlanePointCoords, Vector3 collisionPointCoords)
        {
            detected = collisionDetected;
            dist = targetToCollisionPointDist;
            correspondingClipPlanePoint = correspondingClipPlanePointCoords;
            collisionPoint = collisionPointCoords;
        }

        public bool IsColliding => detected;
        public float TargetToCollidingPointDist => dist;
        public Vector3 clipPlanePoint => correspondingClipPlanePoint;
        public Vector3 HitPoint => collisionPoint;
    }
}
