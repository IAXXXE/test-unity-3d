using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public enum CreatureActionStateType
{
    Wander,
    Rest,
    SeekFood,
    Attack,
    Escape,
    Eat,
    SeekWater,
    Drink,
    Sleep,
    Flee,
    Combat,
}

// 状态基类
public abstract class CreatureActionState
{
    public CreatureActionStateType type;
    protected CreatureAI ai;
    
    public CreatureActionState(CreatureAI ai)
    {
        this.ai = ai;
    }
    
    public virtual void Enter() { }
    public virtual IEnumerator Execute() { yield break; }
    public virtual void Exit() { }
}

// 游走状态
public class AnimalWanderState : CreatureActionState
{
    private float breakTime;
    private float wanderTimer;
    
    public AnimalWanderState(CreatureAI ai) : base(ai) 
    {
        type = CreatureActionStateType.Wander;
    }
    
    public override void Enter()
    {
        SetNewWanderDestination();
        ai.anim.SetAnimationSmooth(AnimState.Walk, 0.15f, EaseType.EaseOutQuad);
    }
    
    public override IEnumerator Execute()
    {
        while(ai.agent.remainingDistance > 0.5f || ai.agent.pathPending)
        {
            yield return new WaitForSeconds(1f);
        }
        ai.behaviorTree.Evaluate();

        if (Random.value > 0.3f)
        {
            breakTime = Random.Range(5, 12);
            ai.StartCoroutine(TakeBreak());

        }
        else
        {
            SetNewWanderDestination();
        }
    }

    public IEnumerator TakeBreak()
    {
        ai.agent.ResetPath();
        ai.anim.SetAnimationSmooth(AnimState.Idle, 0.3f, EaseType.EaseOutCubic);

        while(breakTime > 0)
        {
            breakTime -= 1f;
            yield return new WaitForSeconds(1f);
        }
        SetNewWanderDestination();
        
    }
    
    private void SetNewWanderDestination()
    {
        Vector3 randomDir = Random.insideUnitSphere * ai.wanderRadius;
        randomDir += ai.transform.position;
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDir, out hit, ai.wanderRadius, NavMesh.AllAreas))
        {
            ai.agent.SetDestination(hit.position);
        }
        ai.anim.SetAnimationSmooth(AnimState.Walk, 0.15f, EaseType.EaseOutQuad);
        ai.StartCoroutine(Execute());
    }
}

// 寻找水源状态
public class AnimalSeekWaterState : CreatureActionState
{
    private Vector3 waterPosition;
    
    public AnimalSeekWaterState(CreatureAI ai) : base(ai) 
    {
        type = CreatureActionStateType.SeekWater;
    }
    
    public override void Enter()
    {
        waterPosition = ai.GetNearestWaterSource();
        ai.agent.SetDestination(waterPosition);
        ai.agent.speed = 5f; // 加速移动

        ai.anim.SetAnimationSmooth(AnimState.Run, 0.1f, EaseType.EaseInQuad);

        ai.StartCoroutine(Execute());
    }
    
    public override IEnumerator Execute()
    {
        while(true)
        {
            yield return new WaitForSeconds(1f);
            float dist = Vector3.Distance(ai.transform.position, waterPosition);
            if (dist < ai.drinkRange)
            {
                ai.ChangeState(new AnimalDrinkState(ai));
                yield break;
            }

        }
    }
    
    public override void Exit()
    {
        ai.agent.speed = 3.5f; // 恢复正常速度
    }
}

// 饮水状态
public class AnimalDrinkState : CreatureActionState
{
    private float drinkTimer;
    
    public AnimalDrinkState(CreatureAI ai) : base(ai) 
    {
        type = CreatureActionStateType.Drink;
    }
    
    public override void Enter()
    {
        ai.agent.ResetPath();
        drinkTimer = (ai.stat.GetMaxThirsty() - ai.stat.GetThirsty()) / 3;

        ai.LockState();
        ai.anim.SetAnimationImmediate(AnimState.Idle);

        ai.StartCoroutine(ExecuteAction());
    }
    
    public IEnumerator ExecuteAction()
    {
        while(drinkTimer > 0)
        {
            drinkTimer -= 1;
            yield return new WaitForSeconds(1f);
            ai.stat.IncreaseThirsty(3);
            Debug.Log("Thirsty Increase to " + ai.stat.GetThirsty());
        }
        ai.UnlockState();
        ai.ChangeState(new AnimalWanderState(ai));
    }
}

// 寻找食物状态
public class AnimalSeekFoodState : CreatureActionState
{
    private Transform food;
    
    public AnimalSeekFoodState(CreatureAI ai) : base(ai) 
    { 
        type = CreatureActionStateType.SeekFood;
    }
    
    public override void Enter()
    {
        food = ai.FindNearestFood();
        ai.anim.SetAnimationSmooth(AnimState.Walk, 0.15f, EaseType.EaseOutQuad);
        if (food != null)
        {
            ai.agent.speed = 6f;
            ai.anim.SetAnimationSmooth(AnimState.Run, 0.1f, EaseType.EaseInQuad);
        }

        ai.StartCoroutine(Execute());
    }
    
    public override IEnumerator Execute()
    {
        if (food == null)
        {
            food = ai.FindNearestFood();
            if (food == null)
            {
                ai.ChangeState(new AnimalWanderState(ai));
                yield break;
            }
            yield return null;
        }
        
        ai.agent.SetDestination(food.position);

        float dist = Vector3.Distance(ai.transform.position, food.position);
        while(dist > ai.eatRange)
        {
            yield return new WaitForSeconds(0.5f);
            if(food == null)
            {
                food = ai.FindNearestFood();
                if (food == null)
                {
                    ai.ChangeState(new AnimalWanderState(ai));
                    yield break;
                }
            }
            dist = Vector3.Distance(ai.transform.position, food.position);
        }
        ai.ChangeState(new AnimalEatState(ai));
    }
    
    public override void Exit()
    {
        ai.agent.speed = 3.5f;
    }
}

// 饮食状态
public class AnimalEatState : CreatureActionState
{
    private float eatTimer;
    
    public AnimalEatState(CreatureAI ai) : base(ai) 
    { 
        type = CreatureActionStateType.Eat;
    }
    
    public override void Enter()
    {
        ai.agent.ResetPath();
        eatTimer = (ai.stat.GetMaxSatiety() - ai.stat.GetSatiety()) / 3;

        ai.LockState();
        ai.anim.SetAnimationImmediate(AnimState.Idle);

        ai.StartCoroutine(Execute());
    }
    
    public override IEnumerator Execute()
    {
        while(eatTimer > 0)
        {
            eatTimer -= 1f;
            yield return new WaitForSeconds(1f);
            ai.stat.IncreaseSatiety(3);

            if (ai.stat.GetSatiety() >= ai.stat.GetMaxSatiety()) break;
            
        }
        ai.UnlockState();
        ai.ChangeState(new AnimalWanderState(ai));
    }
}

// 寻找猎物状态
public class AnimalSeekPreyState : CreatureActionState
{
    private Transform prey;
    
    public AnimalSeekPreyState(CreatureAI ai) : base(ai) 
    { 
        type = CreatureActionStateType.SeekFood;
    }
    
    public override void Enter()
    {
        prey = ai.FindNearestPrey();
        if (prey != null)
        {
            ai.agent.speed = 6f;
        }

        ai.StartCoroutine(Execute());
    }
    
    public override IEnumerator Execute()
    {
        if (prey == null)
        {
            prey = ai.FindNearestPrey();
            if (prey == null)
            {
                ai.ChangeState(new AnimalWanderState(ai));
                yield break;
            }
        }
        
        ai.agent.SetDestination(prey.position);
        
        float dist = Vector3.Distance(ai.transform.position, prey.position);
        while(dist > ai.attackRange)
        {
            dist = Vector3.Distance(ai.transform.position, prey.position);
            yield return new WaitForSeconds(0.5f);
        }
        ai.ChangeState(new AnimalAttackState(ai, prey));
    }
    
    public override void Exit()
    {
        ai.agent.speed = 3.5f;
    }
}

// 攻击状态
public class AnimalAttackState : CreatureActionState
{
    private Transform prey;
    private float attackTimer;
    
    public AnimalAttackState(CreatureAI ai, Transform prey) : base(ai)
    {
        this.prey = prey;
        type = CreatureActionStateType.Attack;
    }
    
    public override void Enter()
    {
        ai.agent.ResetPath();
        attackTimer = 2f;
        // 立即切换到攻击姿态（无过渡，表现紧张感）
        ai.anim.SetAnimationImmediate(AnimState.Idle);
        ai.StartCoroutine(Execute());
    }
    
    public override IEnumerator Execute()
    {
        while(attackTimer > 0)
        {
            attackTimer -= 1f;
            yield return new WaitForSeconds(1);
        }

        // 捕猎成功 
        // TODO: 变成新鲜尸体，食用
        ai.stat.IncreaseSatiety(50);
        Destroy(prey.gameObject);
        ai.ChangeState(new AnimalWanderState(ai));
    }

    private void Destroy(GameObject gameObject)
    {
        GameUtils.Instance.DestroyGameObject(gameObject);
    }
}

// 休息状态
public class AnimalRestState : CreatureActionState
{
    private float restTimer;
    
    public AnimalRestState(CreatureAI ai) : base(ai) 
    { 
        type = CreatureActionStateType.Rest;
    }
    
    public override void Enter()
    {
        ai.agent.ResetPath();
        restTimer = Random.Range(10f, 30f);

        ai.LockState();
        ai.StartCoroutine(Execute());
    }
    
    public override IEnumerator Execute()
    {
        while(restTimer > 0)
        {
            restTimer -= 1f;
            yield return new WaitForSeconds(1f);
            ai.stat.Heal(1);
        }
        ai.ChangeState(new AnimalWanderState(ai));
        yield break;
    }
}


// 逃跑状态
public class FleeState : CreatureActionState
{
    private GameObject threat;
    private Vector3 fleeTarget;
    private float checkTimer;
    private float safetyCheckInterval = 1f;
    
    public FleeState(CreatureAI ai, GameObject threat) : base(ai)
    {
        this.threat = threat;
    }
    
    public override void Enter()
    {
        Debug.Log($"{ai.gameObject.name} 开始逃跑！");
        
        // 计算逃跑方向（远离威胁）
        CalculateFleeDirection();
        
        // 最快速度逃跑
        ai.agent.speed = 7f;
        ai.anim.SetAnimationSmooth(AnimState.Run, 0.05f, EaseType.EaseInQuad);
        
        checkTimer = 0f;

        ai.StartCoroutine(Execute());
    }

    public override IEnumerator Execute()
    {
        while(true)
        {
            checkTimer += 0.1f;
        
            // 定期检查是否安全
            if (checkTimer >= safetyCheckInterval)
            {
                checkTimer = 0f;
                
                // 检查是否已经安全（距离威胁足够远或威胁消失）
                if (IsSafe())
                {
                    Debug.Log($"{ai.gameObject.name} 已经安全，停止逃跑");
                    ai.stat.ResetEmotions();
                    ai.ChangeState(new AnimalWanderState(ai));
                    yield break;
                }
                
                // 如果太接近威胁，重新计算逃跑方向
                if (threat != null)
                {
                    float distToThreat = Vector3.Distance(ai.transform.position, threat.transform.position);
                    if (distToThreat < ai.fleeDistance * 0.5f)
                    {
                        CalculateFleeDirection();
                    }
                }
            }
            
            // 检查是否到达逃跑目标点
            if (!ai.agent.pathPending && ai.agent.remainingDistance < 1f)
            {
                // 继续向更远处逃跑
                CalculateFleeDirection();
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    public override void Exit()
    {
        ai.agent.speed = 3.5f;
    }
    
    private void CalculateFleeDirection()
    {
        Vector3 fleeDirection;
        
        if (threat != null)
        {
            // 从威胁反方向逃跑
            // fleeDirection = (ai.transform.position - threat.transform.position).normalized;

            float fleeAngle = 180f;
            // 朝向威胁的反方向
            Vector3 awayDir = (ai.transform.position - threat.transform.position).normalized;

            // 在反方向 ±fleeAngle 之间随机一个角度
            float randomAngle = Random.Range(-fleeAngle, fleeAngle);

            // 绕Y轴旋转（假设地面在XZ平面）
            Quaternion rotation = Quaternion.AngleAxis(randomAngle, Vector3.up);
            fleeDirection = rotation * awayDir;

            // （可选）保持XZ平面方向
            fleeDirection.y = 0;
            fleeDirection.Normalize();
        }
        else
        {
            // 如果威胁消失，随机方向逃跑
            fleeDirection = Random.insideUnitSphere;
            fleeDirection.y = 0;
            fleeDirection.Normalize();
        }
        
        // 计算逃跑目标点
        fleeTarget = ai.transform.position + fleeDirection * ai.fleeDistance;
        
        // 确保目标点在NavMesh上
        NavMeshHit hit;
        if (NavMesh.SamplePosition(fleeTarget, out hit, ai.fleeDistance, NavMesh.AllAreas))
        {
            fleeTarget = hit.position;
            ai.agent.SetDestination(fleeTarget);
        }
    }
    
    private bool IsSafe()
    {
        // 威胁消失
        if (threat == null)
            return true;
        
        // 距离足够远
        float distToThreat = Vector3.Distance(ai.transform.position, threat.transform.position);
        if (distToThreat > ai.fleeDistance)
            return true;
        
        // 恐惧值已经降低
        if (ai.stat.GetFear() < ai.stat.personality.fearThreshold * 0.5f)
            return true;
        
        return false;
    }
}


// 战斗状态
public class CombatState : CreatureActionState
{
    private GameObject target;
    private float attackCooldown;
    private float attackInterval = 1.5f;  // 攻击间隔
    private float chaseTimer;
    private float maxChaseTime = 10f;     // 最大追逐时间
    
    public CombatState(CreatureAI ai, GameObject target) : base(ai)
    {
        this.target = target;
    }
    
    public override void Enter()
    {
        Debug.Log($"{ai.gameObject.name} 进入战斗状态！ 目标 {target.name}");
        
        ai.agent.speed = 6f;  // 追逐速度
        ai.anim.SetAnimationSmooth(AnimState.Run, 0.1f, EaseType.EaseInQuad);
        
        attackCooldown = 0f;
        chaseTimer = 0f;

        ai.StartCoroutine(Execute());
    }
    
    public override IEnumerator Execute()
    {
        while(true)
        {
            // 目标消失或死亡
            if (target == null)
            {
                Debug.Log("目标消失，退出战斗");
                ai.stat.DecayEmotions(Time.deltaTime * 5f);  // 快速平复情绪
                ai.ChangeState(new AnimalWanderState(ai));
                yield break;
            }
            
            chaseTimer += 0.1f;
            attackCooldown -= 0.1f;
            
            // 追逐超时，放弃战斗
            if (chaseTimer > maxChaseTime)
            {
                Debug.Log("追逐超时，放弃战斗");
                ai.stat.ResetEmotions();
                ai.ChangeState(new AnimalWanderState(ai));
                yield break;
            }
            
            float distToTarget = Vector3.Distance(ai.transform.position, target.transform.position);
            
            // 目标太远，放弃战斗
            if (distToTarget > ai.detectionRange * 2f)
            {
                Debug.Log("目标逃离，放弃战斗");
                ai.stat.ResetEmotions();
                ai.ChangeState(new AnimalWanderState(ai));
                yield break;
            }
            // 在攻击范围内
            if (distToTarget <= ai.attackRange)
            {
                chaseTimer = 0;
                // 停止移动，面向目标
                ai.agent.ResetPath();
                
                Vector3 direction = (target.transform.position - ai.transform.position).normalized;
                ai.transform.rotation = Quaternion.Slerp(
                    ai.transform.rotation,
                    Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z)),
                    Time.deltaTime * 10f
                );
                
                // 切换到idle动画（攻击姿态）
                if (ai.anim.GetCurrentAnimation().vert > 0.5f)
                {
                    ai.anim.SetAnimationSmooth(AnimState.Idle, 0.2f);
                }
                
                // 执行攻击
                if (attackCooldown <= 0f)
                {
                    PerformAttack();
                    attackCooldown = attackInterval;
                }
            }
            else
            {
                // 追逐目标
                ai.agent.SetDestination(target.transform.position);
                
                // 确保是奔跑动画
                if (ai.anim.GetCurrentAnimation().state < 0.8f)
                {
                    ai.anim.SetAnimationSmooth(AnimState.Run, 0.15f);
                }
            }
            
            // 检查是否应该撤退（生命值过低或恐惧值上升）
            float healthPercent = (float)ai.stat.GetHealth() / ai.stat.GetMaxHealth();
            if (healthPercent < 0.2f || ai.stat.GetFear() > ai.stat.GetAnger())
            {
                Debug.Log("生命值过低或恐惧，转为逃跑");
                ai.ChangeState(new FleeState(ai, target));
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    public override void Exit()
    {
        ai.agent.speed = 3.5f;
    }
    
    private void PerformAttack()
    {
        Debug.Log($"{ai.gameObject.name} 攻击 {target.name}！");
        
        // 尝试对目标造成伤害
        var damageReceiver = target.GetComponent<IDamageable>();
        if (damageReceiver != null)
        {
            DamageInfo damageInfo = new DamageInfo
            {
                damage = ai.attackDamage,
                damageType = DamageType.Melee,
                attacker = ai.gameObject,
                knockbackDirection = new Vector3(0, 0, 0),
                knockbackForce = 2f,
                isCritical = false
                // isCritical = Random.value < 0.15f // 15%暴击率
            };
            damageReceiver.TakeDamage(damageInfo);
        }
        // 可选：播放攻击动画/音效
        // ai.anim.PlayAttackAnimation();
    }
}