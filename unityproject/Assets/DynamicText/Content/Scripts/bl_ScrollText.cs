using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class bl_ScrollText : MonoBehaviour {

    [Header("Global")]
    public List<string> Texts = new List<string>();

    [Header("Positions")]
    public Vector2 StartPosition;
    public Vector2 MiddlePosition;
    public Vector2 FinalPosition;

    [Header("Settings")]
    public MoveType m_MoveType = MoveType.Snapp;
    [Range(1, 100)]
    public float ScrollSpeed = 100f;
    public float WaitForNextPos = 5f;

    [Header("References")]
    public Text mText = null;

    protected bool isDone = false;
    private int state = 0;
    protected int CurrentText = 0;
    protected bool mAvaible = true;
    protected float _timeStartedLerping;

    [HideInInspector] public Image background;

    [HideInInspector] public bool showMessage;

    void Awake()
    {
       isDone = true;
       background = GetComponent<Image>();
    }
    
    /// <summary>
    /// 
    /// </summary>
    void FixedUpdate()
    {
        if (!showMessage)
            return;
        if(!isDone)
            return;
        if (mText == null)
            return;
        if (Texts.Count< 0)
            return;

        Vector2 p = mText.rectTransform.anchoredPosition;
        if (state == 0 && mAvaible)
        {
            if (m_MoveType == MoveType.Snapp)
            {
                p = Vector2.MoveTowards(p, StartPosition, Time.deltaTime * ((ScrollSpeed * 5) * Time.timeScale));
                if (p == StartPosition && mAvaible)
                {
                    StartCoroutine(NextState(true, 0.0f));
                }
            }
            else if (m_MoveType == MoveType.Lerp)
            {
                float timeSinceStarted = Time.time - _timeStartedLerping;
                float percentageComplete = timeSinceStarted / (ScrollSpeed / 10);
                p = Vector2.Lerp(p, StartPosition, percentageComplete);
                if (percentageComplete >= 1.0f && mAvaible)
                {
                    StartCoroutine(NextState(true, 0.0f));
                }
            }
            
        }
        else if (state == 1 && mAvaible)
        {           
            if (m_MoveType == MoveType.Snapp)
            {
                p = Vector2.MoveTowards(p, MiddlePosition, Time.deltaTime * ((ScrollSpeed * 5) * Time.timeScale));
                if (p == MiddlePosition && mAvaible)
                {
                    StartCoroutine(NextState(false, WaitForNextPos));
                }
            }
            else if (m_MoveType == MoveType.Lerp)
            {
                float timeSinceStarted = Time.time - _timeStartedLerping;
                float percentageComplete = timeSinceStarted / (ScrollSpeed / 10);
                p = Vector2.Lerp(p, MiddlePosition, percentageComplete);
                if (percentageComplete >= 1.0f && mAvaible)
                {
                    StartCoroutine(NextState(false, WaitForNextPos));
                }
            }
            
        }
        else if (state == 2 && mAvaible)
        {           
            if (m_MoveType == MoveType.Snapp)
            {
                p = Vector2.MoveTowards(p, FinalPosition, Time.deltaTime * (ScrollSpeed * 5));
                if (p == FinalPosition && mAvaible)
                {
                    StartCoroutine(NextState(false, 0.5f));
                }
            }
            else if (m_MoveType == MoveType.Lerp)
            {
                float timeSinceStarted = Time.time - _timeStartedLerping;
                float percentageComplete = timeSinceStarted / (ScrollSpeed / 10);
                p = Vector2.Lerp(p, FinalPosition, percentageComplete);
                if (percentageComplete > 0.35f) // Hide black background
                {
                    var backgroundColor = background.color;
                    background.color = new Color(backgroundColor.r, backgroundColor.g, backgroundColor.b, 0);
                }
                if (percentageComplete >= 1.0f && mAvaible)
                {
                    StartCoroutine(NextState(false, 0.5f));
                }
            }
            
        }
        mText.rectTransform.anchoredPosition = p;
    }
    
    /// <summary>
    /// Remove codes ([box])
    /// </summary>
    /// <param name="tl"></param>
    /// <returns></returns>
    private void RemoveCodes(List<string> tl)
    {
        List<string> t = new List<string>();
        t = tl;
        for (int ii = 0; ii < t.Count; ii++)
        {
            if (t[ii].Contains("[box]"))
            {
                t.RemoveAt(ii);
            }
        }
        Check(t);
    }
    
    /// <summary>
    /// Fix odd number from the list
    /// </summary>
    /// <param name="list"></param>
    void Check(List<string> list)
    {
        int num = 0;
        for (int ii = 0; ii < list.Count; ii++)
        {
            if (list[ii].Contains("[box]"))
            {
                num++;
            }
        }
        if (num > 0)
        {
            RemoveCodes(list);
            return;
        }
        //If pass test
        Texts = list;
        isDone = true;
    }
    
    /// <summary>
    /// 
    /// </summary>
    void ResetPosition()
    {
        mText.rectTransform.anchoredPosition = StartPosition;
        showMessage = false;
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator NextState(bool first,float t)
    {
        mAvaible = false;
        _timeStartedLerping = Time.time;
        if (!first)
        {
            yield return new WaitForSeconds(t);
        }
        if (state == 2)
        {
            CurrentText = (CurrentText + 1) % Texts.Count;
            mText.text = Texts[CurrentText];
            ResetPosition();
        }
        state = (state + 1) % 3;
        mAvaible = true;
    }

    [System.Serializable]
    public enum MoveType
    {
        Lerp,
        Snapp,
    }
}