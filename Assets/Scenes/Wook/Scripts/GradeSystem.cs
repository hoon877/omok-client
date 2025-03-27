using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;


public class GradeSystem : MonoBehaviour
{
    public Button btnWin, btnLose, btnReset;        // Test
    public Slider sdGrade;
    public GameObject ui, line, zero;
    public TextMeshProUGUI txtGrade, txtState, txtMax;

    private int grade, score, div;
    private float bar;

    private void Awake()
    {
        bar = sdGrade.GetComponent<RectTransform>().rect.width;
        
        if (PlayerPrefs.HasKey("Grade"))
        {
            grade = PlayerPrefs.GetInt("Grade");
            score = PlayerPrefs.GetInt("Score");
            MaxValueSet();
        }
        else
        {
            grade = 18;
        }
        
        txtState.text = "Your Score is : " + (score - 30);
        GradeView();
        StartCoroutine(UIFadeIn());
    }
    
    void Start()
    {
        btnWin.onClick.AddListener(Win);        // Test
        btnLose.onClick.AddListener(Lose);      // Test
        btnReset.onClick.AddListener(Reset);    // Test
    }

    void Reset()                // Test
    {
        grade = 18;
        score = 30;
        MaxValueSet();
        txtState.text = "Your Score is : " + (score - 30);
        GradeView();
        StartCoroutine(UIFadeIn());
    }

    IEnumerator UIFadeIn()
    {
        ui.GetComponent<CanvasGroup>().alpha = 0;
        
        while (ui.GetComponent<CanvasGroup>().alpha < 1)
        {
            ui.GetComponent<CanvasGroup>().alpha += Time.deltaTime;
            yield return null;
        }
    }

    void GradeView()
    {
        txtGrade.text = "Your Grade : " + grade;
        sdGrade.value = score;
        
        // Color
        if (sdGrade.value > 30)
        {
            sdGrade.transform.GetChild(1).GetChild(0).GetComponent<Image>().color = Color.blue;
        }
        else if (sdGrade.value < 30)
        {
            sdGrade.transform.GetChild(1).GetChild(0).GetComponent<Image>().color = Color.red;
        }
        else
        {
            sdGrade.transform.GetChild(1).GetChild(0).GetComponent<Image>().color = Color.green;
        }
        
        // Save UserInfo
        PlayerPrefs.SetInt("Grade", grade);
        PlayerPrefs.SetInt("Score", (int)sdGrade.value);
    }
    
    void MaxValueSet()
    {
        if (GameObject.FindWithTag("Lines"))
        {
            for (int i = 1; i < div; i++)
            {
                Destroy(GameObject.Find("Line" + i));
            }
        }
        
        switch (grade)
        {
            case 1 :
            case 2 :
            case 3 :
            case 4 :
                div = 12;
                break;
            case 5 :
            case 6 :
            case 7 :
            case 8 :
            case 9 :
                div = 8;
                break;
            default :
                div = 6;
                break;
        }

        sdGrade.maxValue = div * 10;
        txtMax.text = (div*10-30).ToString();
        
        // DevideLine
        for (int i = 1; i < div; i++)
        {
            Vector2 pos = new Vector2((i - div / 2) * bar / (div * 108), sdGrade.transform.position.y);
            GameObject obj = Instantiate(line, pos, Quaternion.identity, transform.Find("/Canvas/UI").transform);
            obj.name = "Line" + i;

            if (i == 3)
            {
                zero.transform.position = pos - new Vector2(0, 50) / 192;
            }
        }
    }

    IEnumerator SliderAnimation(float sliderValue)
    {
        float time = 0;

        while (time < 2)
        {
            time += Time.deltaTime;
            sdGrade.value = Mathf.Lerp(sdGrade.value, sliderValue, time / 2);
            Debug.Log("Slider Moving");
            yield return null;
        }
    }

    void Win()
    {
        StartCoroutine(UIFadeIn());
        
        txtState.text = "YOU WIN";
        
        sdGrade.value += 10;
        
        score += 10;
        if (score >= (int)sdGrade.maxValue)
        {
            UpGrade();
        }

        GradeView();
    }

    void UpGrade()
    {
        if (grade == 1)
        {
            txtState.text = "Your Grade is Max";
            score = (int)sdGrade.maxValue;
        }
        else
        {
            txtState.text = "Your Grade is Up";
            grade--;
            MaxValueSet();
            score = 30;
        }
    }

    void Lose()
    {
        StartCoroutine(UIFadeIn());
        
        txtState.text = "YOU LOSE";
        
        sdGrade.value -= 10;
        score -= 10;
        
        if (score <= 0)
        {
            DownGrade();
        }
        
        GradeView();
    }

    void DownGrade()
    {
        if (grade == 18)
        {
            txtState.text = "Your Grade is Min";
            score = 0;
        }
        else
        {
            txtState.text = "Your Grade is Down";
            grade++;
            MaxValueSet();
            score = (int)sdGrade.maxValue - 30;
        }
    }
}
