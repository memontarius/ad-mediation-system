using System;
using System.Collections.Generic;
using UnityEngine;

namespace YandexMobileAds.Common
{
    internal class MainThreadDispatcher : MonoBehaviour
    {
        private static MainThreadDispatcher instance = null;
        private static readonly Queue<Action> mainThreadQueue = new Queue<Action>();

        private static bool IsRunning
        {
            get { return instance != null; }
        }

        internal static void initialize()
        {
            if (IsRunning)
            {
                return;
            }

            // Add an invisible game object to the scene
            GameObject obj = new GameObject("YandexMainThreadDispatcher");
            obj.hideFlags = HideFlags.HideAndDontSave;
            DontDestroyOnLoad(obj);
            instance = obj.AddComponent<MainThreadDispatcher>();
        }

        internal static void EnqueueAction(Action action)
        {
            lock (mainThreadQueue)
            {
                mainThreadQueue.Enqueue(action);
            }
        }

        void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        void Update()
        {
            // dispatch actions on the main thread when the queue is not empty
            while (mainThreadQueue.Count > 0)
            {
                Action dequeuedAction = null;
                lock (mainThreadQueue)
                {
                    try
                    {
                        dequeuedAction = mainThreadQueue.Dequeue();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
                if (dequeuedAction != null)
                {
                    dequeuedAction.Invoke();
                }
            }
        }

        void OnDisable()
        {
            instance = null;
        }
    }
}
