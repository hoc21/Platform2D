using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;
    private Collider2D coll;
    public float speed = 5f;
    public float jumpForce = 10f;
    public int cherries = 0;
    public Text cherriesText;
    [SerializeField] private LayerMask ground; 
    public float hurtForce = 10f;
    public int health;
    public Text healthAmount;
    private enum State{idle,running,jumping,falling,hurt}
    private State state = State.idle;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        coll = GetComponent<Collider2D>();
        healthAmount.text = health.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        if(state != State.hurt)
        {
            Movement();
        }
        
        AnimationState();
        anim.SetInteger("state",(int)state);
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.CompareTag("Collectable"))
        {
            Destroy(other.gameObject);
            cherries ++;
            cherriesText.text = "Cherries : " + cherries;
        }
        if(other.CompareTag("PowerUp"))
        {
            Destroy(other.gameObject);
            jumpForce = 30f;
            speed = 15f;
            GetComponent<SpriteRenderer>().color = Color.red;
            StartCoroutine(ResetPower());
        }
    }
    void OnCollisionEnter2D(Collision2D other)
    {
        if(other.gameObject.CompareTag("Enemy"))
        {
            Enemy enemy = other.gameObject.GetComponent<Enemy>();
            if(state == State.falling)
            {
                enemy.JumpedOn();
                Debug.Log(state);
            }
            else
            {
                state = State.hurt;
                HandleHealth();
                if (other.gameObject.transform.position.x > transform.position.x)
                {
                    rb.velocity = new Vector2(-hurtForce, rb.velocity.y);
                }
                else
                {
                    rb.velocity = new Vector2(hurtForce, rb.velocity.y);
                }
            }
        }
    }

    private void HandleHealth()
    {
        health -= 1;
        healthAmount.text = health.ToString();
        if (health <= 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void Movement()
    {
        float hDirection = Input.GetAxis("Horizontal");
        if(hDirection<0)
        {
            rb.velocity = new Vector2(-speed,rb.velocity.y);
            transform.localScale = new Vector2(-1,1);
            
        }
        else if(hDirection>0)
        {
            rb.velocity = new Vector2(speed,rb.velocity.y);
            transform.localScale = new Vector2(1,1);
            
        }
        if(Input.GetButtonDown("Jump") && coll.IsTouchingLayers(ground))
        {
            rb.velocity = new Vector2(rb.velocity.x,jumpForce);
            state = State.jumping;
        }
    }
    private void AnimationState()
    {
        if(state == State.jumping)
        {
            if(rb.velocity.y<.1f)
            {
                state = State.falling;
            }
        }
        else if(state == State.falling)
        {
            if(coll.IsTouchingLayers(ground))
            {
                state = State.idle;
            }
        }
        else if(state == State.hurt)
        {
            if(Mathf.Abs(rb.velocity.x) <.1f)
            {
                state = State.idle;
            }
        }

        else if(Mathf.Abs(rb.velocity.x) > 2f)
        {
            state = State.running;
        }
        else
        {
            state = State.idle;
        }
    }
    private IEnumerator ResetPower()
    {
        yield return new WaitForSeconds(10);
        jumpForce = 25f;
        speed = 10f;
        GetComponent<SpriteRenderer>().color = Color.white;
    }
}
