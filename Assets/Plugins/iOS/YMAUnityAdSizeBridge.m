/*
 * This file is a part of the Yandex Advertising Network
 *
 * Version for iOS (C) 2020 YANDEX
 *
 * You may not use this file except in compliance with the License.
 * You may obtain a copy of the License at https://legal.yandex.com/partner_ch/
 */

#import <YandexMobileAds/YandexMobileAds.h>
#import "YMAUnityObjectsStorage.h"
#import "YMAUnityObjectIDProvider.h"
#import "YMAUnityStringConverter.h"

char *YMAUnityObjectIDWithAdSize(YMAAdSize *adSize)
{
    const char *objectID = [YMAUnityObjectIDProvider IDForObject:adSize];
    [[YMAUnityObjectsStorage sharedInstance] setObject:adSize withID:objectID];
    return [YMAUnityStringConverter copiedCString:objectID];
}

char *YMAUnityCreateFixedAdSize(NSInteger width, NSInteger height)
{
    YMAAdSize *adSize = [YMAAdSize fixedSizeWithCGSize:CGSizeMake(width, height)];
    return YMAUnityObjectIDWithAdSize(adSize);
}

char *YMAUnityCreateStickyAdSize(NSInteger width)
{
    YMAAdSize *adSize = [YMAAdSize stickySizeWithContainerWidth:width];
    return YMAUnityObjectIDWithAdSize(adSize);
}

char *YMAUnityCreateFlexibleAdSizeWithSize(NSInteger width, NSInteger height)
{
    YMAAdSize *adSize = [YMAAdSize flexibleSizeWithCGSize:CGSizeMake(width, height)];
    return YMAUnityObjectIDWithAdSize(adSize);
}
