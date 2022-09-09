using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : IEvaluate
{
    public delegate STATE evaluateSAM();
    evaluateSAM _evaluate;

    public virtual void setEvaluate(evaluateSAM eval)
    {
        _evaluate = eval;
    }
    public Node(evaluateSAM eval)
    {
        _evaluate = eval;
    }

    protected Node()
    {
    }
    public enum STATE { SUCCESS,FAILURE,RUNNING}

    public virtual STATE evaluate() { return _evaluate(); }
   
}
