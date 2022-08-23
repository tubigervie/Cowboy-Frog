using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
[DisallowMultipleComponent]
public class PlayerControl : MonoBehaviour
{
    #region Tooltip
    [Tooltip("MovementDetailsSO scriptable object containing movement details such as speed")]
    #endregion
    [SerializeField] private MovementDetailsSO movementDetails;

    private Player player;
    private bool leftMouseDownPreviousFrame = false;
    private int currentWeaponIndex = 1;
    private float moveSpeed;
    private Coroutine playerRollCoroutine;
    private WaitForFixedUpdate waitForFixedUpdate; //dealing with physics only move with fixed update intervals
    private bool isPlayerRolling = false;
    private float playerRollCooldownTimer = 0f;

    private void Awake()
    {
        player = GetComponent<Player>();
        moveSpeed = movementDetails.moveSpeed;
    }

    private void Start()
    {
        waitForFixedUpdate = new WaitForFixedUpdate();
        SetStartingWeapon();
    }

    private void Update()
    {
        if (isPlayerRolling) return;

        //Process player movement input
        MovementInput();

        //Process player weapon input
        WeaponInput();

        PlayerRollCooldownTimer();
    }

    private void PlayerRollCooldownTimer()
    {
        if(playerRollCooldownTimer >= 0f)
           playerRollCooldownTimer -= Time.deltaTime;
    }

    private void WeaponInput()
    {
        Vector3 weaponDirection;
        float weaponAngleDegrees, playerAngleDegrees;
        AimDirection playerAimDirection;

        AimWeaponInput(out weaponDirection, out weaponAngleDegrees, out playerAngleDegrees, out playerAimDirection);

        FireWeaponInput(weaponDirection, weaponAngleDegrees, playerAngleDegrees, playerAimDirection);

        SwitchWeaponInput();

        ReloadWeaponInput();
    }

    private void SwitchWeaponInput()
    {
        if(Input.mouseScrollDelta.y < 0f)
        {
            PreviousWeapon();
        }
        else if(Input.mouseScrollDelta.y > 0f)
        {
            NextWeapon();
        }
        else if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetWeaponByIndex(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetWeaponByIndex(2);
        }
    }

    private void NextWeapon()
    {
        currentWeaponIndex++;
        if(currentWeaponIndex > player.weaponList.Count)
        {
            currentWeaponIndex = 1;
        }
        SetWeaponByIndex(currentWeaponIndex);
    }

    private void PreviousWeapon()
    {
        currentWeaponIndex--;
        if (currentWeaponIndex < 1)
        {
            currentWeaponIndex = player.weaponList.Count;
        }
        SetWeaponByIndex(currentWeaponIndex);
    }

    private void ReloadWeaponInput()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            Weapon currentWeapon = player.activeWeapon.GetCurrentWEapon();
            if (currentWeapon.isWeaponReloading) return;
            if (currentWeapon.weaponRemainingAmmo <= 0 && !currentWeapon.weaponDetails.hasInfiniteAmmo) return;
            if (currentWeapon.weaponClipRemainingAmmo == currentWeapon.weaponDetails.weaponClipAmmoCapacity) return;
            Debug.Log("Call reload");
            player.reloadWeaponEvent.CallReloadWeaponEvent(player.activeWeapon.GetCurrentWEapon(), 0);
        }
    }

    private void FireWeaponInput(Vector3 weaponDirection, float weaponAngleDegrees, float playerAngleDegrees, AimDirection playerAimDirection)
    {
        if(Input.GetMouseButton(0))
        {
            Weapon currentWeapon = player.activeWeapon.GetCurrentWEapon();
            if (!currentWeapon.weaponDetails.isAutomatic && leftMouseDownPreviousFrame) return;
            player.fireWeaponEvent.CallSetFireWeaponEvent(true, leftMouseDownPreviousFrame, playerAimDirection, playerAngleDegrees, weaponAngleDegrees, weaponDirection);
            leftMouseDownPreviousFrame = true;
        }
        else {
            leftMouseDownPreviousFrame = false;
        }
    }

    private void SetStartingWeapon()
    {
        int index = 1;
        foreach(Weapon weapon in player.weaponList)
        {
            if(weapon.weaponDetails == player.playerDetails.startingWeapon)
            {
                SetWeaponByIndex(index);
                break;
            }
            index++;
        }
    }

    private void SetWeaponByIndex(int index)
    {
        if(index - 1 < player.weaponList.Count)
        {
            currentWeaponIndex = index;
            player.setActiveWeaponEvent.CallSetActiveWeaponEvent(player.weaponList[currentWeaponIndex - 1]);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        StopPlayerRollRoutine();
    }

    private void StopPlayerRollRoutine()
    {
        if(playerRollCoroutine != null)
        {
            StopCoroutine(playerRollCoroutine);
            isPlayerRolling = false;
        }
    }

    private void AimWeaponInput(out Vector3 weaponDirection, out float weaponAngleDegrees, out float playerAngleDegrees, out AimDirection playerAimDirection)
    {
        Vector3 mouseWorldPosition = HelperUtilities.GetMouseWorldPosition();

        weaponDirection = (mouseWorldPosition - player.activeWeapon.GetShootPosition());

        Vector3 playerDirection = (mouseWorldPosition - transform.position);

        weaponAngleDegrees = HelperUtilities.GetAngleFromVector(weaponDirection);

        playerAngleDegrees = HelperUtilities.GetAngleFromVector(playerDirection);

        playerAimDirection = HelperUtilities.GetAimDirection(playerAngleDegrees);

        player.aimWeaponEvent.CallAimWeaponEvent(playerAimDirection, playerAngleDegrees, weaponAngleDegrees, weaponDirection);
    }

    private void MovementInput()
    {
        float horizontalMovement = Input.GetAxisRaw("Horizontal");
        float verticalMovement = Input.GetAxisRaw("Vertical");
        bool rightMouseButtonDown = Input.GetMouseButtonDown(1);

        Vector2 direction = new Vector2(horizontalMovement, verticalMovement);

        //Adjust distance for diagonal movement
        if(horizontalMovement != 0f && verticalMovement != 0f)
        {
            direction *= 0.7f;
        }

        if(direction != Vector2.zero)
        {
            //Paintable room = GameManager.Instance.GetCurrentRoom().instantiatedRoom.paintMap;
            //float variableMoveSpeed = moveSpeed;
            //if(room != null && room.TileIsPainted(transform.position))
            //{
            //    variableMoveSpeed *= 1.5f;
            //}
            if(!rightMouseButtonDown)
                player.movementByVelocityEvent.CallMovementByVelocityEvent(direction, moveSpeed);
            else if (playerRollCooldownTimer <= 0f)
            {
                PlayerRoll((Vector3) direction);
            }
        }
        else
            player.idleEvent.CallIdleEvent();
    }

    private void PlayerRoll(Vector3 direction)
    {
        playerRollCoroutine = StartCoroutine(PlayerRollRoutine(direction));
    }

    private IEnumerator PlayerRollRoutine(Vector3 direction)
    {
        // minDistance used to decide when to exit coroutine loop
        float maxExitTime = .4f;
        float minDistance = 0.2f;
        isPlayerRolling = true;
        Vector3 targetPosition = player.transform.position + (Vector3)direction * movementDetails.rollDistance;
        while(Vector3.Distance(player.transform.position, targetPosition) > minDistance)
        {
            if (maxExitTime < 0f)
            {
                StopPlayerRollRoutine();
                yield return waitForFixedUpdate;
            }
            maxExitTime -= Time.fixedDeltaTime;
            player.movementToPositionEvent.CallMovementToPositionEvent(targetPosition, player.transform.position, movementDetails.rollSpeed,
                direction, isPlayerRolling);
            yield return waitForFixedUpdate;
        }
        isPlayerRolling = false;

        playerRollCooldownTimer = movementDetails.rollCooldownTime;
        player.transform.position = targetPosition;
    }
}
