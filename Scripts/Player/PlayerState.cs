using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdleState : IPlayerState
{
    private FSM manager;
    public PlayerIdleState(FSM manager)
    {
        this.manager = manager;
    }

    public void Enter()
    {
        manager.animator.CrossFade("Player_Idle", 0.1f);
    }

    public void Update()
    {
        // 这里的先后顺序不再影响实际得分，只影响视觉表现
        if (PoseDetector.ConsumeAnimR())
        {
            manager.ChangeState(StateType.Atk_R);
        }
        else if (PoseDetector.ConsumeAnimL())
        {
            manager.ChangeState(StateType.Atk_L);
        }
    }
    public void Exit() { }
}

public class PlayerGuardState : IPlayerState
{
    private FSM manager;
    public PlayerGuardState(FSM manager)
    {
        this.manager = manager;
    }

    public void Enter()
    {
        manager.animator.CrossFade("Player_Guard", 0.1f);
    }

    public void Update()
    {
        AnimatorStateInfo stateInfo = manager.animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("Player_Guard") && stateInfo.normalizedTime >= 0.9f)
        {
            manager.ChangeState(StateType.Idle);
        }
    }
    public void Exit() { }
}

public class PlayerAtkRState : IPlayerState
{
    private FSM manager;
    public PlayerAtkRState(FSM manager)
    {
        this.manager = manager;
    }

    public void Enter()
    {
        // 放弃 CrossFade，高速连打时直接强制切入第 0 帧，打击感更硬朗
        manager.animator.Play("Player_AtkR", 0, 0f);
        // 判定代码已移走！
    }

    public void Update()
    {
        if (PoseDetector.ConsumeAnimL())
        {
            manager.ChangeState(StateType.Atk_L);
            return;
        }
        else if (PoseDetector.ConsumeAnimR())
        {
            manager.ChangeState(StateType.Atk_R);
            return;
        }

        AnimatorStateInfo stateInfo = manager.animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("Player_AtkR") && stateInfo.normalizedTime >= 0.8f)
        {
            manager.ChangeState(StateType.Idle);
        }
    }
    public void Exit() { }
}

public class PlayerAtkLState : IPlayerState
{
    private FSM manager;
    public PlayerAtkLState(FSM manager)
    {
        this.manager = manager;
    }

    public void Enter()
    {
        manager.animator.Play("Player_AtkL", 0, 0f);
        // 判定代码已移走！
    }

    public void Update()
    {
        if (PoseDetector.ConsumeAnimR())
        {
            manager.ChangeState(StateType.Atk_R);
            return;
        }
        else if (PoseDetector.ConsumeAnimL())
        {
            manager.ChangeState(StateType.Atk_L);
            return;
        }

        AnimatorStateInfo stateInfo = manager.animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("Player_AtkL") && stateInfo.normalizedTime >= 0.8f) // 缩短一点点后摇
        {
            manager.ChangeState(StateType.Idle);
        }
    }
    public void Exit() { }
}