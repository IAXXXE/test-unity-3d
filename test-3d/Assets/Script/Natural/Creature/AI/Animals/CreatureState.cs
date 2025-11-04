using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public enum CreatureStateType
{
    Wander,
    Rest,
    SeekFood,
    Attack,
    Escape,
    Eat,
    SeekWater,
    Drink,
    Sleep
}

// 状态基类
public abstract class CreatureState
{
    public CreatureStateType type;
    protected CreatureAI ai;
    
    public CreatureState(CreatureAI ai)
    {
        this.ai = ai;
    }
    
    public virtual void Enter() { }
    public virtual IEnumerator Execute() { yield break; }
    public virtual void Exit() { }
}

// 游走状态
public class AnimalWanderState : CreatureState
{
    private float breakTime;
    private float wanderTimer;
    
    public AnimalWanderState(CreatureAI ai) : base(ai) 
    {
        type = CreatureStateType.Wander;
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
            Debug.Log($" pos {ai.transform.position}  target {hit.position}");
        }
        ai.anim.SetAnimationSmooth(AnimState.Walk, 0.15f, EaseType.EaseOutQuad);
        ai.StartCoroutine(Execute());
    }
}

// 寻找水源状态
public class AnimalSeekWaterState : CreatureState
{
    private Vector3 waterPosition;
    
    public AnimalSeekWaterState(CreatureAI ai) : base(ai) 
    {
        type = CreatureStateType.SeekWater;
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
public class AnimalDrinkState : CreatureState
{
    private float drinkTimer;
    
    public AnimalDrinkState(CreatureAI ai) : base(ai) 
    {
        type = CreatureStateType.Drink;
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
public class AnimalSeekFoodState : CreatureState
{
    private Transform food;
    
    public AnimalSeekFoodState(CreatureAI ai) : base(ai) 
    { 
        type = CreatureStateType.SeekFood;
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
        }
        
        ai.agent.SetDestination(food.position);

        float dist = Vector3.Distance(ai.transform.position, food.position);
        while(dist > ai.eatRange)
        {
            yield return new WaitForSeconds(0.5f);
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
public class AnimalEatState : CreatureState
{
    private float eatTimer;
    
    public AnimalEatState(CreatureAI ai) : base(ai) 
    { 
        type = CreatureStateType.Eat;
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
public class AnimalSeekPreyState : CreatureState
{
    private Transform prey;
    
    public AnimalSeekPreyState(CreatureAI ai) : base(ai) 
    { 
        type = CreatureStateType.SeekFood;
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
public class AnimalAttackState : CreatureState
{
    private Transform prey;
    private float attackTimer;
    
    public AnimalAttackState(CreatureAI ai, Transform prey) : base(ai)
    {
        this.prey = prey;
        type = CreatureStateType.Attack;
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
public class AnimalRestState : CreatureState
{
    private float restTimer;
    
    public AnimalRestState(CreatureAI ai) : base(ai) 
    { 
        type = CreatureStateType.Rest;
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