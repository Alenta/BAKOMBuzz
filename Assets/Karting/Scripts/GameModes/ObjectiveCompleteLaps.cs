using System.Collections;
using KartGame.Track;
using UnityEngine;

public class ObjectiveCompleteLaps : Objective
{

    [Tooltip("How many laps should the player complete before the game is over?")]
    public int lapsToComplete;

    [Header("Notification")]
    [Tooltip("Start sending notification about remaining laps when this amount of laps is left")]
    public int notificationLapsRemainingThreshold = 1;

    [Header("Laps With Targets")]
    [Tooltip("Only used in LapsWithTargets mode: name of the things that must all be collected before the final lap counts")]
    public string collectibleName = "checkpoint";

    public int currentLap { get; private set; }

    bool IsFinalLap => currentLap + 1 >= lapsToComplete;

    void Awake()
    {
        currentLap = 0;

        // set a title and description specific for this type of objective, if it hasn't one
        if (string.IsNullOrEmpty(title))
            title = $"Complete {lapsToComplete} {targetName}s";

    }

    IEnumerator Start()
    {
        // LapsWithTargets reuses the existing Laps behaviour in the time manager
        GameMode timeMode = gameMode == GameMode.LapsWithTargets ? GameMode.Laps : gameMode;
        TimeManager.OnSetTime(totalTimeInSecs, isTimed, timeMode);
        TimeDisplay.OnSetLaps(lapsToComplete);
        yield return new WaitForEndOfFrame();
        Register();
    }

    // Rejects the final lap until every plain target has been collected
    protected override bool CanFinishLap()
    {
        if (gameMode != GameMode.LapsWithTargets) return true;
        if (!IsFinalLap || AllTargetsCollected) return true;

        UpdateObjective(string.Empty, GetUpdatedCounterAmount(),
            $"Collect all the {collectibleName}s first ({NumberOfTargetsRemaining} left)");
        return false;
    }

    protected override void OnTargetCollected()
    {
        print("Collected target");
        if (isCompleted) return;
        print("Is completed");

        string notificationText = AllTargetsCollected
            ? $"All {collectibleName}s collected - finish the race!"
            : string.Empty;

        UpdateObjective(string.Empty, GetUpdatedCounterAmount(), notificationText);
    }

    protected override void ReachCheckpoint(int remaining)
    {

        if (isCompleted)
            return;

        currentLap++;

        int targetRemaining = lapsToComplete - currentLap;

        // update the objective text according to how many enemies remain to kill
        if (targetRemaining == 0)
        {
            CompleteObjective(string.Empty, GetUpdatedCounterAmount(),
                "Objective complete: " + title);
        }
        else if (targetRemaining == 1)
        {
            string notificationText = notificationLapsRemainingThreshold >= targetRemaining
                ? "One " + targetName + " left"
                : string.Empty;
            UpdateObjective(string.Empty, GetUpdatedCounterAmount(), notificationText);
        }
        else if (targetRemaining > 1)
        {
            // create a notification text if needed, if it stays empty, the notification will not be created
            string notificationText = notificationLapsRemainingThreshold >= targetRemaining
                ? targetRemaining + " " + targetName + "s to collect left"
                : string.Empty;

            UpdateObjective(string.Empty, GetUpdatedCounterAmount(), notificationText);
        }

    }

    public override string GetUpdatedCounterAmount()
    {
        string laps = currentLap + " / " + lapsToComplete;

        if (gameMode != GameMode.LapsWithTargets)
            return laps;

        return laps + "  (" + NumberOfTargetsCollected + " / " + NumberOfTargetsTotal + " " + collectibleName + "s)";
    }

}