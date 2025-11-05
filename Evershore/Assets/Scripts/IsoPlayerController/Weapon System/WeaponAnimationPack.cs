using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AnimationPack", menuName = "WeaponSystem/AnimationPack")]
public class WeaponAnimationPack: ScriptableObject
{
    public List<AnimationData> animationData;
}

public enum ANIMiD
{
    Idle,
    Run,
    Aim,
    Fire,
    Charge,
    Reload,
    Block
}
[System.Serializable]
public struct AnimationData
{

    public ANIMiD id;
    public AnimationClip clip;
}
