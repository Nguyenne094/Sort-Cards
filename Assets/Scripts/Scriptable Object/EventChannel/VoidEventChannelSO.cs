using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace Bap.EventChannel
{
    [CreateAssetMenu(fileName = "NewVoidEventChannelSO", menuName = "Event Channel SO/Void")]
    public  class VoidEventChannelSO : DescriptionAbstractSO
    {
        [Tooltip("The action to perform")]
        public UnityAction OnEventRaised;
        
        public void RaiseEvent()
        {
            if (OnEventRaised == null)
                return;

            OnEventRaised.Invoke();
        }
    }
}