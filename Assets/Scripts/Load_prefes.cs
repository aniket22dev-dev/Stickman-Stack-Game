using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Load_prefes : MonoBehaviour
{
    public Text highScoreText;
    public List<Material> mats;
    public GameObject[] Player_body;
    
    private const string COLOR_KEY = "PlayerColorIndex";

    void Start()
    {
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        highScoreText.text = "" + highScore;

        int savedColorIndex = PlayerPrefs.GetInt(COLOR_KEY, 0);
        foreach (GameObject body in Player_body)
        {
            body.GetComponent<SkinnedMeshRenderer>().material = mats[savedColorIndex];
           
        }
    }

    public void Exit()
    {
        Application.Quit();
    }
}
