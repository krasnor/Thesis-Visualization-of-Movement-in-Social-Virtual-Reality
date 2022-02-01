using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[RequireComponent(typeof(Collider))]
public class StudyLookAtInteractable : MonoBehaviour
{
    [Tooltip("in seconds")]
    public float TriggerAt = 5.0f;
    [Tooltip("in seconds")]
    public float TriggerAtDelay = 2.0f;
    public bool UseDelayTrigger = false;
    public bool AllowInteraction = true;

    public Material BaseMaterial;
    public Material ValidGazeMaterial;
    public Material OnTriggeredMaterial;
    public GameObject StateColoredGameObject;
    private Renderer m_stateColoredGameObjectRenderer;

    public Collider StandingVolume;
    public GameObject PlayerHead;

    public UnityEvent OnTrigger;
    public UnityEvent OnDelayedTrigger;
    public UnityEvent OnReset;

    public UnityEvent OnPlayerFirstArrived;
    private bool OnPlayerFirstArrivedTriggered = false;

    public UnityEvent OnValidGazeEnter;
    public UnityEvent OnValidGazeExit;

    //private UnityEvent OnHeadGazeEnter;
    //private UnityEvent OnHeadGazeStay;
    //private UnityEvent OnHeadGazeExit;

    //private float m_gazeEnterTime = 0f;
    private bool m_isBeingLookedAtByLocalPlayer = false;
    private bool m_localPlayerInsideStandingVolume = false;
    private bool m_lastFrameValidGaze = false;
    private float m_gazeAndPosEnterTime = 0f;


    protected bool m_alreadyTriggered = false;
    private float m_TimeTriggered = 0f;
    protected bool m_delayTriggered = false;

    public bool HasValidGaze { get { return m_isBeingLookedAtByLocalPlayer && m_localPlayerInsideStandingVolume; } }
    public bool IsAlreadyTriggered { get { return m_alreadyTriggered; } }
    public bool DelayTriggered { get { return m_delayTriggered; } }

    //protected bool debugLogs = false;

    public void ResetInteractable()
    {
        AllowInteraction = true;

        m_alreadyTriggered = false;
        m_TimeTriggered = 0f;
        m_delayTriggered = false;
        SetColor(BaseMaterial);
        OnPlayerFirstArrivedTriggered = false;
    }

    public void EnableInteractions(bool a_state)
    {
        AllowInteraction = a_state;
    }

    public void SetColor(Material a_m)
    {
        //if (debugLogs) Debug.Log("setting color: " + a_m.color);
        if (StateColoredGameObject != null && a_m != null)
        {
            var curr_material = m_stateColoredGameObjectRenderer.material;
            if(curr_material != a_m)
            {
                m_stateColoredGameObjectRenderer.material = a_m;
            }
        }
    }
    public void SetColor(Color a_c)
    {
        if (StateColoredGameObject != null)
        {
            m_stateColoredGameObjectRenderer.material.color = a_c;
        }
    }
    public Color GetColor()
    {
        if (StateColoredGameObject != null)
        {
            return m_stateColoredGameObjectRenderer.material.color;
        }
        return new Color();
    }

    public Transform GetStandingAreaLocation()
    {
        return StandingVolume.gameObject.transform;
    }

    protected virtual void Awake()
    {
        m_stateColoredGameObjectRenderer = StateColoredGameObject.GetComponent<Renderer>();
        if (m_stateColoredGameObjectRenderer == null)
            throw new MissingComponentException("StateColoredGameObject.Renderer Component is missing");
    }
    // Start is called before the first frame update
    protected virtual void Start()
    {
        if (StandingVolume == null)
            throw new MissingComponentException("StandingVolume Component is not set");
        if (PlayerHead == null)
            throw new MissingComponentException("PlayerHead Component is not set");

        SetColor(BaseMaterial);
    }


    // Update is called once per frame
    protected virtual void Update()
    {
        if (!AllowInteraction)
        {
            return;
        }

        // Process Delay Timer
        if (UseDelayTrigger && m_alreadyTriggered && !m_delayTriggered && (Time.time - m_TimeTriggered) > TriggerAtDelay)
        {
            m_delayTriggered = true;
            OnDelayedTrigger.Invoke();
            //Debug.Log("DelayTriggered --- triggered: " + Time.time);
        }

        if (PlayerHead != null)
        {
            if (StandingVolume.bounds.Contains(PlayerHead.transform.position))
            {
                //Debug.Log("head is inside collider: " + Time.time);
                m_localPlayerInsideStandingVolume = true;
                if(!OnPlayerFirstArrivedTriggered && !m_alreadyTriggered)
                {
                    OnPlayerFirstArrivedTriggered = true;
                    OnPlayerFirstArrived.Invoke();
                }
            }
            else
            {
                m_localPlayerInsideStandingVolume = false;
            }
        }

        if (m_isBeingLookedAtByLocalPlayer && m_localPlayerInsideStandingVolume)
        {
            if (!m_alreadyTriggered && m_gazeAndPosEnterTime == 0f)
            {
                // begin of look & pos valid
                //Debug.Log("ValidGaze Begin: " + Time.time);

                m_gazeAndPosEnterTime = Time.time;
                m_lastFrameValidGaze = true;
                SetColor(ValidGazeMaterial);
                OnValidGazeEnter.Invoke();
            }
        }
        else
        {
            //reset as not both conditions are met
            m_gazeAndPosEnterTime = 0f;
            if (m_lastFrameValidGaze)
            {
                //Debug.Log("ValidGaze Lost: " + Time.time);

                m_lastFrameValidGaze = false;
                if (!m_alreadyTriggered)
                {
                    SetColor(BaseMaterial);
                }

                OnValidGazeExit.Invoke();
            }
        }

        if (!m_alreadyTriggered && m_isBeingLookedAtByLocalPlayer && m_localPlayerInsideStandingVolume && (Time.time - m_gazeAndPosEnterTime) > TriggerAt)
        {
            //Debug.Log("OnTriggerInvoke: " + Time.time);
            m_alreadyTriggered = true;
            m_TimeTriggered = Time.time;
            SetColor(OnTriggeredMaterial);
            OnTrigger.Invoke();
        }
    }

    public virtual void OnGazeEnter()
    {
        //Debug.Log("OnGazeEnter: " + Time.time);
        m_isBeingLookedAtByLocalPlayer = true;
        //m_gazeEnterTime = Time.time;
        //OnHeadGazeEnter.Invoke();
    }

    public virtual void OnGazeHover()
    {
        //Debug.Log("OnGazeHover: " + (Time.time - m_gazeEnterTime));
        //if (!AlreadyTriggered && (Time.time - m_gazeEnterTime) > TriggerAt)
        //{
        //    Debug.Log("OnGazeHover --- triggered: " + Time.time);
        //    AlreadyTriggered = true;
        //    m_TriggerFinishTime = Time.time;
        //    OnTrigger.Invoke();
        //}
        //OnHeadGazeStay.Invoke();
    }

    public virtual void OnGazeExit()
    {
        //Debug.Log("OnGazeExit: " + Time.time);
        m_isBeingLookedAtByLocalPlayer = false;
        //m_gazeEnterTime = 0f;
        //m_TimeTriggered = 0f;
        //OnHeadGazeExit.Invoke();
    }
}
