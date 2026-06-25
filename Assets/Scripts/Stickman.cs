using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Stickman : MonoBehaviour
{
    public float jumpForce = 5f;
    private Rigidbody rb;
    public bool isGrounded;
    private Animator anim;
    private GameObject Stopper;
    public List<GameObject> cubes;
    public GameObject[] impact_fx;
    public GameObject[] player_Body; 
    public bool impact = false;
    public GameObject canvas;
    private bool processingOnit = false;
    public  GameObject[] dead_Fx;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        Stopper = GameObject.FindWithTag("Use");
        dead_Fx[2].SetActive(true);
    }

    void Update()
    {
       
        if (Input.GetMouseButtonDown(0))
        {
            Jump();
            impact = true;
        }

        if (impact == false)
        {
            foreach (GameObject fx in impact_fx)
                fx.SetActive(false);
        }

        if (isGrounded == true && impact == true)
            StartCoroutine(Runfx());
    }

    IEnumerator Runfx()
    {
        foreach (GameObject fx in impact_fx)
            fx.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        impact = false;
    }

    void Jump()
    {
        if (isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            anim.SetTrigger("Jump");
            isGrounded = false;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Onit" && !processingOnit)
        {
            processingOnit = true;
            isGrounded = true;

            GameObject Hurdel = other.transform.parent.gameObject;
            cubes.Add(Hurdel);

            GameObject OnDie = GameObject.FindWithTag("Kill");
            if (OnDie != null)
                OnDie.SetActive(false);

            Hurdel.GetComponent<Move>().IsMove = false;

            Stack stackComp = Stopper.GetComponent<Stack>();
            if (stackComp != null && stackComp.canStack)
            {
                Transform ChangeY = stackComp.Stacker[1].transform;

                // Read which side this prefab came from so the offset is correct
                PrefabDirection dirComp = Hurdel.GetComponent<PrefabDirection>();
                bool fromRight = dirComp != null ? dirComp.comingFromRight : true;

                // Prefab came from RIGHT → it approached from the right side → offset left  (-0.4)
                // Prefab came from LEFT  → it approached from the left  side → offset right (+0.4)
                float xOffset = fromRight ? -0.4f : 0.4f;

                ChangeY.position = new Vector3(
                    Hurdel.transform.position.x + xOffset,
                    ChangeY.position.y,
                    ChangeY.position.z
                );

                stackComp.Stacked();
            }

            other.gameObject.SetActive(false);
            StartCoroutine(ResetOnitGuard());
        }

        if (other.tag == "Kill")
        {
            GameObject[] foundObjects = GameObject.FindGameObjectsWithTag("Stack");
            this.GetComponent<CapsuleCollider>().enabled = false;
            rb.constraints = RigidbodyConstraints.None;
            foreach (GameObject c in foundObjects)
            {
                c.GetComponent<Move>().IsMove = false;
                cubes.Add(c);
            }
     
            SaveHighScore();
            jumpForce=0f;
            foreach (GameObject b in player_Body)
            {
                dead_Fx[0].SetActive(true);
                dead_Fx[1].SetActive(true);
                b.SetActive(false);
            }
            StartCoroutine(loadScene());
        }
    }

 
    IEnumerator ResetOnitGuard()
    {
        yield return new WaitForSeconds(0.6f);
        processingOnit = false;
    }

    public void SaveHighScore()
    {
        int currentHighScore = PlayerPrefs.GetInt("HighScore", 0);
        if (canvas.GetComponent<Points>().score > currentHighScore)
        {
            PlayerPrefs.SetInt("HighScore", canvas.GetComponent<Points>().score);
            PlayerPrefs.Save();
        }
    }

    IEnumerator loadScene()
    {
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezePosition;
        yield return new WaitForSeconds(2.5f);
        SceneManager.LoadScene("Game");
    }
}
