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
    public float maxHealth = 30;
    public float curHealth = 30;

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
    bool bomb;
    public GameObject bombCoolTime;
    //float bombTime = 3f;

    public Image shield_CoolTime;
    bool shield;
    public GameObject shieldCoolTime;
    //float shieldTime = 5f;

    // 플레이어 체력
    public Slider hp_Slider;

    CharacterController cc;
    Animator anim;
    Weapon weapon;
    GameObject gameManager;

    private void Awake()
    {
        gameManager = GameObject.Find("GameManager");
        bomb_CoolTime = gameManager.GetComponent<GameManager>().bomb_CoolTime;
        bombCoolTime = gameManager.GetComponent<GameManager>().bombCoolTime;
        shield_CoolTime = gameManager.GetComponent<GameManager>().shield_CoolTime;
        shieldCoolTime = gameManager.GetComponent<GameManager>().shieldCoolTime;
        hp_Slider = gameManager.GetComponent<GameManager>().hp_Slider;
        //bombCoolTime = GameManager.Instance.bombCoolTime;
        //shield_CoolTime = GameManager.Instance.shield_CoolTime;
        //shieldCoolTime = GameManager.Instance.shieldCoolTime;
        //hp_Slider = GameManager.Instance.hp_Slider;

        cc = gameObject.GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        weapon = GetComponentInChildren<Weapon>();        
    }

    private void Start()
    {
        hp_Slider.value = (float)curHealth / (float)maxHealth;        
        bombCoolTime.SetActive(false);
        shieldCoolTime.SetActive(false);
        //bombCoolTime.enabled = false;
        //shieldCoolTime.enabled = false;
        bomb_CoolTime.fillAmount = 1;
        bomb = false;
        bomb_CoolTime.enabled = false;
        shield_CoolTime.fillAmount = 1;
        shield = false;
        shield_CoolTime.enabled = false;
        effect.SetActive(false);
    }

    private void Update()
    {
        GetInput();
        Move();
        Attack();
        Die();
        HandleHp();
        ThrowBomb();
        Defend();
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Sword")
        {
            curHealth -= 10f;
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

    void HandleHp()
    {
        hp_Slider.value = (float)curHealth / (float)maxHealth;
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
            bomb_CoolTime.fillAmount = 1;
            StartCoroutine(LightCoolTime());
            GameObject instantBomb = Instantiate(grenadeObj, firePos.transform.position,
                firePos.transform.rotation);
            Rigidbody rigidBomb = instantBomb.GetComponent<Rigidbody>();
            rigidBomb.AddForce(firePos.transform.forward * 13f, ForceMode.Impulse);
        }        
    }
    IEnumerator LightCoolTime()
    {
        while(bomb_CoolTime.fillAmount > 0)
        {
            bomb_CoolTime.fillAmount -= 1 * Time.smoothDeltaTime / 3f;
            yield return null;
            bomb = true;
        }
        bomb = false;
        bomb_CoolTime.enabled = false;
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
        if (dDown && stern == false && isDie == false && shield == false)
        {
            //shieldCoolTime.enabled = true;
            shieldCoolTime.SetActive(true);
            shield = true;
            shield_CoolTime.enabled = true;
            shield_CoolTime.fillAmount = 1;
            StartCoroutine(DefendCoolTime());
            UseS();
            scol.enabled = true;
            anim.SetTrigger("Guard");
            //StartCoroutine(DCoolTimeText());
        }        
    }
    IEnumerator DefendCoolTime()
    {
        while (shield_CoolTime.fillAmount > 0)
        {            
            shield_CoolTime.fillAmount -= 1 * Time.smoothDeltaTime / 5f;
            yield return null;
            shield = true;            
        }
        shield = false;
        shield_CoolTime.enabled = false;        
    }

    void UseS()
    {
        StartCoroutine(ShieldTime());        
    }
    IEnumerator ShieldTime()
    {
        scol.enabled = true;
        effect.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        effect.SetActive(false);
        scol.enabled = false;
    }
    //IEnumerator DCoolTimeText()
    //{
    //    while (shieldTime > 0)
    //    {
    //        yield return new WaitForSeconds(1.0f);
    //        shieldTime -= 1.0f;
    //        //shieldCoolTime.text = "" + shieldTime;
    //        shieldCoolTime.GetComponent<Text>().text = "" + shieldTime;
    //    }

    //    //shieldCoolTime.enabled = false;        
    //    shieldCoolTime.SetActive(false);
    //    yield break;
    //}
}
