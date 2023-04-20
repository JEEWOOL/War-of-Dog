using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    // 플레이어 이동
    public float playerMoveSpeed = 5;
    float hAxis;
    float vAxis;
    bool rDown;    

    // 플레이어 점프
    public float gravity = -10;
    float yVelocity = 0;
    public float jumpPower = 3f;
    public bool isJumping = false;

    // 플레이어 공격
    bool aDown;
    float attackDelay;
    bool isAttackReady = true;

    // 플레이어 스턴
    bool stern = false;

    // 플레이어 섬광
    public int hasBomb = 3;
    bool bDown;
    public GameObject grenadeObj;
    public GameObject firePos;

    CharacterController cc;
    Animator anim;
    Weapon weapon;

    private void Awake()
    {
        cc = gameObject.GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        weapon = GetComponentInChildren<Weapon>();
    }

    private void Update()
    {
        GetInput();
        Move();
        Attack();
        ThrowBomb();
    }    

    void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        rDown = Input.GetKey(KeyCode.LeftShift);
        aDown = Input.GetMouseButtonDown(0);
        bDown = Input.GetMouseButtonDown(1);
    }

    void Move()
    {
        Vector3 dir = new Vector3(hAxis, 0, vAxis).normalized;

        anim.SetBool("isWalk", dir != Vector3.zero);
        anim.SetBool("isRun", rDown);

        dir = Camera.main.transform.TransformDirection(dir);

        if (cc.collisionFlags == CollisionFlags.Below)
        {
            if (isJumping)
            {
                isJumping = false;
                yVelocity = 0;
            }
        }

        if (Input.GetButtonDown("Jump") && !isJumping)
        {
            yVelocity = jumpPower;
            isJumping = true;
        }

        yVelocity += gravity * Time.deltaTime;
        dir.y = yVelocity;

        if(!isAttackReady || stern == true)
        {
            dir = Vector3.zero;
        }

        cc.Move(dir * playerMoveSpeed * (rDown ? 1.5f : 1f) * Time.deltaTime);
    }

    void Attack()
    {
        attackDelay += Time.deltaTime;
        isAttackReady = weapon.rate < attackDelay;
        if (aDown && isAttackReady && stern == false)
        {
            weapon.Use();
            anim.SetTrigger("Attack");
            attackDelay = 0;
        }
    }

    void ThrowBomb()
    {
        if (hasBomb == 0)
        {
            return;
        }

        if (bDown /*&& !isAttackReady*/)
        {
            GameObject instantBomb = Instantiate(grenadeObj, firePos.transform.position,
                firePos.transform.rotation);
            Rigidbody rigidBomb = instantBomb.GetComponent<Rigidbody>();
            rigidBomb.AddForce(firePos.transform.forward * 13f, ForceMode.Impulse);

            hasBomb--;
        }
    }
}
