using System.Collections;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class PlayerMovementController : MonoBehaviour
{
    [SerializeField] private SteamVR_Action_Boolean moveUpAction;
    [SerializeField] private SteamVR_Action_Boolean moveDownAction;
    [SerializeField] private SteamVR_Action_Vector2 moveAction;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private float verticalSpeed = 10.0f;
    [SerializeField] private float speed = 1;
    [SerializeField] private float gravity = -9.81f;

    private bool canMoveVertically = false;
    private float verticalVelocity = 0;  // 添加垂直速度变量

    private void Update()
    {
        Move();
        HandleVerticalMovement();
        ApplyGravity();  // 添加重力处理
    }

    private void Move()
    {
        Vector3 direction = Player.instance.hmdTransform.TransformDirection(new Vector3(moveAction.axis.x, 0, moveAction.axis.y));
        characterController.Move(speed * Time.deltaTime * Vector3.ProjectOnPlane(direction, Vector3.up));
    }

    private void HandleVerticalMovement()
    {
        if (canMoveVertically)
        {
            if (moveUpAction.GetState(SteamVR_Input_Sources.Any))
            {
                verticalVelocity = verticalSpeed;
                characterController.Move(Vector3.up * verticalSpeed * Time.deltaTime);
            }
            else if (moveDownAction.GetState(SteamVR_Input_Sources.Any))
            {
                verticalVelocity = -verticalSpeed;
                characterController.Move(Vector3.down * verticalSpeed * Time.deltaTime);
            }
        }
    }

    // 新添加的重力处理方法
    private void ApplyGravity()
    {
        if (!canMoveVertically)  // 只在不能垂直移动时应用重力
        {
            verticalVelocity += gravity * Time.deltaTime;
            Vector3 gravityMove = new Vector3(0, verticalVelocity * Time.deltaTime, 0);
            characterController.Move(gravityMove);

            // 如果角色已经在地面上，重置垂直速度
            if (characterController.isGrounded && verticalVelocity < 0)
            {
                verticalVelocity = 0;
            }
        }
    }

    public void SetCanMoveVertically(bool canMove)
    {
        canMoveVertically = canMove;
        if (canMove)
        {
            verticalVelocity = 0;  // 重置垂直速度当切换到可以垂直移动时
        }
        Debug.Log("Set canMoveVertically to: " + canMove);
    }
}