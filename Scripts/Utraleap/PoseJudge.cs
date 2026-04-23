using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoseJudge : MonoBehaviour
{
    public static bool isFistL = false;
    public static bool isFistR = false;
    public static bool isPalmL = false;
    public static bool isPalmR = false;
    public static bool isOKR = false;

    // ================= ∫À–ƒ–ﬁ∏¥£∫∑¿÷πøÁ≥°æ∞◊¥Ã¨ø®À¿ =================
    private void Awake()
    {
        isFistL = false;
        isFistR = false;
        isPalmL = false;
        isPalmR = false;
        isOKR = false;
    }

    //◊Û ÷»≠
    public void OnFistL() { isFistL = true; }
    public void LostFistL() { isFistL = false; }

    //”“ ÷»≠
    public void OnFistR() { isFistR = true; }
    public void LostFistR() { isFistR = false; }

    //◊Û ÷’∆
    public void OnPalmL() { isPalmL = true; }
    public void LostPalmL() { isPalmL = false; }

    //”“ ÷’∆
    public void OnPalmR() { isPalmR = true; }
    public void LostPalmR() { isPalmR = false; }

    //”“ ÷ƒ¥÷∏
    public void OnOKR() { isOKR = true; }
    public void LostOKR() { isOKR = false; }
}