/*
 * This file is a part of the Yandex Advertising Network
 *
 * Version for iOS (C) 2019 YANDEX
 *
 * You may not use this file except in compliance with the License.
 * You may obtain a copy of the License at https://legal.yandex.com/partner_ch/
 */

using System;
using YandexMobileAds.Base;
using System.Collections.Generic;

namespace YandexMobileAds.Platforms.iOS
{
    #if (UNITY_5 && UNITY_IOS) || UNITY_IPHONE
    
    public class AdRequestClient : IDisposable
    {
        public string ObjectId { get; private set; }
        private readonly LocationClient location;
        private readonly string contextQuery;
        private readonly ListClient contextTags;
        private readonly DictionaryClient parameters;
        private readonly string age;
        private readonly string gender;

        public AdRequestClient(AdRequest adRequest)
        {
            if (adRequest.Location != null) 
            {
                this.location = new LocationClient(adRequest.Location);
            }
            this.contextQuery = adRequest.ContextQuery;
            this.age = adRequest.Age;
            this.gender = adRequest.Gender;
            this.contextTags = new ListClient();
            if (adRequest.ContextTags != null)
            {
                foreach (string item in adRequest.ContextTags)
                {
                    this.contextTags.Add(item);
                }
            }
            this.parameters = new DictionaryClient();
            if (adRequest.Parameters != null)
            {
                foreach (KeyValuePair<string, string> entry in adRequest.Parameters)
                {
                    this.parameters.Put(entry.Key, entry.Value);
                }
            }
            string locationId = this.location != null ? 
                                    this.location.ObjectId : null;
            string contextTagsId = this.contextTags != null ? 
                                       this.contextTags.ObjectId : null;
            string parametersId = this.parameters != null ? 
                                      this.parameters.ObjectId : null;
            this.ObjectId = AdRequestBridge.YMAUnityCreateAdRequest(
                locationId, contextQuery, contextTagsId, parametersId, age, gender);
        }

        public void Destroy()
        {
            ObjectBridge.YMAUnityDestroyObject(this.ObjectId);
        }

        public void Dispose()
        {
            this.Destroy();
        }

        ~AdRequestClient()
        {
            this.Destroy();
        }
    }

    #endif
}