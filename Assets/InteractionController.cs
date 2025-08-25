using System;
using System.Collections.Generic;
using UnityEngine;

public class InteractionController : MonoBehaviour
{

    [SerializeField] private Transform player; // raíz de la excavadora (para medir distancia)
    [SerializeField] private HUDController hud;

    [SerializeField] private VeinSpawner spawner;
    [SerializeField] private KeyCode collectKey = KeyCode.E;
    private CoalVein veinTarget;
    private Nexus nexusTarget;
    private SessionState state;
    private GameConfig config;

    private HashSet<CoalVein> veinsInRange = new HashSet<CoalVein>();

    public void SetSessionState(SessionState sessionState)
    {
        state = sessionState;
    }

    public void setConfig(GameConfig gameConfig)
    {
        config = gameConfig;
    }

    private void Update()
    {
        if (Input.GetKeyDown(collectKey))
        {
            if (nexusTarget != null)
            {
                Debug.Log("Pulsado boton interaccion al lado de nexo");
                TryDeposit(nexusTarget);
                hud.HideInteractionText();

            }
            else if (veinTarget != null)
            {
                Debug.Log("Pulsado boton interaccion al lado de veta");
                if (TryCollect(veinTarget))
                {
                    UpdateVein();
                    return;
                }
            }
        }
    }


    //Called from CoalVein.cs
    public void NotifyVeinEntered(CoalVein v)
    {
        if (v == null) return;
        veinsInRange.Add(v);
        UpdateVein();
    }

    public void NotifyVeinExited(CoalVein v)
    {
        if (v == null) return;
        veinsInRange.Remove(v);
        veinTarget = SelectClosestVein();

        if (veinTarget == v)
        {
            veinTarget = null;
        }
        UpdateVein();
    }

    //Called from Nexus.cs
    public void NotifyNexusEntered(Nexus nexus)
    {
        if (nexus == null) return;
        nexusTarget = nexus;
        if(state.coalInDepot > 0) hud.ShowInteractionText("Pulsa E para depositar");    
    }

    public void NotifyNexusExited(Nexus nexus)
    {
        if (nexus == null) return;
        nexusTarget = null;
        hud.HideInteractionText();
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
            hud.ShowNotificationText("Deposito lleno", 3f);
            return false;

        }
        else
        {
            state.coalInDepot++;
            hud.setCoalText(state.coalInDepot);
        }


        veinsInRange.Remove(vein);
        //Destroy(vein.gameObject);
        spawner.ReplaceVein(vein.gameObject);

        if (veinTarget == vein)
        {
            veinTarget = null;
        }

        Debug.Log("Recogida con exito");

        return true;
    }

    private bool TryDeposit(Nexus nexus)
    {
        if (nexus == null) return false;

        if (state.coalInDepot <= 0)
        {
            Debug.Log("Deposito vacio");
            hud.ShowNotificationText("Deposito vacio, recoge carbon por el escenario", 3f);
            return false;

        }
        state.coalInDepot = 0;
        hud.setCoalText(state.coalInDepot);
        Debug.Log("Deposito con exito");

        return true;
    }

    private void UpdateVein()
    {
        veinTarget = SelectClosestVein();

        if (veinTarget != null)
        {
            hud.ShowInteractionText("Pulsa E para recoger");
            return;
        }
        else
        {
            hud.HideInteractionText();
        }
    }


   
}
