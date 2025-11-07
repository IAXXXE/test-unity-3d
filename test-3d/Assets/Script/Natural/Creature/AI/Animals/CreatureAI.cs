// 生物AI控制器
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
public class CreatureAI : MonoBehaviour
{
    [Header("组件引用")]
    public CreatureStat stat;
    public NavMeshAgent agent;
    public CreatureAnimation anim;

    [Header("AI配置")]
    public float wanderRadius = 20f;
    public float detectionRange = 120f;
    public float attackRange = 2f;
    public float eatRange = 2f;
    public float drinkRange = 1.5f;

    [Header("战斗配置")]
    public float fleeDistance = 20f;        // 逃跑距离
    public float combatCheckInterval = 0.5f; // 战斗检测间隔
    public int attackDamage = 10;            // 攻击伤害
    
    [Header("需求阈值")]
    public int hungerThreshold;  // 低于此值开始寻找食物
    public int thirstThreshold;  // 低于此值开始寻找水源
    
    public CreatureActionState currentActionState;
    public BehaviorTree behaviorTree;
    private Vector3[] waterSources;
    public Transform currentTarget;
    private GameObject currentThreat;  // 当前威胁目标

    public bool isStateLocked = false;  // 添加状态锁
    public float stateLockTimer = 0f;

    public void Init(CreatureStat stat)
    {
        this.stat = stat;

        anim = GetComponent<CreatureAnimation>();

        hungerThreshold = stat.GetMaxSatiety() / 3;
        thirstThreshold = stat.GetMaxThirsty() / 3;

        agent = transform.GetComponent<NavMeshAgent>();
        agent.speed = stat.GetSpeed();
        agent.radius = transform.GetComponent<CharacterController>().radius;
        agent.height = transform.GetComponent<CharacterController>().height;

        var raceData = CharacterDatabase.Instance.GetRaceData(stat.data.raceType);
        // 根据Race配置
        wanderRadius = raceData.wanderRadius;
        detectionRange = raceData.detectionRange;
        attackRange = raceData.attackRange;
        eatRange = raceData.eatRange;
        drinkRange = raceData.drinkRange;

        fleeDistance = raceData.fleeDistance;
        combatCheckInterval = raceData.combatCheckInterval;
        attackDamage = raceData.attackDamage;
        hungerThreshold = raceData.hungerThreshold;
        thirstThreshold = raceData.thirstThreshold;
    }
    
    private void Start()
    {
        // 初始化水源信息
        waterSources = FindWaterSources();
        
        // 构建行为树
        behaviorTree = BuildBehaviorTree();
        
        // 初始状态
        ChangeState(new AnimalWanderState(this));
    }

    private void Destroy()
    {

    }
    
    private BehaviorTree BuildBehaviorTree()
    {
        return new BehaviorTree(
            new SelectorNode(
                // 优先级0: 情绪反应（最高优先级）
                new SequenceNode(
                    new ConditionNode(() => stat.GetEmotionState() != EmotionState.Calm &&
                                           !(currentActionState is FleeState) &&
                                           !(currentActionState is CombatState)),
                    new ActionNode(() => {
                        EmotionState emotion = stat.GetEmotionState();
                        
                        if (emotion == EmotionState.Afraid)
                        {
                            // 逃跑
                            ChangeState(new FleeState(this, currentThreat));
                        }
                        else if (emotion == EmotionState.Angry)
                        {
                            // 反击
                            if (currentThreat != null)
                                ChangeState(new CombatState(this, currentThreat));
                        }
                        
                        return NodeState.SUCCESS;
                    })
                ),

                // 优先级1: 生存危机
                new SequenceNode(
                    new ConditionNode(() => stat.GetSatiety() == 0 || stat.GetThirsty() == 0),
                    new ActionNode(() => {
                        // 进入危机模式
                        if (stat.GetSatiety() == 0)
                        {
                            if(stat.data.isCarnivore) ChangeState(new AnimalSeekPreyState(this));
                            else ChangeState(new AnimalSeekFoodState(this));
                        }
                        else if (stat.GetThirsty() == 0) 
                            ChangeState(new AnimalSeekWaterState(this));
                        return NodeState.SUCCESS;
                    })
                ),
                
                // 优先级2: 饥饿
                new SequenceNode(
                    new ConditionNode(() => stat.GetSatiety() < hungerThreshold),
                    new ActionNode(() => {
                        if(stat.data.isCarnivore) ChangeState(new AnimalSeekPreyState(this));
                        else ChangeState(new AnimalSeekFoodState(this));
                        return NodeState.SUCCESS;
                    })
                ),

                // 优先级3: 口渴
                new SequenceNode(
                    new ConditionNode(() => stat.GetThirsty() < thirstThreshold),
                    new ActionNode(() => {
                        ChangeState(new AnimalSeekWaterState(this));
                        return NodeState.SUCCESS;
                    })
                ),
                
                // 优先级4: 休息恢复
                new SequenceNode(
                    new ConditionNode(() => stat.GetHealth() < stat.GetMaxHealth() * 0.8f),
                    new ActionNode(() => {
                        ChangeState(new AnimalRestState(this));
                        return NodeState.SUCCESS;
                    })
                ),
                
                // 默认: 游走
                new ActionNode(() => {
                    if (!(currentActionState is AnimalWanderState))
                        ChangeState(new AnimalWanderState(this));
                    return NodeState.SUCCESS;
                })
            )
        );
    }

    public void LockState(float duration = 10000)
    {
        isStateLocked = true;
        stateLockTimer = duration;
    }

    public void UnlockState()
    {
        isStateLocked = false;
    }
    
    public void ChangeState(CreatureActionState newState)
    {
        if(isStateLocked) return;

        // 战斗和逃跑状态可以互相切换
        if ((currentActionState is FleeState || currentActionState is CombatState) &&
            !(newState is FleeState || newState is CombatState))
        {
            // 只有在情绪平复后才能切换到其他状态
            if (stat.GetEmotionState() != EmotionState.Calm)
                return;
        }

        StopAllCoroutines();

        currentActionState?.Exit();
        currentActionState = newState;
        currentActionState?.Enter();

        Debug.Log($"{stat.data.name} change state {newState}");
    }

    public void ForcedChangrState(CreatureActionState newState)
    {
        StopAllCoroutines();

        currentActionState?.Exit();
        currentActionState = newState;
        currentActionState?.Enter();

        Debug.Log($"Force {stat.data.name} change state {newState}");
    }

    // 受到伤害时的回调
    public void OnDamageReceived(float damage, GameObject attacker)
    {
        currentThreat = attacker;
        
        // 根据性格和情绪立即做出反应
        EmotionState emotion = stat.GetEmotionState();
        
        // 强制切换状态（忽略锁定）
        isStateLocked = false;
        
        if (emotion == EmotionState.Afraid)
        {
            ChangeState(new FleeState(this, attacker));
        }
        else if (emotion == EmotionState.Angry)
        {
            ChangeState(new CombatState(this, attacker));
        }
    }
    
    public void UpdateSurvivalStats()
    {
        // 每秒消耗
        stat.LoseSatiety(1);
        stat.LoseThirsty(1);
        
        // 生存危机扣血
        if (stat.GetSatiety() == 0)
            stat.LoseHealth(1);
        if (stat.GetThirsty() == 0)
            stat.LoseHealth(1);
        
        // 状态良好时恢复
        if (stat.GetSatiety() > stat.GetMaxSatiety() / 2 && stat.GetThirsty() > stat.GetMaxThirsty() / 2)
            stat.Heal(1);
    }
    
    public Vector3 GetNearestWaterSource()
    {
        Vector3 nearest = waterSources[0];
        float minDist = Vector3.Distance(transform.position, nearest);
        
        foreach (var water in waterSources)
        {
            float dist = Vector3.Distance(transform.position, water);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = water;
            }
        }
        return nearest;
    }
    
    public Transform FindNearestPrey()
    {
        // 使用Physics.OverlapSphere检测范围内的猎物
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRange);
        Transform nearest = null;
        float minDist = Mathf.Infinity;
        
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Prey"))
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = hit.transform;
                }
            }
        }

        currentTarget = nearest;
        return nearest;
    }
    
    private Vector3[] FindWaterSources()
    {
        GameObject[] waters = GameObject.FindGameObjectsWithTag("Water");
        Vector3[] positions = new Vector3[waters.Length];
        for (int i = 0; i < waters.Length; i++)
            positions[i] = waters[i].transform.position;
        return positions;
    }

    public Transform FindNearestFood()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRange);
        Transform nearest = null;
        float minDist = Mathf.Infinity;
        
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Food"))
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = hit.transform;
                }
            }
        }
        currentTarget = nearest;

        return nearest;
    }
}

