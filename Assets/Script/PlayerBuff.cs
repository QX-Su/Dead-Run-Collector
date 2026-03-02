using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerCharacterController))]
public class PlayerBuffs : MonoBehaviour
{
    PlayerCharacterController ctrl;

    float baseWalk;
    float baseSprint;
    float baseJump;

    Coroutine buffRoutine;

    void Awake()
    {
        ctrl = GetComponent<PlayerCharacterController>();

        // 记录基础值（只在游戏开始读一次）
        baseWalk = ctrl.walkSpeed;
        baseSprint = ctrl.sprintSpeed;
        baseJump = ctrl.jumpHeight;
    }

    // 速度&跳跃翻倍（闪电）
    public void ApplyThunder(float duration)
    {
        ApplyBuff(
            walkMul: 2f,
            sprintMul: 2f,
            jumpMul: 2f,
            duration: duration
        );
    }

    // 速度减半（毒素）
    public void ApplyPoison(float duration)
    {
        ApplyBuff(
            walkMul: 0.5f,
            sprintMul: 0.5f,
            jumpMul: 1f,
            duration: duration
        );
    }

    void ApplyBuff(float walkMul, float sprintMul, float jumpMul, float duration)
    {
        // 覆盖式：后吃到的效果替换前一个（最简单、最不乱）
        if (buffRoutine != null) StopCoroutine(buffRoutine);
        buffRoutine = StartCoroutine(BuffTimer(walkMul, sprintMul, jumpMul, duration));
    }

    IEnumerator BuffTimer(float walkMul, float sprintMul, float jumpMul, float duration)
    {
        ctrl.walkSpeed = baseWalk * walkMul;
        ctrl.sprintSpeed = baseSprint * sprintMul;
        ctrl.jumpHeight = baseJump * jumpMul;

        yield return new WaitForSeconds(duration);

        // 恢复
        ctrl.walkSpeed = baseWalk;
        ctrl.sprintSpeed = baseSprint;
        ctrl.jumpHeight = baseJump;

        buffRoutine = null;
    }
}