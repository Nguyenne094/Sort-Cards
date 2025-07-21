using UnityEngine;


public abstract class DescriptionAbstractSO : ScriptableObject
{
    [TextArea(5, 10)] public string Description;
}