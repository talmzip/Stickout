using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;


public class ScoreManager : MonoBehaviour
{
    #region Singelton Decleration

    private static ScoreManager _instance;

    public static ScoreManager Instance { get { return _instance; } }


    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }
    #endregion
    int TotalScore = 0;

    public int ScoreNormalValue;
    public int ScoreBonusValue;
    public int ScoreOcclusionValue;

    public TextMeshPro ScoreText;
    public float scoreAppearenceDuration;
    public AnimationCurve scoreAppearenceCurve;

    private void Start()
    {
        ScoreText.color = Color.clear;
    }

    public void AddScore(StickType type, bool isBonus)
    {
        if (TotalScore == 0)
            SetScoreAppearence(true);

        if (isBonus)
        {
            TotalScore += ScoreBonusValue;
        }
        else
        {
            switch (type)
            {
                case StickType.Normal:
                    TotalScore += ScoreNormalValue;
                    break;
                case StickType.Occlusion:
                    TotalScore += ScoreOcclusionValue;
                    break;
                default: TotalScore += 0; break;
            }
        }

        ScoreText.text = TotalScore.ToString();

    }

    public void SetScoreAppearence(bool shouldAppear)
    {
        StopAllCoroutines();
        StartCoroutine(setAppearence(shouldAppear));
    }

    IEnumerator setAppearence(bool shouldAppear)
    {
        Color targetColor = shouldAppear ? new Color(1,1,1,.3f): Color.clear;
        Color currentColor = ScoreText.color;

        float lerpTime = 0;
        while(lerpTime<scoreAppearenceDuration)
        {
            lerpTime += Time.deltaTime;
            float t = lerpTime / scoreAppearenceDuration;
            t = scoreAppearenceCurve.Evaluate(t);

            ScoreText.color = Color.Lerp(currentColor, targetColor, t);

            yield return null;
        }
    }

    public void ResetScore()
    {
        TotalScore = 0;
        SetScoreAppearence(false);
    }

}
