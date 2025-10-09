using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class TopDownPlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Camera cam;

    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (cam == null) cam = Camera.main;
    }

    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        Vector3 forward = cam.transform.forward;
        Vector3 right = cam.transform.right;

        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDir = forward * moveZ + right * moveX;

        // 移动
        controller.SimpleMove(moveDir * moveSpeed);

        // 朝向（如果有输入）
        // if (moveDir != Vector3.zero)
        // {
        //     transform.forward = moveDir;
        // }
    }
}
