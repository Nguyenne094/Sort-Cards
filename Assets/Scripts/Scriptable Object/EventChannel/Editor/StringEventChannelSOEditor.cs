using UnityEditor;
using UnityEngine.UIElements;

namespace Bap.EventChannel
{
    [CustomEditor(typeof(StringEventChannelSO))]
    public class StringEventChannelSOEditor : GeneralEventChannelSOEditor<string>
    {

    }
}