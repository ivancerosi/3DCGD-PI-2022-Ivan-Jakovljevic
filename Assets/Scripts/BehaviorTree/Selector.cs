using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selector : Node
{
    public LinkedList<Node> nodes = new LinkedList<Node>();


    override public Node.STATE evaluate()
    {
        foreach (var node in nodes)
        {
            STATE outcome = node.evaluate();
            if (outcome == Node.STATE.SUCCESS) return Node.STATE.SUCCESS;
            if (outcome == Node.STATE.RUNNING) return Node.STATE.RUNNING;
        }
        return Node.STATE.FAILURE;
    }

    override public void setEvaluate(evaluateSAM eval) { }
}
