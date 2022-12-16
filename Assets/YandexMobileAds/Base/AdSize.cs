/*
 * This file is a part of the Yandex Advertising Network
 *
 * Version for Unity (C) 2018 YANDEX
 *
 * You may not use this file except in compliance with the License.
 * You may obtain a copy of the License at https://legal.yandex.com/partner_ch/
 */

namespace YandexMobileAds.Base
{
    /// <summary>
    /// Banner size type
    /// </summary>
    public enum AdSizeType
    {
        /// with specified height and width of the banner
        [System.Obsolete("Use Flexible or Sticky AdSizeType instead of fixed")]
        Fixed,
        /// with the specified maximum height and width of the banner
        Flexible,
        /// with the specified maximum width of a sticky banner
        Sticky
    }

    /// <summary>
    /// This class is responsible for the banner size.
    /// </summary>
    public class AdSize
    {
        private const int NotSpecified = -1;
        private const string DEPRECATION_STRING =
            "Use flexible or sticky banners instead of fixed. " +
            "Fixed AdSize API will be removed starting from version 3.* ";
        [System.Obsolete(DEPRECATION_STRING)]
        public static readonly AdSize BANNER_240x400 = new AdSize {Width = 240, Height = 400, AdSizeType = AdSizeType.Fixed};
        [System.Obsolete(DEPRECATION_STRING)]
        public static readonly AdSize BANNER_300x250 = new AdSize {Width = 300, Height = 250, AdSizeType = AdSizeType.Fixed};
        [System.Obsolete(DEPRECATION_STRING)]
        public static readonly AdSize BANNER_300x300 = new AdSize {Width = 300, Height = 300, AdSizeType = AdSizeType.Fixed};
        [System.Obsolete(DEPRECATION_STRING)]
        public static readonly AdSize BANNER_320x50 = new AdSize {Width = 320, Height = 50, AdSizeType = AdSizeType.Fixed};
        [System.Obsolete(DEPRECATION_STRING)]
        public static readonly AdSize BANNER_320x100 = new AdSize {Width = 320, Height = 100, AdSizeType = AdSizeType.Fixed};
        [System.Obsolete(DEPRECATION_STRING)]
        public static readonly AdSize BANNER_400x240 = new AdSize {Width = 400, Height = 240, AdSizeType = AdSizeType.Fixed};
        [System.Obsolete(DEPRECATION_STRING)]
        public static readonly AdSize BANNER_728x90 = new AdSize {Width = 728, Height = 90, AdSizeType = AdSizeType.Fixed};

        /// <summary>
        /// The initial width of the banner
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// The initial height of the banner
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// Banner size type
        /// </summary>
        public AdSizeType AdSizeType { get; private set; }

        /// <summary>
        /// Creates an object of the AdSize class with the specified maximum width of a sticky banner.
        /// </summary>
        /// <param name="width">Maximum width available for a banner.</param>
        /// <returns>An object of the AdSize class with the specified maximum width of a sticky banner.</returns>
        public static AdSize StickySize(int width)
        {
            return new AdSize {Width = width, Height = NotSpecified, AdSizeType = AdSizeType.Sticky};
        }

        /// <summary>
        /// Creates an object of the AdSize class with the specified maximum height and width of the banner.
        /// </summary>
        /// <param name="width">Maximum width available for a banner.</param>
        /// <param name="height">Maximum height available for a banner.</param>
        /// <returns>
        /// An object of the AdSize class with the specified maximum height and width of the banner.
        /// </returns>
        public static AdSize FlexibleSize(int width, int height)
        {
            return new AdSize {Width = width, Height = height, AdSizeType = AdSizeType.Flexible};
        }
    }
}