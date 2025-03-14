using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class GradeSystem : MonoBehaviour
{
    public Button btnWin, btnLose, btnReset;
    public Slider sdGrade;
    public TextMeshProUGUI txtGrade, txtState;

    private int grade, score;

    private void Awake()
    {
        if (PlayerPrefs.HasKey("Grade"))
        {
            grade = PlayerPrefs.GetInt("Grade");
            score = PlayerPrefs.GetInt("Score");
            MaxValueCheck();
        }
        else
        {
            grade = 18;
        }
        
        GradeView();
    }
    
    void Start()
    {
        btnWin.onClick.AddListener(Win);
        btnLose.onClick.AddListener(Lose);
        btnReset.onClick.AddListener(Reset);
    }

    void Reset()
    {
        grade = 18;
        score = 30;
        GradeView();
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
    
    void MaxValueCheck()
    {
        switch (grade)
        {
            case 1 :
            case 2 :
            case 3 :
            case 4 :
                sdGrade.maxValue = 130;
                break;
            case 5 :
            case 6 :
            case 7 :
            case 8 :
            case 9 :
                sdGrade.maxValue = 80;
                break;
            default :
                sdGrade.maxValue = 60;
                break;
        }
    }

    void Win()
    {
        txtState.text = "YOU WIN";
        sdGrade.value += 10;

        if (sdGrade.value >= sdGrade.maxValue)
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
        }
        else
        {
            txtState.text = "Your Grade is Up";
            grade--;
            MaxValueCheck();
            sdGrade.value = 30;
        }
    }

    void Lose()
    {
        txtState.text = "YOU LOSE";
        sdGrade.value -= 10;
        
        if (sdGrade.value <= 0)
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
        }
        else
        {
            txtState.text = "Your Grade is Down";
            grade++;
            MaxValueCheck();
            sdGrade.value = sdGrade.maxValue - 30;
        }
    }
}
