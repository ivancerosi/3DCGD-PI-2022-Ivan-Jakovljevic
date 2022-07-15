using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils
{
    public static bool inRange(Vector3 originPos, Vector3 targetPos, float attackRange)
    {
        if ((targetPos - originPos).magnitude <= attackRange) return true;
        return false;
    }

    public delegate bool RayCast(Vector3 origin, Vector3 direction, out RaycastHit hitinfo, float range);
    public static bool canAttack(RayCast raycast, Vector3 originPos, Vector3 targetPos, RaycastHit hitinfo, float attackRange, string targetid) {
        if (!inRange(originPos, targetPos, attackRange)) return false;

        if (raycast(originPos, targetPos - originPos, out hitinfo, attackRange))
        {
            if (hitinfo.transform.tag==targetid || hitinfo.transform.name==targetid) return true;
        }
        return false;
    }

    public static bool canAttack(RayCast raycast, Vector3 originPos, Vector3 targetPos, float attackRange, string targetid)
    {
        if (!inRange(originPos, targetPos, attackRange)) return false;

        RaycastHit hitinfo;
        if (raycast(originPos, targetPos - originPos, out hitinfo, attackRange))
        {
            if (hitinfo.transform.tag == targetid || hitinfo.transform.name == targetid) return true;
        }
        return false;
    }

    public static bool canAttack(Vector3 originPos, Vector3 targetPos, float attackRange, string targetid)
    {
        if (!inRange(originPos, targetPos, attackRange)) return false;

        RaycastHit hitinfo;
        if (Physics.Raycast(originPos, targetPos - originPos, out hitinfo, attackRange))
        {
            if (hitinfo.transform.tag == targetid || hitinfo.transform.name == targetid) return true;
        }
        return false;
    }

}
