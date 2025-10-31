using System.Collections.Generic;

public enum NodeState { SUCCESS, FAILURE, RUNNING }

public abstract class Node
{
    public abstract NodeState Evaluate();
}

public class SelectorNode : Node
{
    private List<Node> children;
    
    public SelectorNode(params Node[] nodes)
    {
        children = new List<Node>(nodes);
    }
    
    public override NodeState Evaluate()
    {
        foreach (var child in children)
        {
            switch (child.Evaluate())
            {
                case NodeState.SUCCESS:
                    return NodeState.SUCCESS;
                case NodeState.RUNNING:
                    return NodeState.RUNNING;
            }
        }
        return NodeState.FAILURE;
    }
}

public class SequenceNode : Node
{
    private List<Node> children;
    
    public SequenceNode(params Node[] nodes)
    {
        children = new List<Node>(nodes);
    }
    
    public override NodeState Evaluate()
    {
        foreach (var child in children)
        {
            switch (child.Evaluate())
            {
                case NodeState.FAILURE:
                    return NodeState.FAILURE;
                case NodeState.RUNNING:
                    return NodeState.RUNNING;
            }
        }
        return NodeState.SUCCESS;
    }
}

public class ConditionNode : Node
{
    private System.Func<bool> condition;
    
    public ConditionNode(System.Func<bool> condition)
    {
        this.condition = condition;
    }
    
    public override NodeState Evaluate()
    {
        return condition() ? NodeState.SUCCESS : NodeState.FAILURE;
    }
}

public class ActionNode : Node
{
    private System.Func<NodeState> action;
    
    public ActionNode(System.Func<NodeState> action)
    {
        this.action = action;
    }
    
    public override NodeState Evaluate()
    {
        return action();
    }
}

public class BehaviorTree
{
    private Node root;
    
    public BehaviorTree(Node root)
    {
        this.root = root;
    }
    
    public void Evaluate()
    {
        root.Evaluate();
    }
}