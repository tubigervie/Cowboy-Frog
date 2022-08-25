using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CinemachineTargetGroup))]
public class CinemachineTarget : SingletonMonobehaviour<CinemachineTarget>
{
    private CinemachineTargetGroup cinemachineTargetGroup;
    private List<CinemachineTargetGroup.Target> targets = new List<CinemachineTargetGroup.Target>();

    protected override void Awake()
    {
        base.Awake();
        cinemachineTargetGroup = GetComponent<CinemachineTargetGroup>();
    }

    private void Start()
    {
        SetCinemachineTargetGroupToPlayer();
    }

    private void SetCinemachineTargetGroupToPlayer()
    {
        CinemachineTargetGroup.Target cinemachineGroupTarget_player = new CinemachineTargetGroup.Target { weight = 1f, radius = 1f, target = GameManager.Instance.GetPlayer().transform };
        CinemachineTargetGroup.Target[] cinemachineTargetArray = new CinemachineTargetGroup.Target[] { cinemachineGroupTarget_player };
        targets.Add(cinemachineGroupTarget_player);
        cinemachineTargetGroup.m_Targets = cinemachineTargetArray;
    }

    private void SetCinemachineTarget(Transform targetToAdd)
    {
        CinemachineTargetGroup.Target cinemachineGroupTarget = new CinemachineTargetGroup.Target { weight = 1f, radius = 1f, target = targetToAdd };
        CinemachineTargetGroup.Target[] cinemachineTargetArray = new CinemachineTargetGroup.Target[] { cinemachineGroupTarget };
        targets.Add(cinemachineGroupTarget);
        cinemachineTargetGroup.m_Targets = cinemachineTargetArray;
    }

    public void SetToTargets()
    {
        cinemachineTargetGroup.m_Targets = targets.ToArray();
    }

    public void AddToTargetGroup(Transform targetToAdd)
    {
        if (CheckIfContains(targetToAdd)) return;
        CinemachineTargetGroup.Target cinemachineGroupTarget_toAdd = new CinemachineTargetGroup.Target { weight = 1f, radius = 1f, target = targetToAdd };
        targets.Add(cinemachineGroupTarget_toAdd);
        cinemachineTargetGroup.m_Targets = targets.ToArray();
    }

    public void RemoveFromTargetGroup(Transform targetToRemove)
    {
        CinemachineTargetGroup.Target? target = GetByTransform(targetToRemove);
        if (target == null) return;
        targets.Remove((CinemachineTargetGroup.Target)target);
    }

    private CinemachineTargetGroup.Target? GetByTransform(Transform targetToGet)
    {
        foreach (CinemachineTargetGroup.Target target in targets)
        {
            if (target.target == targetToGet)
            {
                return target;
            }
        }
        return null;
    }

    private bool CheckIfContains(Transform targetToAdd)
    {
        foreach(CinemachineTargetGroup.Target target in targets)
        {
            if(target.target == targetToAdd)
            {
                return true;
            }
        }
        return false;
    }

}
