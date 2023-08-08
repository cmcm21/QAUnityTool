using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TagwizzQASniffer.Core
{
    public interface ILifeCycleSubscriber
    {
        public void OnAwake();
        public void OnStart();
        public void OnUpdate();
    }

    public class LifeCycle : MonoBehaviour
    {
        private readonly HashSet<ILifeCycleSubscriber> _subscribers = new HashSet<ILifeCycleSubscriber>();

        public void Subscribe(ILifeCycleSubscriber subscriber)
        {
            _subscribers.Add(subscriber);
        }

        public void Unsubscribe(ILifeCycleSubscriber subscriber)
        {
            _subscribers.Remove(subscriber);
        }
        private void Awake()
        {
            foreach(var subscriber  in _subscribers)
                subscriber.OnAwake();
        }

        private void Start()
        {
            foreach(var subscriber  in _subscribers)
                subscriber.OnStart();
        }

        private void Update()
        {
            foreach(var subscriber  in _subscribers)
                subscriber.OnUpdate();
        }
    }
}