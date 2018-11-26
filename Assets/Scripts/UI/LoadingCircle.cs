///////////////////////////////////////////////////////////////////////////
// CI6510 Optimised Game Programming    (Kingston University)            //
// Course Work 1 : Network Game with Plug-ins                            //
// Submission by Max Bryans (K1628007) and Taylor Fowler (K1612040)      //
// December 2018                                                         //
///////////////////////////////////////////////////////////////////////////
using UnityEngine;

// Credit:
//  https://www.salusgames.com/2017/01/08/circle-loading-animation-in-unity3d/

public class LoadingCircle : MonoBehaviour
{
    public float RotateSpeed = 200f;
    private RectTransform _rectComponent;

    private void Start()
    {
        _rectComponent = GetComponent<RectTransform>();
    }

    private void Update()
    {
        _rectComponent.Rotate(0f, 0f, RotateSpeed * Time.deltaTime);
    }
}
