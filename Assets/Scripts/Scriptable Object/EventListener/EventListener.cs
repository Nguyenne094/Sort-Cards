using UnityEngine.Events;
using UnityEngine;
using Bap.EventChannel;

namespace Bap.EventChannel
{
    public abstract class EventListener<T> : MonoBehaviour
    {
        public EventChannelSO<T> EventChannel;
        public UnityEvent<T> Response;

        public void Invoke(T value)
        {
            Response?.Invoke(value);
        }
    }
}