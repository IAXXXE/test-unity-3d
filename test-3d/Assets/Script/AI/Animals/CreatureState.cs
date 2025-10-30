using UnityEngine;
using UnityEngine.AI;

// 状态基类
public abstract class CreatureState
{
    protected CreatureAI ai;
    
    public CreatureState(CreatureAI ai)
    {
        this.ai = ai;
    }
    
    public virtual void Enter() { }
    public virtual void Execute() { }
    public virtual void Exit() { }
}

// 游走状态
public class AnimalWanderState : CreatureState
{
    private float wanderTimer;
    private float pauseTimer;
    private bool isPaused;
    
    public AnimalWanderState(CreatureAI ai) : base(ai) { }
    
    public override void Enter()
    {
        SetNewWanderDestination();
    }
    
    public override void Execute()
    {
        if (isPaused)
        {
            pauseTimer -= Time.deltaTime;
            if (pauseTimer <= 0)
            {
                isPaused = false;
                SetNewWanderDestination();
            }
        }
        else
        {
            if (!ai.agent.pathPending && ai.agent.remainingDistance < 0.5f)
            {
                // 到达目的地，随机决定是否休息
                if (Random.value > 0.3f)
                {
                    isPaused = true;
                    pauseTimer = Random.Range(3f, 8f);
                    ai.agent.ResetPath();
                }
                else
                {
                    SetNewWanderDestination();
                }
            }
        }
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
    }
}

// 寻找水源状态
public class AnimalSeekWaterState : CreatureState
{
    private Vector3 waterPosition;
    
    public AnimalSeekWaterState(CreatureAI ai) : base(ai) { }
    
    public override void Enter()
    {
        waterPosition = ai.GetNearestWaterSource();
        ai.agent.SetDestination(waterPosition);
        ai.agent.speed = 5f; // 加速移动
    }
    
    public override void Execute()
    {
        float dist = Vector3.Distance(ai.transform.position, waterPosition);
        if (dist < ai.drinkRange)
        {
            ai.ChangeState(new AnimalDrinkState(ai));
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
    
    public AnimalDrinkState(CreatureAI ai) : base(ai) { }
    
    public override void Enter()
    {
        ai.agent.ResetPath();
        drinkTimer = 3f;
    }
    
    public override void Execute()
    {
        drinkTimer -= Time.deltaTime;
        
        // 持续恢复渴度
        ai.stat.IncreaseThirsty((int)(20 * Time.deltaTime));
        
        if (drinkTimer <= 0 || ai.stat.GetThirsty() >= ai.stat.GetMaxThirsty())
        {
            ai.ChangeState(new AnimalWanderState(ai));
        }
    }
}

// 寻找猎物状态
public class AnimalSeekFoodState : CreatureState
{
    private Transform food;
    
    public AnimalSeekFoodState(CreatureAI ai) : base(ai) { }
    
    public override void Enter()
    {
        food = ai.FindNearestFood();
        if (food != null)
        {
            ai.agent.speed = 6f;
        }
    }
    
    public override void Execute()
    {
        if (food == null)
        {
            food = ai.FindNearestFood();
            if (food == null)
            {
                ai.ChangeState(new AnimalWanderState(ai));
                return;
            }
        }
        
        ai.agent.SetDestination(food.position);
        
        float dist = Vector3.Distance(ai.transform.position, food.position);
        Debug.Log($" dist {dist} range {ai.eatRange}");
        if (dist < ai.eatRange)
        {
            Debug.Log("Change To Eat State");
            // ai.ChangeState(new AnimalEatState(ai));
            ai.stat.IncreaseSatiety(30);
        }
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
    
    public AnimalEatState(CreatureAI ai) : base(ai) { }
    
    public override void Enter()
    {
        ai.agent.ResetPath();
        eatTimer = 1f;
    }
    
    public override void Execute()
    {
        eatTimer -= Time.deltaTime;
        
        // 持续恢复饱食度
        ai.stat.IncreaseSatiety((int)(20 * Time.deltaTime));
        
        if (eatTimer <= 0 || ai.stat.GetSatiety() >= ai.stat.GetMaxSatiety())
        {
            ai.ChangeState(new AnimalWanderState(ai));
        }
    }
}

// 寻找猎物状态
public class AnimalSeekPreyState : CreatureState
{
    private Transform prey;
    
    public AnimalSeekPreyState(CreatureAI ai) : base(ai) { }
    
    public override void Enter()
    {
        prey = ai.FindNearestPrey();
        if (prey != null)
        {
            ai.agent.speed = 6f;
        }
    }
    
    public override void Execute()
    {
        if (prey == null)
        {
            prey = ai.FindNearestPrey();
            if (prey == null)
            {
                ai.ChangeState(new AnimalWanderState(ai));
                return;
            }
        }
        
        ai.agent.SetDestination(prey.position);
        
        float dist = Vector3.Distance(ai.transform.position, prey.position);
        if (dist < ai.attackRange)
        {
            ai.ChangeState(new AnimalAttackState(ai, prey));
        }
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
    }
    
    public override void Enter()
    {
        ai.agent.ResetPath();
        attackTimer = 2f;
    }
    
    public override void Execute()
    {
        attackTimer -= Time.deltaTime;
        
        if (attackTimer <= 0)
        {
            // 捕猎成功 
            // TODO: 变成新鲜尸体，食用
            ai.stat.IncreaseSatiety(50);
            Destroy(prey.gameObject);
            ai.ChangeState(new AnimalWanderState(ai));
        }
    }

    private void Destroy(GameObject gameObject)
    {
        throw new System.NotImplementedException();
    }
}

// 休息状态
public class AnimalRestState : CreatureState
{
    private float restTimer;
    
    public AnimalRestState(CreatureAI ai) : base(ai) { }
    
    public override void Enter()
    {
        ai.agent.ResetPath();
        restTimer = Random.Range(5f, 10f);
    }
    
    public override void Execute()
    {
        restTimer -= Time.deltaTime;
        
        // 加速恢复生命值
        ai.stat.Heal((int)(5 * Time.deltaTime));
        
        if (restTimer <= 0 || ai.stat.GetHealth() >= ai.stat.GetMaxHealth())
        {
            ai.ChangeState(new AnimalWanderState(ai));
        }
    }
}