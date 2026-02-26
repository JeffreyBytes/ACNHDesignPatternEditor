using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Settings
{
    public static int RefreshsRate
    {
        get
        {
            var rate = PlayerPrefs.GetInt("RefreshRate");
            if (rate == 0) rate = 60;
            return rate;
        }
        set
        {
            PlayerPrefs.SetInt("RefreshRate", value);
            Application.targetFrameRate = value;
        }
    }

    public static int AnimationSpeed
    {
        get
        {
            return PlayerPrefs.GetInt("AnimationSpeed");
        }
        set
        {
            PlayerPrefs.SetInt("AnimationSpeed", value);
        }
    }

    public static float AnimationMultiplier
    {
        get
        {
            if (AnimationSpeed == 1)
                return 0.5f;
            else if (AnimationSpeed == 2)
                return 0.2f;
            return 1.0f;
        }
    }
}