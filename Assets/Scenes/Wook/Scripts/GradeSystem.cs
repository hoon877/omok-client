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

    void Start()
    {
        btnWin.onClick.AddListener(Win);
        btnLose.onClick.AddListener(Lose);
    }

    void Win()
    {
        txtState.text = "YOU WIN";
        sdGrade.value += 10;
        ColorCheck();
    }

    void Lose()
    {
        txtState.text = "YOU LOSE";
        sdGrade.value -= 10;
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
