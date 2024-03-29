﻿using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Networking;

namespace Virterix.Common
{
    public class RemoteLoader : MonoBehaviour
    {

        public enum CheckMode
        {
            EveryFrame,
            FixedUpdate,
            WaitForSeconds
        }

        public enum DestroyMode
        {
            NotDestroy,
            DestroyObject,
            DestroyScript
        }

        public DestroyMode m_destroyMode;
        public float m_periodCheckingInSeconds = 1.0f;

        public void StartLoading(string url, float waitingTime, CheckMode checkingMode, Action<bool, UnityWebRequest> callbackLoadingCompleted)
        {
            StartCoroutine(LoadingHandler(url, waitingTime, checkingMode, callbackLoadingCompleted));
        }

        public static RemoteLoader Load(string url, float waitingTime, CheckMode checkingMode, DestroyMode destroyMode, Action<bool, UnityWebRequest> callbackLoadingCompleted)
        {
            RemoteLoader component = new GameObject("RemoteLoader").AddComponent<RemoteLoader>();
            component.m_destroyMode = destroyMode;
            component.StartLoading(url, waitingTime, checkingMode, callbackLoadingCompleted);
            return component;
        }

        private IEnumerator LoadingHandler(string url, float waitingTime, CheckMode checkingMode, Action<bool, UnityWebRequest> OnLoadingCompleted)
        {
            UnityWebRequest www = UnityWebRequest.Get(url);
            www.SendWebRequest();

            float passedTime = 0.0f;
            bool success = false;
            YieldInstruction waitInstruction = null;

            switch (checkingMode)
            {
                case CheckMode.EveryFrame:
                    waitInstruction = new WaitForEndOfFrame();
                    break;
                case CheckMode.FixedUpdate:
                    waitInstruction = new WaitForFixedUpdate();
                    break;
                case CheckMode.WaitForSeconds:
                    waitInstruction = new WaitForSeconds(m_periodCheckingInSeconds);
                    break;
            }

            while (!www.isDone)
            {
                switch (checkingMode)
                {
                    case CheckMode.EveryFrame:
                        yield return waitInstruction;
                        passedTime += Time.unscaledDeltaTime;
                        break;
                    case CheckMode.FixedUpdate:
                        yield return waitInstruction;
                        passedTime += Time.fixedDeltaTime;
                        break;
                    case CheckMode.WaitForSeconds:
                        yield return waitInstruction;
                        passedTime += m_periodCheckingInSeconds;
                        break;
                }

                if (passedTime > waitingTime)
                    break;
            }

            success = www.error == null;
            if (OnLoadingCompleted != null)
                OnLoadingCompleted(success, www);

            www.Dispose();

            switch (m_destroyMode)
            {
                case DestroyMode.DestroyObject:
                    Destroy(this.gameObject);
                    break;
                case DestroyMode.DestroyScript:
                    Destroy(this);
                    break;
            }
            yield return null;
        }

    }
}