using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements.Experimental;


public enum StateType
{
    Idle,
    Guard,
    Atk_R,
    Atk_L,
}
public class FSM_Parameter
{
    public int guardPoint;
    public int playerScore;
}
public class FSM_AnimationNames { }
public class FSM_SoundEffects { }

public class FSM : MonoBehaviour
{
    public GameObject player;

    public FSM_Parameter parameter;
    public FSM_AnimationNames animationNames;
    public FSM_SoundEffects soundEffects;

    public Animator animator;


    public IPlayerState currentState;
    public Dictionary<StateType,IPlayerState> states =new Dictionary<StateType, IPlayerState>();

    void Start()
    {
        states.Add(StateType.Idle, new PlayerIdleState(this));
        states.Add(StateType.Guard, new PlayerGuardState(this));
        states.Add(StateType.Atk_R, new PlayerAtkRState(this));
        states.Add(StateType.Atk_L, new PlayerAtkLState(this));
        animator = player.GetComponent<Animator>();
        currentState = states[StateType.Idle];
    }

    // Update is called once per frame
    void Update()
    {
        currentState.Update();
    }

    public void ChangeState(StateType state)
    {
        if (currentState != null)
        {
            currentState.Exit();
        }
        currentState = states[state];
        currentState.Enter();
    }

}
