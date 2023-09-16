using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TagwizzQASniffer.Core
{
    public interface ISnifferObserverSubscriber
    {
        public void OnAwake();
        public void OnStart();
        public void OnUpdate();

        public void OnEnabled();
        public void OnDisabled();
        public void OnDestroy();
    }

    public class SnifferObserver : MonoBehaviour
    {
        private readonly HashSet<ISnifferObserverSubscriber> _subscribers = new HashSet<ISnifferObserverSubscriber>();


        public void Subscribe(ISnifferObserverSubscriber subscriber)
        {
            _subscribers.Add(subscriber);
        }

        public void Unsubscribe(ISnifferObserverSubscriber subscriber)
        {
            _subscribers.Remove(subscriber);
        }
        private void Awake()
        {
            DontDestroyOnLoad(this);
            foreach(var subscriber  in _subscribers)
                subscriber.OnAwake();
        }

        private void OnEnable()
        {
            foreach(var subscriber  in _subscribers)
                subscriber.OnEnabled();
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

        private void OnDisable()
        {
            foreach(var subscriber  in _subscribers)
                subscriber.OnDisabled();
        }

        private void OnDestroy()
        {
            foreach(var subscriber  in _subscribers)
                subscriber.OnDestroy();
        }
    }
}