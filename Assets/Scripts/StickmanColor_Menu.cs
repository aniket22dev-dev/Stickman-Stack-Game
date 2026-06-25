using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class StickmanColor_Menu : MonoBehaviour
{
    public List<Material> mats;
    public GameObject[] Player_body;
    public Material Default_mat;
    private const string COLOR_KEY = "PlayerColorIndex";
    private int highscore;
    private void Start()
    {
        highscore = PlayerPrefs.GetInt("HighScore", 0);
        Exit();
    }

    public void Applycolor(int colorIndex)
    {
        foreach (GameObject body in Player_body)
        {
            body.GetComponent<Renderer>().material = mats[colorIndex];
        }

        PlayerPrefs.SetInt(COLOR_KEY, colorIndex);
        PlayerPrefs.Save();
    }

     public void Exit()
    {
        foreach (GameObject body in Player_body)
        {
            body.GetComponent<Renderer>().material = Default_mat;
        }
    }
}