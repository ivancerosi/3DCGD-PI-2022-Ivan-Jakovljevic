using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sequence : Node
{
    public LinkedList<Node> nodes;

    public Sequence()
    {
        nodes = new LinkedList<Node>();
    }

    override public Node.STATE evaluate()
    {
        foreach (var node in nodes)
        {
            STATE outcome = node.evaluate();
            if (outcome == Node.STATE.FAILURE) return Node.STATE.FAILURE;
            if (outcome == Node.STATE.RUNNING) return Node.STATE.RUNNING;
        }
        return Node.STATE.SUCCESS;
    }
    override public void setEvaluate(evaluateSAM eval) { }

}
