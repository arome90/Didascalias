//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using TMPro;

//public class TextMeshProScript : MonoBehaviour
//{

//    public const string STAND_UP_ANIM = "Stand Up";
//    public const string SITTING_IDLE_ANIM = "Sitting";
//    public const string STANDING_IDLE_ANIM = "Standing";
//    public const string SIT_DOWN_ANIM = "Sit Down";
//    public const string CHEERING_ANIM = "Cheering";

//    public const string DISTRAIDO_STATE = "Distraido";
//    public const string CHEERING_STATE = "Cheering";
//    public const string SITTING_STATE = "Sitting";




//    public Transform cam;
//    public Animator anim;

//    // Tiempo en el que la animación de levantarse empieza a elevar la altura del personaje
//    float levantar = 0.23f;

//    // Tiempo en el que la animación de sentarse (levantarse al revés) ya ha bajado del todo la altura del personaje
//    float terminaSentar = 2.01f;

//    // Tiempo en el que la animación de sentarse (levantarse al revés) empieza a bajar la altura del personaje
//    float empiezaSentar = 0.9f;

//    float timeSinceStandUp = 0f;
//    float timeSinceSitDown = 0f;

//    private void Start()
//    {
//        TextMeshPro tmp = this.GetComponent<TextMeshPro>();
//        tmp.SetText(transform.parent.name);
//    }

//    void LateUpdate()
//    {
//        transform.LookAt(transform.position + cam.forward);

//        AnimatorStateInfo currentAnimation = anim.GetCurrentAnimatorStateInfo(0);

//        if (currentAnimation.IsName(SITTING_IDLE_ANIM))
//        {
//            timeSinceSitDown = 0f;
//            transform.position = new Vector3(transform.position.x, 2.35f, transform.position.z);
//        }
//        else if (currentAnimation.IsName(STANDING_IDLE_ANIM))
//        {
//            timeSinceStandUp = 0f;
//            transform.position = new Vector3(transform.position.x, 2.75f, transform.position.z);
//        }
//        else if (currentAnimation.IsName(STAND_UP_ANIM) && transform.position.y < 2.75f)
//        {
//            timeSinceStandUp = timeSinceStandUp + Time.deltaTime;
//            if (timeSinceStandUp > levantar / 2)
//            {
//                transform.position = new Vector3(transform.position.x, transform.position.y + (1f * Time.deltaTime), transform.position.z);
//            }
//        }
//        else if (currentAnimation.IsName(SIT_DOWN_ANIM) && transform.position.y > 2.35f)
//        {
//            timeSinceSitDown = timeSinceSitDown + Time.deltaTime;
//            if (timeSinceSitDown > empiezaSentar / 2 && timeSinceSitDown < terminaSentar / 2)
//            {
//                transform.position = new Vector3(transform.position.x, transform.position.y - (1f * Time.deltaTime), transform.position.z);
//            }
//        }
//    }
//}
