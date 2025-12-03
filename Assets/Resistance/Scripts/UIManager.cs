using UnityEngine;

public class UIManager : MonoBehaviour
{
    public Culculator culculator;
    public void OnClickCulculate()
    {
        culculator.StartCulculate();
    }
}
