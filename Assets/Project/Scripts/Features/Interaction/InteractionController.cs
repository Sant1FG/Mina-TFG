using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;

/// <summary>
/// Handles player interactions with elements of the scenario (coal veins and obstacles).
/// Raises events for toasts, vein collection, vein deposit and interactions prompts.
/// </summary>
public class InteractionController : MonoBehaviour
{
    //Reference to the player position
    [SerializeField] private Transform player;
    [SerializeField] private VeinSpawner spawner;
    [SerializeField] private KeyCode collectKey = KeyCode.E;
    private CoalVein closestVeinInRange;
    private Nexus nexusInRange;
    private SessionState state;
    private GameConfig config;
    /// <summary>
    /// Invoked to send a notification toast request. Carries a message (string) and a duration (float)
    /// </summary>
    public event Action<string, float> OnNotificationToast;
    /// <summary>
    /// Invoked to toggle the interaction prompt visibility
    /// </summary>
    public event Action<bool> OnShowInteraction;
    /// <summary>
    /// Invoked to signal that a coal unit has been successfully collected
    /// </summary>
    public event Action<bool> OnCollectCoal;
    /// <summary>
    /// Invoked to signal that a deposit action has been performed at the nexus.
    /// </summary>
    public event Action<bool> OnDepositCoal;
    private HashSet<CoalVein> veinsInRange;
    private bool interactionVisible;

    /// <summary>
    /// Called by Unity when the script instance is being loaded.
    /// Sets interactions prompt as hidden and creates a fresh set for veins in range.
    /// </summary>
    private void Awake()
    {
        interactionVisible = false;
        veinsInRange = new HashSet<CoalVein>();
    }

    /// <summary>
    /// Injects the session state used for capacity,score and flags. 
    /// </summary>
    /// <param name="sessionState">Injected session state.</param>
    public void SetSessionState(SessionState sessionState)
    {
        state = sessionState;
    }

    /// <summary>
    /// Injects the game configuration.
    /// </summary>
    /// <param name="gameConfig">Injected game config.</param>
    public void SetConfig(GameConfig gameConfig)
    {
        config = gameConfig;
    }

    /// <summary>
    /// Called by Unity once per frame. Polls the collect key while the game is running.
    /// Deposits at the nexus if present. Otherwise tries to collect the closest vein.
    /// </summary>
    private void Update()
    {
        if (state != null && !state.isRunning) return;

        if (Input.GetKeyDown(collectKey))
        {
            if (nexusInRange != null)
            {
                Debug.Log("InteractionController: Pressed interaction button next to Nexus");
                TryDeposit(nexusInRange);
                SetInteractionVisible(false);

            }
            else if (closestVeinInRange != null)
            {
                Debug.Log("InteractionController: Pressed interaction button next to coal vein");
                if (TryCollect(closestVeinInRange))
                {
                    RefreshInteractionTarget();
                }
            }
        }
    }

    /// <summary>
    ///  Registers the vein as in range and, if it is closer than the current target, makes it the closest vein.
    ///  Sets the interaction prompt as visible.
    /// </summary>
    /// <param name="v">Coal vein whose trigger the player entered</param>
    public void NotifyVeinEntered(CoalVein v)
    {
        if (v == null) return;
        veinsInRange.Add(v);

        //Only updates closest vein if it's better than the current one
        if (closestVeinInRange == null ||
        (v.transform.position - player.position).sqrMagnitude <
        (closestVeinInRange.transform.position - player.position).sqrMagnitude)
        {
            closestVeinInRange = v;
        }
        SetInteractionVisible(true);
    }

    /// <summary>
    /// Removes the vein from range and, if it was the closest, finds the new closest vein.
    /// Hides the  interactions prompt if there is no vein left in range of the player.
    /// </summary>
    /// <param name="v">Coal vein whose trigger the player exited</param>
    public void NotifyVeinExited(CoalVein v)
    {
        if (v == null) return;
        veinsInRange.Remove(v);

        if (closestVeinInRange == v) closestVeinInRange = SelectClosestVein();

        SetInteractionVisible(closestVeinInRange != null);
    }

    /// <summary>
    /// Shows the interaction prompt if there is coal to deposit.
    /// </summary>
    /// <param name="nexus">Nexus whose trigger the player entered.</param>
    public void NotifyNexusEntered(Nexus nexus)
    {
        if (nexus == null || state == null) return;
        nexusInRange = nexus;
        if (state.coalInDepot > 0) SetInteractionVisible(true);
    }

    /// <summary>
    /// Hides the interaction prompt when exiting the nexus trigger.
    /// </summary>
    /// <param name="nexus">Nexus whose trigger the player exited.</param>
    public void NotifyNexusExited(Nexus nexus)
    {
        if (nexus == null) return;
        nexusInRange = null;
        SetInteractionVisible(false);
    }


    /// <summary>
    /// Return the closest vein among those in range.
    /// </summary>
    /// <returns>The closest CoalVein or null if none are available</returns>
    private CoalVein SelectClosestVein()
    {

        if (veinsInRange.Count == 0) return null;

        CoalVein closest = null;
        float closestDist = float.MaxValue;
        Vector3 p = player.position;

        foreach (var item in veinsInRange)
        {
            if (item == null) continue;

            float dist = (item.transform.position - p).sqrMagnitude;
            if (dist < closestDist)
            {
                closest = item;
                closestDist = dist;
            }
        }

        return closest;
    }

    /// <summary>
    /// Attempts to collect from the specified vein.
    /// Emits a notification toast if vehicle deposit is full, otherwise raises the collection event,
    /// removes the vein from the in-range set and asks the spawner to replace it.
    /// </summary>
    /// <param name="vein">Vein to collect</param>
    /// <returns>True is collection was a success, otherwise false.</returns>
    private bool TryCollect(CoalVein vein)
    {
        if (vein == null) return false;

        if (state.coalInDepot >= config.depositMax)
        {
            Debug.Log("InteractionController: Deposit is full");
            OnNotificationToast?.Invoke(LocalizationSettings.StringDatabase.GetLocalizedString("depositFullNotification"), 3f);
            OnCollectCoal?.Invoke(false);
            return false;

        }
        else
        {
            OnCollectCoal?.Invoke(true);
        }


        veinsInRange.Remove(vein);

        if (closestVeinInRange == vein)
        {
            closestVeinInRange = null;
        }

        spawner.ReplaceVein(vein);

        Debug.Log("InteractionController: Collect was successful");
        return true;
    }

    /// <summary>
    /// Attempts to deposit at the nexus.
    /// If depot is empty, emits a notification toast; otherwise raises the deposit event and 
    /// shows a confirmation toast.
    /// </summary>
    /// <param name="nexus">Nexus to deposit</param>
    /// <returns>False if the deposit is empty; otherwise true</returns>
    private bool TryDeposit(Nexus nexus)
    {
        if (nexus == null) return false;

        if (state.coalInDepot <= 0)
        {
            Debug.Log("Deposito vacio");
            OnDepositCoal?.Invoke(false);
            OnNotificationToast?.Invoke(LocalizationSettings.StringDatabase.GetLocalizedString("depositEmptyNotification"), 3f);
            return false;

        }
        OnDepositCoal?.Invoke(true);
        Debug.Log("Deposito con exito");
        OnNotificationToast?.Invoke(LocalizationSettings.StringDatabase.GetLocalizedString("depositSuccessNotification"), 3f);
        return true;
    }

    /// <summary>
    /// Recalculates the closest vein and updates the interaction prompt.
    /// </summary>
    private void RefreshInteractionTarget()
    {
        closestVeinInRange = SelectClosestVein();

        SetInteractionVisible(closestVeinInRange != null);
    }

    /// <summary>
    /// Sets the interaction prompt visibility.
    /// Only raises the events when the state changes to prevent unnecesary HUD updates.
    /// </summary>
    /// <param name="visible">True to makes the interaction prompt visible, otherwise false.</param>
    private void SetInteractionVisible(bool visible)
    {
        if (visible == interactionVisible) return;
        interactionVisible = visible;
        OnShowInteraction?.Invoke(visible);
    }

}
