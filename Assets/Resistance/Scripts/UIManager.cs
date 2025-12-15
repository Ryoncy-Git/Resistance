using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public Calculator culculator;
    public TextMeshProUGUI TextRes;
    public void OnClickCulculate()
    {
        culculator.StartCalculate();
    }

    public void ChangeTextRes(double res)
    {
        TextRes.text = "Resistance = " + res + "Ω";
        return;
    }
}
