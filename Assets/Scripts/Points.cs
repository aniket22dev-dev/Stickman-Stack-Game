using UnityEngine;
using UnityEngine.UI;
public class Points : MonoBehaviour
{
    public Text point_text;
    public  int score;

    private void Start()
    {

        Pluspoint();
    }
    // Update is called once per frame
    public void Pluspoint()
    {
        point_text.text= "" + score;
    }
}
