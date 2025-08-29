using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class InteractionController : MonoBehaviour
{

    [SerializeField] private Transform player; // raíz de la excavadora (para medir distancia)
    [SerializeField] private VeinSpawner spawner;
    [SerializeField] private KeyCode collectKey = KeyCode.E;
    private CoalVein closestVeinInRange;
    private Nexus nexusInRange;
    private SessionState state;
    private GameConfig config;

    public event Action<string,float> OnNotificationToast;
    public event Action<bool> OnShowInteraction;
    public event Action OnCollectCoal;
    public event Action OnDepositCoal;

    private HashSet<CoalVein> veinsInRange = new HashSet<CoalVein>();

    public void SetSessionState(SessionState sessionState)
    {
        state = sessionState;
    }

    public void SetConfig(GameConfig gameConfig)
    {
        config = gameConfig;
    }

    private void Update()
    {
        if (Input.GetKeyDown(collectKey))
        {
            if (nexusInRange != null)
            {
                Debug.Log("Pulsado boton interaccion al lado de nexo");
                TryDeposit(nexusInRange);
                OnShowInteraction?.Invoke(false);

            }
            else if (closestVeinInRange != null)
            {
                Debug.Log("Pulsado boton interaccion al lado de veta");
                if (TryCollect(closestVeinInRange))
                {
                    RefreshInteractionTarget();
                }
            }
        }
    }


    //Called from CoalVein.cs
    public void NotifyVeinEntered(CoalVein v)
    {
        if (v == null) return;
        veinsInRange.Add(v);
        RefreshInteractionTarget();
    }

    public void NotifyVeinExited(CoalVein v)
    {
        if (v == null) return;
        veinsInRange.Remove(v);
        closestVeinInRange = SelectClosestVein();

        if (closestVeinInRange == v)
        {
            closestVeinInRange = null;
        }
        RefreshInteractionTarget();
    }

    //Called from Nexus.cs
    public void NotifyNexusEntered(Nexus nexus)
    {
        if (nexus == null || state == null) return;
        nexusInRange = nexus;
        if(state.coalInDepot > 0) OnShowInteraction?.Invoke(true);    
    }

    public void NotifyNexusExited(Nexus nexus)
    {
        if (nexus == null) return;
        nexusInRange = null;
        OnShowInteraction?.Invoke(false);
    }


    private CoalVein SelectClosestVein()
    {

        if (veinsInRange.Count == 0) return null;

        CoalVein closest = null;
        float closestDist = float.MaxValue;

        foreach (var item in veinsInRange)
        {
            if (item == null) continue;

            float dist = (item.transform.position - player.position).sqrMagnitude;
            if (dist < closestDist)
            {
                closest = item;
                closestDist = dist;
            }
        }

        return closest;
    }

    private bool TryCollect(CoalVein vein)
    {
        if (vein == null) return false;

        if (state.coalInDepot >= config.depositMax)
        {
            Debug.Log("Deposito lleno");
            OnNotificationToast?.Invoke("Deposito lleno. Entrega el carbón en la central", 3f);
            return false;

        }
        else
        {
            OnCollectCoal?.Invoke();
        }


        veinsInRange.Remove(vein);

        if (closestVeinInRange == vein)
        {
            closestVeinInRange = null;
        }

        spawner.ReplaceVein(vein);
        
        Debug.Log("Recogida con exito");
        return true;
    }

    private bool TryDeposit(Nexus nexus)
    {
        if (nexus == null) return false;

        if (state.coalInDepot <= 0)
        {
            Debug.Log("Deposito vacio");
            OnNotificationToast?.Invoke("Deposito vacio. Recoge carbon en la mina", 3f);
            return false;

        }
        OnDepositCoal?.Invoke();
        Debug.Log("Deposito con exito");
        OnNotificationToast?.Invoke("Carbon depositado! Añadiendo tiempo limite", 3f);
        return true;
    }

    private void RefreshInteractionTarget()
    {
        closestVeinInRange = SelectClosestVein();

        if (closestVeinInRange != null)
        {
            OnShowInteraction?.Invoke(true);
            return;
        }
        else
        {
            OnShowInteraction?.Invoke(false);
        }
    }


   
}
