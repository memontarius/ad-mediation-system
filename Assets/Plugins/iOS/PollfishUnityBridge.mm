#import "PollfishUnityBridge.h"
#import <Pollfish/Pollfish.h>

extern void UnitySendMessage(const char * className,const char * methodName, const char * param);

@implementation PollfishUnityBridge

NSString *gameObj; // the main game object in Unity that will listen for the events


//Loads before application's didFinishLaunching method is called (i.e. when this plugin
//is added to the Objective C runtime)
+ (void)load
{
    // Register for Pollfish notifications to inform Unity
    
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(surveyCompleted:)
                                                 name:@"PollfishSurveyCompleted" object:nil];
    
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(surveyOpened:)
                                                 name:@"PollfishOpened" object:nil];
    
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(surveyClosed:)
                                                 name:@"PollfishClosed" object:nil];
    
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(surveyReceived:)
                                                 name:@"PollfishSurveyReceived" object:nil];
    
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(surveyNotAvailable:)
                                                 name:@"PollfishSurveyNotAvailable" object:nil];
    
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(userNotEligible:)
                                                 name:@"PollfishUserNotEligible" object:nil];
    
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(userRejectedSurvey:)
                                                 name:@"PollfishUserRejectedSurvey" object:nil];
    
}


//iOS-to-Unity, methods get called when the notification triggers

+ (void)surveyCompleted:(NSNotification *)notification
{
    int surveyCPA = [[[notification userInfo] valueForKey:@"survey_cpa"] intValue];
    int surveyIR = [[[notification userInfo] valueForKey:@"survey_ir"] intValue];
    int surveyLOI = [[[notification userInfo] valueForKey:@"survey_loi"] intValue];
    
    NSString *surveyClass =(NSString *) [[notification userInfo] valueForKey:@"survey_class"];
    
    NSString *rewardName = [[notification userInfo] valueForKey:@"reward_name"];
    int rewardValue = [[[notification userInfo] valueForKey:@"reward_value"] intValue];
    
    const char *surveyInfo = [[NSString stringWithFormat:@"%d,%d,%d,%@ ,%@,%d",surveyCPA, surveyIR, surveyLOI, surveyClass, rewardName, rewardValue] UTF8String];
    
    UnitySendMessage([gameObj UTF8String],"surveyCompleted",surveyInfo);
}

+ (void)surveyOpened:(NSNotification *)notification
{
    UnitySendMessage([gameObj UTF8String],"surveyOpened","");
}

+ (void)surveyClosed:(NSNotification *)notification
{
    UnitySendMessage([gameObj UTF8String],"surveyClosed","");
}

+ (void)surveyReceived:(NSNotification *)notification
{
    
    int surveyCPA = [[[notification userInfo] valueForKey:@"survey_cpa"] intValue];
    int surveyIR = [[[notification userInfo] valueForKey:@"survey_ir"] intValue];
    int surveyLOI = [[[notification userInfo] valueForKey:@"survey_loi"] intValue];
    
    NSString *surveyClass =[[notification userInfo] valueForKey:@"survey_class"];
    
    NSString *rewardName = [[notification userInfo] valueForKey:@"reward_name"];
    int rewardValue = [[[notification userInfo] valueForKey:@"reward_value"] intValue];
    
    
    NSLog(@"Pollfish: Survey Completed - SurveyPrice:%d andSurveyIR: %d andSurveyLOI:%d andSurveyClass:%@ andRewardName:%@ andRewardValue:%d", surveyCPA,surveyIR, surveyLOI, surveyClass, rewardName, rewardValue);
    
    const char *surveyInfo = ([notification userInfo]!=nil)? [[NSString stringWithFormat:@"%d,%d,%d,%@ ,%@,%d",surveyCPA, surveyIR, surveyLOI, surveyClass, rewardName, rewardValue] UTF8String]: "";
    
    UnitySendMessage([gameObj UTF8String],"surveyReceived",surveyInfo);

}


+ (void)userNotEligible:(NSNotification *)notification
{
    UnitySendMessage([gameObj UTF8String],"userNotEligible","");
}

+ (void)userRejectedSurvey:(NSNotification *)notification
{
    UnitySendMessage([gameObj UTF8String],"userRejectedSurvey","");
}

+ (void)surveyNotAvailable:(NSNotification *)notification
{
    UnitySendMessage([gameObj UTF8String],"surveyNotAvailable","");
}
@end

//Unity-to-iOS, will get called when Unity method is called.
extern "C"
{
    void PollfishInitWith(int position, int padding, const char *apiKey, bool release_mode, bool reward_mode, const char *request_uuid,const char * attributes, bool offerwall_mode) {
        
        NSString *attris = [NSString stringWithUTF8String:attributes];
        NSArray *attributesArray = [attris componentsSeparatedByString:@"\n"];
        
        __block NSMutableDictionary *oAttributes = [[NSMutableDictionary alloc] init];
        
        for (int i=0; i < [attributesArray count]; i++) {
            
            NSString *keyValuePair = [attributesArray objectAtIndex:i];
            NSRange range = [keyValuePair rangeOfString:@"="];
            
            if (range.location != NSNotFound) {
                
                NSString *key = [keyValuePair substringToIndex:range.location];
                NSString *value = [keyValuePair substringFromIndex:range.location+1];
                
                [oAttributes setObject:value forKey:key];
            }
        }


        __block  int pollfishPosition=position;
        __block int indPadding =padding;
        __block  BOOL rewardMode=reward_mode;
        __block  BOOL releaseMode = release_mode;
        __block BOOL offerwallMode = offerwall_mode;
        __block NSString *requestUUID = [NSString stringWithUTF8String:request_uuid? request_uuid : ""];
        
        PollfishParams *pollfishParams =  [PollfishParams initWith:^(PollfishParams *pollfishParams) {
            
            pollfishParams.indicatorPosition=pollfishPosition;
            pollfishParams.indicatorPadding=indPadding;
            pollfishParams.releaseMode= releaseMode;
            pollfishParams.offerwallMode= offerwallMode;
            pollfishParams.rewardMode=rewardMode;
            pollfishParams.requestUUID=requestUUID;
            pollfishParams.userAttributes=[oAttributes count]>0?oAttributes:nil;
        }];
        
        [Pollfish initWithAPIKey:[NSString stringWithUTF8String: apiKey ? apiKey : ""] andParams:pollfishParams];
        
    }
    
    void  ShowPollfishFunction(){
        [Pollfish show];
    }
    
    void  HidePollfishFunction(){
        [Pollfish hide];
        
    }
    
    bool IsPollfishPresentFunction() {
        
        return [Pollfish isPollfishPresent];
    }
    
    
    void SetEventObjectNamePollfish(const char * gameObjName) {
        
        gameObj =[[NSString stringWithUTF8String: gameObjName ? gameObjName : ""] copy];
    }
}
