#import <UIKit/UIKit.h>
#import "UnityAppController.h"
 
 @interface MyAppController : UnityAppController
 {
 }
 
 - (BOOL)application:(UIApplication*)application didFinishLaunchingWithOptions:(NSDictionary*)launchOptions;
 
 - (BOOL) application:(UIApplication *)application handleOpenURL:(NSURL *)url;
 
 - (BOOL) application:(UIApplication *)application openURL:(NSURL *)url sourceApplication:(NSString *)sourceApplication annotation:(id)annotation;
 
 @end
 @implementation MyAppController
 
 NSURL *launchURL = NULL;
 
 - (BOOL)application:(UIApplication*)application didFinishLaunchingWithOptions:(NSDictionary*)launchOptions
 {
     [super application:application didFinishLaunchingWithOptions:launchOptions];
     
     if ([launchOptions objectForKey:UIApplicationLaunchOptionsURLKey]) {
         NSURL *url = [launchOptions objectForKey:UIApplicationLaunchOptionsURLKey];
         
		 launchURL = url;
		 
		/* NSURLComponents *components = [NSURLComponents componentsWithURL:url resolvingAgainstBaseURL:NO];
		NSArray *queryItems = [components queryItems];

		NSMutableDictionary *dict = [NSMutableDictionary new];

		for (NSURLQueryItem *item in queryItems)
		{
			// [dict setObject:[item value] forKey:[item name]];
			
			NSString *keyName = [item name];
			NSString *keyValue = [item name];
			
			[[NSUserDefaults standardUserDefaults] setObject:keyName forKey:@keyName];
			[[NSUserDefaults standardUserDefaults] setObject:keyValue forKey:@keyValue];
		}
		
		[[NSUserDefaults standardUserDefaults] synchronize];
		*/
     }

     return YES;
 }
 
 extern "C" void _startOpenURL()
 {
     if (launchURL != NULL)
	 {
		 UnitySendMessage("AppURLSchemeHandler", "OnOpenWithUrl", [[launchURL absoluteString] UTF8String]);
	 }
 }
 
 -(BOOL) application:(UIApplication *)application handleOpenURL:(NSURL *)url
 {
     UnitySendMessage("AppURLSchemeHandler", "OnOpenWithUrl", [[url absoluteString] UTF8String]);
     
     return YES;
 }
  
 -(BOOL) application:(UIApplication *)application openURL:(NSURL *)url sourceApplication:(NSString *)sourceApplication annotation:(id)annotation
 {
     UnitySendMessage("AppURLSchemeHandler", "OnOpenWithUrl", [[url absoluteString] UTF8String]);
     return YES;
 }
 @end
 
 IMPL_APP_CONTROLLER_SUBCLASS(MyAppController)