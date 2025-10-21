using UnityEngine;
using Cinemachine;

[ExecuteAlways]
[SaveDuringPlay]
[AddComponentMenu("Cinemachine/Extensions/Simple Cinemachine Collider")]
public class SimpleCinemachineCollider : CinemachineExtension
{
    public LayerMask collisionMask = ~0;
    public float cameraRadius = 0.3f;
    public float minDistance = 0.5f;
    public float returnSmooth = 5f;

    private float currentDistance;

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage,
        ref CameraState state,
        float deltaTime)
    {
        if (stage != CinemachineCore.Stage.Body)
            return;

        var camPos = state.RawPosition;
        var targetPos = state.ReferenceLookAt;
        Vector3 dir = camPos - targetPos;
        float desiredDist = dir.magnitude;

        if (Physics.SphereCast(targetPos, cameraRadius, dir.normalized, out RaycastHit hit, desiredDist, collisionMask))
        {
            currentDistance = Mathf.Max(hit.distance - 0.1f, minDistance);
        }
        else
        {
            currentDistance = Mathf.Lerp(currentDistance, desiredDist, deltaTime * returnSmooth);
        }

        state.RawPosition = targetPos + dir.normalized * currentDistance;
    }
}
