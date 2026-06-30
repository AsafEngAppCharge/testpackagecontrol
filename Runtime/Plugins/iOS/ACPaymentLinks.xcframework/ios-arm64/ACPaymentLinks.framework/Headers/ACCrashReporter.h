//
//  ACCrashReporter.h
//  ACPaymentLinks
//
//  Created by Omer Cohen on 26/02/2026.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

@interface ACCrashReporter : NSObject

/// Install crash hooks once (thread-safe). Call early from SDK init.
+ (void)install;

/// Crash directory inside Library/Caches/ac_sdk_crash/crashes
+ (NSString *)crashReportsDirectory;

@end

NS_ASSUME_NONNULL_END
