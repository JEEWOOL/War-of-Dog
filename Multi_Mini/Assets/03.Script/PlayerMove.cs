using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMove : MonoBehaviour
{
    // 플레이어 이동 / 체력
    public float playerMoveSpeed = 5;
    float hAxis;
    float vAxis;
    bool rDown;
    public int maxHealth = 30;
    public int curHealth = 30;

    // 플레이어 점프
    public float gravity = -10;
    float yVelocity = 0;
    public float jumpPower = 3f;
    public bool isJumping = false;

    // 플레이어 공격
    bool aDown;
    float attackDelay;
    bool isAttackReady = true;

    // 플레이어 스턴 / 사망
    public bool stern = false;
    public bool isDie = false;

    // 플레이어 섬광    
    bool bDown;
    public GameObject grenadeObj;
    public GameObject firePos;

    // 플레이어 방어
    public GameObject effect;
    public BoxCollider scol;
    bool dDown;

    // 플레이어 스킬 쿨타임
    public Image bomb_CoolTime;
    //float bombdelay = 3f;
    bool bomb = false;

    public Image shield_CoolTime;

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
        Defend();
        Die();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Sword")
        {
            curHealth -= 10;
            anim.SetTrigger("damage");
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Bomb" || collision.gameObject.tag == "Shield")
        {
            StartCoroutine(BombAttack());
        }
    }
    IEnumerator BombAttack()
    {
        stern = true;
        anim.SetTrigger("Hit");
        yield return new WaitForSeconds(2f);
        stern = false;
    }

    void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        rDown = Input.GetKey(KeyCode.LeftShift);
        aDown = Input.GetMouseButtonDown(0);
        bDown = Input.GetMouseButtonDown(1);
        dDown = Input.GetKeyDown(KeyCode.E);
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

        if (Input.GetButtonDown("Jump") && !isJumping && stern == false && isDie == false)
        {
            yVelocity = jumpPower;
            isJumping = true;
        }

        yVelocity += gravity * Time.deltaTime;
        dir.y = yVelocity;

        if(!isAttackReady || stern == true || isDie == true)
        {
            dir = Vector3.zero;
        }

        cc.Move(dir * playerMoveSpeed * (rDown ? 1.5f : 1f) * Time.deltaTime);
    }

    void Attack()
    {
        attackDelay += Time.deltaTime;
        isAttackReady = weapon.rate < attackDelay;
        if (aDown && isAttackReady && stern == false && isDie == false)
        {
            weapon.Use();
            anim.SetTrigger("Attack");
            attackDelay = 0;
        }
    }

    void ThrowBomb()
    {
        if (bDown && stern == false && isDie == false && bomb == false)
        {
            bomb = true;
            bomb_CoolTime.enabled = true;
            GameObject instantBomb = Instantiate(grenadeObj, firePos.transform.position,
                firePos.transform.rotation);
            Rigidbody rigidBomb = instantBomb.GetComponent<Rigidbody>();
            rigidBomb.AddForce(firePos.transform.forward * 13f, ForceMode.Impulse);
        }        
    }    

    void Die()
    {
        if(curHealth <= 0)
        {
            isDie = true;
            anim.SetBool("Die", true);
            Destroy(this.gameObject, 3f);
        }
    }

    void Defend()
    {
        if (dDown && stern == false && isDie == false)
        {
            anim.SetTrigger("Guard");
            StopCoroutine(DefendS());
            StartCoroutine(DefendS());
        }
    }
    IEnumerator DefendS()
    {
        effect.SetActive(true);
        scol.enabled = true;
        yield return new WaitForSeconds(0.5f);
        effect.SetActive(false);
        scol.enabled = false;
    }
}
