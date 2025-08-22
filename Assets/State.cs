using Unity.Netcode;
using UnityEngine;

public class State : NetworkBehaviour
{

    [HideInInspector] public Game game;


    public virtual void OnStateEnter()
    {

    }

    public virtual void OnReset()
    {
        
    }


    void Update()
    {
        OnStateUpdate();
    }

    public virtual void OnStateUpdate()
    {

    }

    public virtual void OnStateExit()
    {
        
    }
}
