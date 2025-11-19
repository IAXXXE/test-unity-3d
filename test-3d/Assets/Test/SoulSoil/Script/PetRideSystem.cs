using UnityEngine;
//## 11. **扩展：骑乘系统**

public class PetRideSystem : MonoBehaviour
{
    [Header("骑乘设置")]
    public Transform ridePosition;  // 骑乘位置
    public float rideSpeed = 8f;
    
    private Transform player;
    private CharacterController playerController;
    private PetBase mountedPet;
    private bool isRiding = false;
    
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerController = player.GetComponent<CharacterController>();
    }
    
    // 骑乘宠物
    public void Mount(PetBase pet)
    {
        if (pet == null || isRiding) return;
        
        // 检查宠物是否可骑乘
        if (pet is CreaturePet creaturePet && creaturePet.canRide)
        {
            isRiding = true;
            mountedPet = pet;
            
            // 停止宠物跟随
            pet.StopFollow();
            
            // 将玩家移动到骑乘位置
            if (ridePosition != null)
            {
                player.SetParent(ridePosition);
                player.localPosition = Vector3.zero;
                player.localRotation = Quaternion.identity;
            }
            
            // 禁用玩家控制器
            if (playerController != null)
                playerController.enabled = false;
            
            // 修改宠物移动速度
            if (pet.agent != null)
                pet.agent.speed = rideSpeed;
            
            Debug.Log($"骑上了 {pet.GetPetName()}");
        }
    }
    
    // 下马
    public void Dismount()
    {
        if (!isRiding || mountedPet == null) return;
        
        // 恢复玩家位置
        player.SetParent(null);
        
        // 启用玩家控制器
        if (playerController != null)
            playerController.enabled = true;
        
        // 恢复宠物速度并继续跟随
        if (mountedPet.agent != null)
            mountedPet.agent.speed = 3.5f;
        
        mountedPet.Follow(player);
        
        Debug.Log($"下马: {mountedPet.GetPetName()}");
        
        isRiding = false;
        mountedPet = null;
    }
    
    private void Update()
    {
        if (isRiding)
        {
            HandleRideMovement();
            
            // 按空格下马
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Dismount();
            }
        }
    }
    
    private void HandleRideMovement()
    {
        if (mountedPet == null || mountedPet.agent == null) return;
        
        // 获取输入
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        if (horizontal != 0 || vertical != 0)
        {
            // 计算移动方向
            Vector3 direction = new Vector3(horizontal, 0, vertical).normalized;
            
            // 相对于摄像机的方向
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                Vector3 forward = mainCamera.transform.forward;
                Vector3 right = mainCamera.transform.right;
                forward.y = 0;
                right.y = 0;
                forward.Normalize();
                right.Normalize();
                
                direction = forward * vertical + right * horizontal;
            }
            
            // 移动宠物
            Vector3 targetPosition = mountedPet.transform.position + direction * rideSpeed * Time.deltaTime;
            mountedPet.agent.SetDestination(targetPosition);
            
            // 旋转宠物朝向移动方向
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                mountedPet.transform.rotation = Quaternion.Slerp(
                    mountedPet.transform.rotation,
                    targetRotation,
                    Time.deltaTime * 10f
                );
            }
        }
    }
}