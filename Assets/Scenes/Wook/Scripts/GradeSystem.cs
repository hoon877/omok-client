using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class GradeSystem : MonoBehaviour
{
    public Button btnWin, btnLose;
    public Slider sdGrade;
    public TextMeshProUGUI txtGrade, txtState;

    private int grade;

    void Start()
    {
        btnWin.onClick.AddListener(Win);
        btnLose.onClick.AddListener(Lose);

        if (PlayerPrefs.HasKey("Grade"))
        {
            grade = PlayerPrefs.GetInt("Grade");
        }
        else
        {
            grade = 18;
        }
        
        FloatingGrade();
    }

    void FloatingGrade()
    {
        txtGrade.text = "Your Grade : " + grade;
        PlayerPrefs.SetInt("Grade", grade);
    }

    void Win()
    {
        txtState.text = "YOU WIN";
        sdGrade.value += 10;

        if (sdGrade.value >= sdGrade.maxValue)
        {
            if (grade == 1)
            {
                txtState.text = "Your Grade is Max";
            }
            else
            {
                txtState.text = "Your Grade is Up";
                grade--;
                sdGrade.value = 30;
            }
            FloatingGrade();
        }
        ColorCheck();
    }

    void Lose()
    {
        txtState.text = "YOU LOSE";
        sdGrade.value -= 10;
        
        if (sdGrade.value <= 0)
        {
            if (grade == 18)
            {
                txtState.text = "Your Grade is Min";
            }
            else
            {
                txtState.text = "Your Grade is Down";
                grade++;
                sdGrade.value = 30;
            }
            FloatingGrade();
        }
        
        ColorCheck();
    }

    void ColorCheck()
    {
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
    }
}
