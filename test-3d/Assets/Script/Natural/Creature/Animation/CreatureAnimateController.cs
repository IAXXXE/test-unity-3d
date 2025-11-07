using UnityEngine;
using System.Collections;

public class CreatureAnimation : MonoBehaviour
{
    [Header("组件引用")]
    public Animator animator;
    
    private readonly int stateHash = Animator.StringToHash("State");
    private readonly int vertHash = Animator.StringToHash("Vert");
    
    [Header("平滑设置")]
    public float transitionDuration = 0.2f;
    public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);  // 自定义曲线
    
    private float currentVert;
    private float currentState;
    
    private Coroutine transitionCoroutine;
    private bool isTransitioning = false;
    
    private void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }
    
    // 平滑过渡（使用预设曲线）
    public void SetAnimationSmooth(AnimState state, float duration = -1f, EaseType easeType = EaseType.SmoothStep)
    {
        float targetVert = 0f;
        float targetState = 0f;
        
        switch (state)
        {
            case AnimState.Idle:
                targetVert = 0f;
                targetState = 0f;
                break;
            case AnimState.Walk:
                targetVert = 1f;
                targetState = 0f;
                break;
            case AnimState.Run:
                targetVert = 1f;
                targetState = 1f;
                break;
        }
        
        SetAnimationSmooth(targetVert, targetState, duration, easeType);
    }
    
    // 平滑过渡（自定义参数）
    public void SetAnimationSmooth(float targetVert, float targetState, float duration = -1f, EaseType easeType = EaseType.SmoothStep)
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }
        
        if (duration < 0)
            duration = transitionDuration;
        
        transitionCoroutine = StartCoroutine(TransitionToAnimation(targetVert, targetState, duration, easeType));
    }
    
    // 协程：执行平滑过渡
    private IEnumerator TransitionToAnimation(float targetVert, float targetState, float duration, EaseType easeType)
    {
        isTransitioning = true;
        
        float startVert = currentVert;
        float startState = currentState;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            
            // 根据缓动类型计算插值
            float easedT = ApplyEase(t, easeType);
            
            currentVert = Mathf.Lerp(startVert, targetVert, easedT);
            currentState = Mathf.Lerp(startState, targetState, easedT);
            
            animator.SetFloat(vertHash, currentVert);
            animator.SetFloat(stateHash, currentState);
            
            yield return null;
        }
        
        // 确保最终值精确
        currentVert = targetVert;
        currentState = targetState;
        animator.SetFloat(vertHash, currentVert);
        animator.SetFloat(stateHash, currentState);
        
        isTransitioning = false;
        transitionCoroutine = null;
        
        OnTransitionComplete();
    }
    
    // 使用自定义AnimationCurve过渡
    public void SetAnimationSmoothWithCurve(AnimState state, AnimationCurve curve = null, float duration = -1f)
    {
        float targetVert = 0f;
        float targetState = 0f;
        
        switch (state)
        {
            case AnimState.Idle:
                targetVert = 0f;
                targetState = 0f;
                break;
            case AnimState.Walk:
                targetVert = 1f;
                targetState = 0f;
                break;
            case AnimState.Run:
                targetVert = 1f;
                targetState = 1f;
                break;
        }
        
        SetAnimationSmoothWithCurve(targetVert, targetState, curve, duration);
    }
    
    public void SetAnimationSmoothWithCurve(float targetVert, float targetState, AnimationCurve curve = null, float duration = -1f)
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }
        
        if (duration < 0)
            duration = transitionDuration;
        
        if (curve == null)
            curve = transitionCurve;
        
        transitionCoroutine = StartCoroutine(TransitionWithCurve(targetVert, targetState, duration, curve));
    }
    
    // 使用AnimationCurve的协程
    private IEnumerator TransitionWithCurve(float targetVert, float targetState, float duration, AnimationCurve curve)
    {
        isTransitioning = true;
        
        float startVert = currentVert;
        float startState = currentState;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            
            // 使用AnimationCurve计算插值
            float curveT = curve.Evaluate(t);
            
            currentVert = Mathf.Lerp(startVert, targetVert, curveT);
            currentState = Mathf.Lerp(startState, targetState, curveT);
            
            animator.SetFloat(vertHash, currentVert);
            animator.SetFloat(stateHash, currentState);
            
            yield return null;
        }
        
        currentVert = targetVert;
        currentState = targetState;
        animator.SetFloat(vertHash, currentVert);
        animator.SetFloat(stateHash, currentState);
        
        isTransitioning = false;
        transitionCoroutine = null;
        
        OnTransitionComplete();
    }
    
    // 应用缓动函数
    private float ApplyEase(float t, EaseType easeType)
    {
        switch (easeType)
        {
            case EaseType.Linear:
                return t;
                
            case EaseType.SmoothStep:
                return Mathf.SmoothStep(0f, 1f, t);
                
            case EaseType.EaseInQuad:
                return t * t;
                
            case EaseType.EaseOutQuad:
                return t * (2 - t);
                
            case EaseType.EaseInOutQuad:
                return t < 0.5f ? 2 * t * t : -1 + (4 - 2 * t) * t;
                
            case EaseType.EaseInCubic:
                return t * t * t;
                
            case EaseType.EaseOutCubic:
                return (--t) * t * t + 1;
                
            case EaseType.EaseInOutCubic:
                return t < 0.5f ? 4 * t * t * t : (t - 1) * (2 * t - 2) * (2 * t - 2) + 1;
                
            default:
                return t;
        }
    }
    
    // 立即设置动画
    public void SetAnimationImmediate(AnimState state)
    {
        float targetVert = 0f;
        float targetState = 0f;
        
        switch (state)
        {
            case AnimState.Idle:
                targetVert = 0f;
                targetState = 0f;
                break;
            case AnimState.Walk:
                targetVert = 1f;
                targetState = 0f;
                break;
            case AnimState.Run:
                targetVert = 1f;
                targetState = 1f;
                break;
        }
        
        SetAnimationImmediate(targetVert, targetState);
    }
    
    public void SetAnimationImmediate(float targetVert, float targetState)
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
            transitionCoroutine = null;
        }
        
        isTransitioning = false;
        currentVert = Mathf.Clamp01(targetVert);
        currentState = Mathf.Clamp01(targetState);
        
        animator.SetFloat(vertHash, currentVert);
        animator.SetFloat(stateHash, currentState);
    }
    
    public bool IsTransitioning()
    {
        return isTransitioning;
    }
    
    public (float vert, float state) GetCurrentAnimation()
    {
        return (currentVert, currentState);
    }
    
    public void StopTransition()
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
            transitionCoroutine = null;
            isTransitioning = false;
        }
    }
    
    private void OnTransitionComplete()
    {
        // Debug.Log($"动画过渡完成: Vert={currentVert:F2}, State={currentState:F2}");
    }
    
    private void OnDisable()
    {
        StopTransition();
    }
}

// 缓动类型枚举
public enum EaseType
{
    Linear,
    SmoothStep,
    EaseInQuad,
    EaseOutQuad,
    EaseInOutQuad,
    EaseInCubic,
    EaseOutCubic,
    EaseInOutCubic
}

public enum AnimState
{
    Idle,
    Walk,
    Run,
    Eat,
    Drink,
    Attack,
    Rest
}