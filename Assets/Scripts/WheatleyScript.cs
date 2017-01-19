using UnityEngine;
using System.Collections;

public class WheatleyScript : MonoBehaviour
{
    public Transform target;
    private float speed = 5f;

    public int state = -1;

    private int num = 0;
    private int pos = 0;
    private Vector3 randomPos;
    public float randomDistance;
    public GameObject scannerRot;
    public Animator anim;
    Game game;
    // Use this for initialization
    void Start()
    {
         game = Game.getInstance();
         randomPosition();
    }

    private void randomPosition()
    {
        Vector3 p = Camera.main.transform.position;
        randomPos= new  Vector3(
            Random.Range(p.x - 2, p.x + 2),
            Random.Range(p.y - 0.5f, p.y + 1),
            Random.Range(p.z - 2, p.z + 2));
        state = -1;
       randomDistance = Vector3.Distance(transform.position, randomPos);
    }
    // Update is called once per frame
    void Update()
    {
        //Up Down Movement on Position;
        if (state == -2)
        {
            float distance = Vector3.Distance(transform.position, randomPos);
            var targetRotation = Quaternion.LookRotation(Camera.main.transform.position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * speed);

        }
        // Random Movment
        else if (state == -1)
        {
            float distance = Vector3.Distance(transform.position, randomPos);
            if (distance < 0.1f)
            {
                Invoke("randomPosition", Random.Range(1, 15));
                state = -2;
            }
            float step = Time.deltaTime *distance*(randomDistance-distance+0.1f);
            var targetRotation = Quaternion.LookRotation(Camera.main.transform.position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * speed);
            transform.position = Vector3.MoveTowards(transform.position, randomPos, step);

        }
        //Find ScanningTarget
        else if (state == 0)
        {
            CancelInvoke("randomPosition");
            num = game.getTestItems().Count;
            if (num < 1)
            {
                state = -1;
                return;
            }
            GameObject obj = game.getGameObjectFor(game.getTestItems()[pos]);
            if (obj == null) return;
            target = obj.transform;
            Vector3 targetPosition = target.position + new Vector3(0, 1, 0);
            float distance = Vector3.Distance(transform.position, targetPosition);
            if (randomDistance == -1) randomDistance = distance;
            float step = distance * Time.deltaTime * (randomDistance - distance + 0.1f);

            var targetRotation = Quaternion.LookRotation(target.position - transform.position);

            // Smoothly rotate towards the target point.
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * speed);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);
            if (distance < 0.1f)
            {
                scannerRot.SetActive(true);
                anim.SetBool("isScanning", true);
                state++;
            }
        }
        else if (state == 1)

        {
            
            if (num == 2 && pos == 0 && game.testProgress > 40)
            {
                pos++;
                randomDistance = -1;
                state = 0;
                scannerRot.SetActive(false);
                anim.SetBool("isScanning", false);
            }
            else if (game.testProgress > 80)
            {
                randomPosition();
                scannerRot.SetActive(false);
                anim.SetBool("isScanning", false);
            }



        }

    }
}
