using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveManager : MonoBehaviour
{
    List<Objective> m_Objectives = new List<Objective>();

    public List<Objective> Objectives => m_Objectives;

    public static Action<Objective> RegisterObjective;

    public void OnEnable()
    {
        RegisterObjective += OnRegisterObjective;
    }
    [ContextMenu("Log objectives")]
    void LogObjectives()
    {
        Debug.Log($"[Mgr] registered={m_Objectives.Count}");
        foreach (var o in m_Objectives)
            Debug.Log($"[Mgr] {o.name} mode={o.gameMode} completed={o.isCompleted} optional={o.isOptional} blocking={o.isBlocking()}", o);
    }
    public bool AreAllObjectivesCompleted()
    {
        if (m_Objectives.Count == 0)
            return false;

        for (int i = 0; i < m_Objectives.Count; i++)
        {
            // pass every objectives to check if they have been completed
            if (m_Objectives[i].isBlocking())
            {
                // break the loop as soon as we find one uncompleted objective
                return false;
            }
        }

        // found no uncompleted objective
        return true;
    }

    public void OnRegisterObjective(Objective objective)
    {
        m_Objectives.Add(objective);
    }
}
