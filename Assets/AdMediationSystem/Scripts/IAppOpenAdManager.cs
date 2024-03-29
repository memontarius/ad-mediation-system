﻿using System;

namespace Virterix.AdMediation
{
    public interface IAppOpenAdManager
    {
        public Action<bool> LoadComplete { get; set; }
        public bool IsAdAvailable { get; }
        public bool IsOpened { get; }
        public void LoadAd();
        public bool ShowAdIfAvailable();
    }
}