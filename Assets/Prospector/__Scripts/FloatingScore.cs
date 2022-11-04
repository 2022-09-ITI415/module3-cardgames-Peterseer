using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum eFSState
{
    idle,
    pre,
    active,
    post
}

public class FloatingScore : MonoBehaviour
{
    [Header("Set Dynamically")]
    public eFSState state = eFSState.idle;
    [SerializeField]
    protected int _score = 0;
    public string scoreString;
    // The score property sets both _score and scoreString 
    public int score
    {
        get
        {
            return (_score);
        }
        set
        {
            _score = value;
            scoreString = _score.ToString("N0");// "N0" adds commas to the num 
                                                // Search "C# Standard Numeric Format Strings" for ToString formats 
            GetComponent<Text>().text = scoreString;
        }
    }

    public List<Vector2> bezierPts; // B¨¦zier points for movement 
    public List<float> fontSizes; // B¨¦zier points for font scaling 
    public float timeStart = -1f;
    public float timeDuration = 1f;
    public string easingCurve = Easing.InOut; // Uses Easing in Utils.cs
       void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
