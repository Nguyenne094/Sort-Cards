using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace Bap.EventChannel
{
    /// <summary>
    /// Other type of T just need to change to resonable T type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class EventChannelSO<T> : DescriptionAbstractSO
    {
        [Tooltip("The action to perform")]
        public UnityAction<T> OnEventRaised;
        
        public void RaiseEvent(T parameter)
        {
            if (OnEventRaised == null)
                return;

            OnEventRaised.Invoke(parameter);
        }
    }
}