using System;
using System.Collections.Generic;
using KartGame.Track;
using UnityEngine;
using UnityEngine.Events;

public enum GameMode
{
    TimeLimit, Crash, Laps,
    // Works like Laps, but the final lap only counts once every plain target has been collected
    LapsWithTargets
}

public abstract class Objective : MonoBehaviour
{
    [Tooltip("Which game mode are you playing?")]
    public GameMode gameMode;

    protected int m_PickupTotal;

    [Tooltip("Name of the target object the player will collect/crash/complete for this objective")]
    public string targetName;

    [Tooltip("Short text explaining the objective that will be shown on screen")]
    public string title;

    [Tooltip("Short text explaining the objective that will be shown on screen")]
    public string description;

    [Tooltip("Whether the objective is required to win or not")]
    public bool isOptional;

    [Tooltip("Delay before the objective becomes visible")]
    public float delayVisible;

    [Header("Requirements")]
    [Tooltip("Does the objective have a time limit?")]
    public bool isTimed;

    [Tooltip("If there is a time limit, how long in secs?")]
    public int totalTimeInSecs;
    public bool isCompleted { get; protected set; }
    public bool isBlocking() => !(isOptional || isCompleted);

    public UnityAction<UnityActionUpdateObjective> onUpdateObjective;

    protected NotificationHUDManager m_NotificationHUDManager;
    protected ObjectiveHUDManger m_ObjectiveHUDManger;

    public static Action<TargetObject> OnRegisterPickup;
    public static Action<TargetObject> OnUnregisterPickup;

    public DisplayMessage displayMessage;

    private List<TargetObject> pickups = new List<TargetObject>();

    public List<TargetObject> Pickups => pickups;
    public int NumberOfPickupsTotal { get; private set; }
    public int NumberOfPickupsRemaining => Pickups.Count;

    // LapsWithTargets only: plain targets (anything that is not a LapObject) are counted separately,
    // so they never interfere with the per-lap checkpoint logic in Pickups.
    public int NumberOfTargetsTotal { get; private set; }
    public int NumberOfTargetsCollected { get; private set; }
    public int NumberOfTargetsRemaining => NumberOfTargetsTotal - NumberOfTargetsCollected;
    public bool AllTargetsCollected => NumberOfTargetsCollected >= NumberOfTargetsTotal;

    protected bool IsLapMode => gameMode == GameMode.Laps || gameMode == GameMode.LapsWithTargets;

    bool IsPlainTarget(TargetObject pickup) =>
        gameMode == GameMode.LapsWithTargets && !(pickup is LapObject);

    public int NumberOfActivePickupsRemaining()
    {
        int total = 0;
        for (int i = 0; i < Pickups.Count; i++)
        {
            if (Pickups[i].active) total++;
        }

        return total;
    }

    protected abstract void ReachCheckpoint(int remaining);

    // Called when a lap is about to be completed. Return false to reject the lap (it will not count).
    protected virtual bool CanFinishLap() => true;

    // Called when a plain target is collected in LapsWithTargets mode.
    protected virtual void OnTargetCollected() { }

    void OnEnable()
    {
        OnRegisterPickup += RegisterPickup;
        OnUnregisterPickup += UnregisterPickup;
    }

    protected void Register()
    {
        // add this objective to the list contained in the objective manager
        ObjectiveManager.RegisterObjective(this);

        // register this objective in the ObjectiveHUDManger
        m_ObjectiveHUDManger = FindObjectOfType<ObjectiveHUDManger>();
        DebugUtility.HandleErrorIfNullFindObject<ObjectiveHUDManger, Objective>(m_ObjectiveHUDManger, this);
        m_ObjectiveHUDManger.RegisterObjective(this);

        // register this objective in the NotificationHUDManager
        m_NotificationHUDManager = FindObjectOfType<NotificationHUDManager>();
        DebugUtility.HandleErrorIfNullFindObject<NotificationHUDManager, Objective>(m_NotificationHUDManager, this);
        m_NotificationHUDManager.RegisterObjective(this);
    }

    public void UpdateObjective(string descriptionText, string counterText, string notificationText)
    {
        onUpdateObjective?.Invoke(new UnityActionUpdateObjective(this, descriptionText, counterText, false,
            notificationText));
    }

    public void CompleteObjective(string descriptionText, string counterText, string notificationText)
    {
        isCompleted = true;
        UpdateObjective(descriptionText, counterText, notificationText);

        // unregister this objective form both HUD managers
        m_ObjectiveHUDManger.UnregisterObjective(this);
        m_NotificationHUDManager.UnregisterObjective(this);
    }

    public virtual string GetUpdatedCounterAmount()
    {
        return "";
    }

    public void RegisterPickup(TargetObject pickup)
    {
        if (pickup.gameMode != gameMode) return;

        if (IsPlainTarget(pickup))
        {
            NumberOfTargetsTotal++;
            return;
        }

        Pickups.Add(pickup);

        NumberOfPickupsTotal++;
    }

    public void UnregisterPickup(TargetObject pickupCollected)
    {
        // very first line of UnregisterPickup, before the gameMode filter:

        if (pickupCollected.gameMode != gameMode) return;

        // plain targets only count towards the "all targets collected" requirement
        if (IsPlainTarget(pickupCollected))
        {
            NumberOfTargetsCollected++;
            OnTargetCollected();
            return;
        }

        // removes the pickup from the list, so that we can keep track of how many are left on the map
        if (IsLapMode)
        {
            pickupCollected.active = false;

            LapObject lapObject = (LapObject)pickupCollected;

            if (!lapObject.finishLap) return;
            if (!lapObject.lapOverNextPass)
            {
                TimeDisplay.OnUpdateLap();
                lapObject.lapOverNextPass = true;
                return;
            }

            if (NumberOfActivePickupsRemaining() != 0) return;

            // lap rejected (e.g. final lap without all targets): the player has to drive another lap
            if (!CanFinishLap())
            {
                ResetPickups();
                return;
            }

            ReachCheckpoint(0);
            ResetPickups();
            TimeDisplay.OnUpdateLap();

        }
        else
        {
            ReachCheckpoint(NumberOfPickupsRemaining - 1);
            Pickups.Remove(pickupCollected);
            if (gameMode == GameMode.Laps)
                KartGame.Track.TimeDisplay.OnUpdateLap();
        }
    }

    public void ResetPickups()
    {
        for (int i = 0; i < Pickups.Count; i++)
        {
            Pickups[i].active = true;
        }
    }

    void OnDisable()
    {
        OnRegisterPickup -= RegisterPickup;
        OnUnregisterPickup -= UnregisterPickup;
    }

}

public class UnityActionUpdateObjective
{
    public Objective objective;
    public string descriptionText;
    public string counterText;
    public bool isComplete;
    public string notificationText;

    public UnityActionUpdateObjective(Objective objective, string descriptionText, string counterText, bool isComplete, string notificationText)
    {
        this.objective = objective;
        this.descriptionText = descriptionText;
        this.counterText = counterText;
        this.isComplete = isComplete;
        this.notificationText = notificationText;
    }
}