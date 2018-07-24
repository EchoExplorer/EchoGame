
#import "IOSNativePopUpsManager.h"
@implementation IOSNativePopUpsManager

+(void)showTwoText: (NSString *) title message: (NSString*) msg okTitle:(NSString*) b1 fromVar:(NSString*) b2{
    
    UIAlertController *alertController = [UIAlertController alertControllerWithTitle:title message:msg preferredStyle:UIAlertControllerStyleAlert];
    
    [alertController addTextFieldWithConfigurationHandler:^(UITextField *textField) {
        textField.secureTextEntry = YES;
    }];
    
    UIAlertAction *noAction = [UIAlertAction actionWithTitle:b2 style:UIAlertActionStyleDefault handler:^(UIAlertAction * _Nonnull action) {
        [alertController dismissViewControllerAnimated:NO completion:nil];
        UnitySendMessage("Player", "switchNo", [DataConverter NSIntToChar:0]);
    }];
    
    UIAlertAction *okAction = [UIAlertAction actionWithTitle:b1 style:UIAlertActionStyleDefault handler:^(UIAlertAction * _Nonnull action) {
        [alertController dismissViewControllerAnimated:NO completion:nil];
        UnitySendMessage("Player", "setPassword", [DataConverter NSStringToChar:[[alertController textFields][0] text]]);
        UnitySendMessage("Player", "switchYes", [DataConverter NSIntToChar:0]);
    }];
    
    [alertController addAction:noAction];
    [alertController addAction:okAction];
    
    UIViewController *viewController = [[[[UIApplication sharedApplication] delegate] window] rootViewController];
    NSLayoutConstraint *constraint = [NSLayoutConstraint constraintWithItem:alertController.view attribute:NSLayoutAttributeHeight     relatedBy:NSLayoutRelationLessThanOrEqual toItem:nil attribute:NSLayoutAttributeNotAnAttribute multiplier:1 constant:viewController.view.frame.size.height*0.8f];
    [alertController.view addConstraint:constraint];
    
    [[[[UIApplication sharedApplication] keyWindow] rootViewController] presentViewController:alertController animated:YES completion:nil];
    
}

+ (void)showThree: (NSString *) title message: (NSString*) msg yesTitle:(NSString*) b1 noTitle: (NSString*) b2 naTitle: (NSString*) b3{
    
    UIAlertController *alertController = [UIAlertController alertControllerWithTitle:title message:msg preferredStyle:UIAlertControllerStyleAlert];
    UIAlertAction *yesAction = [UIAlertAction actionWithTitle:b1 style:UIAlertActionStyleDefault handler:^(UIAlertAction * _Nonnull action) {
        [alertController dismissViewControllerAnimated:NO completion:nil];
        UnitySendMessage("Player", "switchYes", [DataConverter NSIntToChar:0]);
    }];
    
    UIAlertAction *noAction = [UIAlertAction actionWithTitle:b2 style:UIAlertActionStyleDefault handler:^(UIAlertAction * _Nonnull action) {
        [alertController dismissViewControllerAnimated:NO completion:nil];
        UnitySendMessage("Player", "switchNo", [DataConverter NSIntToChar:0]);
    }];
    
    UIAlertAction *naAction = [UIAlertAction actionWithTitle:b3 style:UIAlertActionStyleDefault handler:^(UIAlertAction * _Nonnull action) {
        [alertController dismissViewControllerAnimated:NO completion:nil];
        UnitySendMessage("Player", "switchNA", [DataConverter NSIntToChar:0]);
    }];
    
    [alertController addAction:noAction];
    [alertController addAction:naAction];
    [alertController addAction:yesAction];
    
    UIViewController *viewController = [[[[UIApplication sharedApplication] delegate] window] rootViewController];
    NSLayoutConstraint *constraint = [NSLayoutConstraint constraintWithItem:alertController.view attribute:NSLayoutAttributeHeight relatedBy:NSLayoutRelationLessThanOrEqual toItem:nil attribute:NSLayoutAttributeNotAnAttribute multiplier:1 constant:viewController.view.frame.size.height*0.8f];
    [alertController.view addConstraint:constraint];
    
    [[[[UIApplication sharedApplication] keyWindow] rootViewController] presentViewController:alertController animated:YES completion:nil];
    
}

+ (void)showTwo: (NSString *) title message: (NSString*) msg yesTitle:(NSString*) b1 noTitle: (NSString*) b2{
    
    UIAlertController *alertController = [UIAlertController alertControllerWithTitle:title message:msg preferredStyle:UIAlertControllerStyleAlert];
    UIAlertAction *yesAction = [UIAlertAction actionWithTitle:b1 style:UIAlertActionStyleDefault handler:^(UIAlertAction * _Nonnull action) {
        [alertController dismissViewControllerAnimated:NO completion:nil];
        UnitySendMessage("Player", "switchYes", [DataConverter NSIntToChar:0]);
    }];
    
    UIAlertAction *noAction = [UIAlertAction actionWithTitle:b2 style:UIAlertActionStyleDefault handler:^(UIAlertAction * _Nonnull action) {
        [alertController dismissViewControllerAnimated:NO completion:nil];
        UnitySendMessage("Player", "switchNo", [DataConverter NSIntToChar:0]);
    }];
    
    [alertController addAction:noAction];
    [alertController addAction:yesAction];
    
    UIViewController *viewController = [[[[UIApplication sharedApplication] delegate] window] rootViewController];
    NSLayoutConstraint *constraint = [NSLayoutConstraint constraintWithItem:alertController.view attribute:NSLayoutAttributeHeight relatedBy:NSLayoutRelationLessThanOrEqual toItem:nil attribute:NSLayoutAttributeNotAnAttribute multiplier:1 constant:viewController.view.frame.size.height*0.8f];
    [alertController.view addConstraint:constraint];
    
    [[[[UIApplication sharedApplication] keyWindow] rootViewController] presentViewController:alertController animated:YES completion:nil];
    
}

+(void)showOne: (NSString *) title message: (NSString*) msg okTitle:(NSString*) b1 {
    
    UIAlertController *alertController = [UIAlertController alertControllerWithTitle:title message:msg preferredStyle:UIAlertControllerStyleAlert];
    
    UIAlertAction *okAction = [UIAlertAction actionWithTitle:b1 style:UIAlertActionStyleDefault handler:^(UIAlertAction * _Nonnull action) {
        [alertController dismissViewControllerAnimated:NO completion:nil];
        UnitySendMessage("Player", "switchYes", [DataConverter NSIntToChar:0]);
    }];
    
    [alertController addAction:okAction];
    
    UIViewController *viewController = [[[[UIApplication sharedApplication] delegate] window] rootViewController];
    NSLayoutConstraint *constraint = [NSLayoutConstraint constraintWithItem:alertController.view attribute:NSLayoutAttributeHeight     relatedBy:NSLayoutRelationLessThanOrEqual toItem:nil attribute:NSLayoutAttributeNotAnAttribute multiplier:1 constant:viewController.view.frame.size.height*0.8f];
    [alertController.view addConstraint:constraint];
    
    [[[[UIApplication sharedApplication] keyWindow] rootViewController] presentViewController:alertController animated:YES completion:nil];
    
}

+ (void)showTwoG: (NSString *) title message: (NSString*) msg yesTitle:(NSString*) b1 noTitle: (NSString*) b2{
    
    UIAlertController *alertController = [UIAlertController alertControllerWithTitle:title message:msg preferredStyle:UIAlertControllerStyleAlert];
    UIAlertAction *yesAction = [UIAlertAction actionWithTitle:b1 style:UIAlertActionStyleDefault handler:^(UIAlertAction * _Nonnull action) {
        [alertController dismissViewControllerAnimated:NO completion:nil];
        UnitySendMessage("GameManager", "switchYes", [DataConverter NSIntToChar:0]);
    }];
    
    UIAlertAction *noAction = [UIAlertAction actionWithTitle:b2 style:UIAlertActionStyleDefault handler:^(UIAlertAction * _Nonnull action) {
        [alertController dismissViewControllerAnimated:NO completion:nil];
        UnitySendMessage("GameManager", "switchNo", [DataConverter NSIntToChar:0]);
    }];
    
    [alertController addAction:noAction];
    [alertController addAction:yesAction];
    
    UIViewController *viewController = [[[[UIApplication sharedApplication] delegate] window] rootViewController];
    NSLayoutConstraint *constraint = [NSLayoutConstraint constraintWithItem:alertController.view attribute:NSLayoutAttributeHeight relatedBy:NSLayoutRelationLessThanOrEqual toItem:nil attribute:NSLayoutAttributeNotAnAttribute multiplier:1 constant:viewController.view.frame.size.height*0.8f];
    [alertController.view addConstraint:constraint];
    
    [[[[UIApplication sharedApplication] keyWindow] rootViewController] presentViewController:alertController animated:YES completion:nil];
    
}

+(void)showOneG: (NSString *) title message: (NSString*) msg okTitle:(NSString*) b1 {
    
    UIAlertController *alertController = [UIAlertController alertControllerWithTitle:title message:msg preferredStyle:UIAlertControllerStyleAlert];
    
    UIAlertAction *okAction = [UIAlertAction actionWithTitle:b1 style:UIAlertActionStyleDefault handler:^(UIAlertAction * _Nonnull action) {
        [alertController dismissViewControllerAnimated:NO completion:nil];
        UnitySendMessage("GameManager", "switchYes", [DataConverter NSIntToChar:0]);
    }];
    
    [alertController addAction:okAction];
    
    UIViewController *viewController = [[[[UIApplication sharedApplication] delegate] window] rootViewController];
    NSLayoutConstraint *constraint = [NSLayoutConstraint constraintWithItem:alertController.view attribute:NSLayoutAttributeHeight     relatedBy:NSLayoutRelationLessThanOrEqual toItem:nil attribute:NSLayoutAttributeNotAnAttribute multiplier:1 constant:viewController.view.frame.size.height*0.8f];
    [alertController.view addConstraint:constraint];
    
    [[[[UIApplication sharedApplication] keyWindow] rootViewController] presentViewController:alertController animated:YES completion:nil];
    
}


extern "C" {
    // Unity Call
    void _ex_ShowTwoText(char* title, char* message, char* ok, char* to) {
        [IOSNativePopUpsManager showTwoText:[DataConverter charToNSString:title] message:[DataConverter charToNSString:message] okTitle:[DataConverter charToNSString:ok] fromVar:[DataConverter charToNSString:to]];
    }
    
    void _ex_ShowThree(char* title, char* message, char* yes, char* no, char* na) {
        [IOSNativePopUpsManager showThree:[DataConverter charToNSString:title] message:[DataConverter charToNSString:message] yesTitle:[DataConverter charToNSString:yes] noTitle:[DataConverter charToNSString:no] naTitle:[DataConverter charToNSString:na]];
    }
    
    void _ex_ShowTwo(char* title, char* message, char* yes, char* no) {
        [IOSNativePopUpsManager showTwo:[DataConverter charToNSString:title] message:[DataConverter charToNSString:message] yesTitle:[DataConverter charToNSString:yes] noTitle:[DataConverter charToNSString:no]];
    }
    
    void _ex_ShowOne(char* title, char* message, char* ok) {
        [IOSNativePopUpsManager showOne:[DataConverter charToNSString:title] message:[DataConverter charToNSString:message] okTitle:[DataConverter charToNSString:ok]];
    }
    
    void _ex_ShowTwoG(char* title, char* message, char* yes, char* no) {
        [IOSNativePopUpsManager showTwoG:[DataConverter charToNSString:title] message:[DataConverter charToNSString:message] yesTitle:[DataConverter charToNSString:yes] noTitle:[DataConverter charToNSString:no]];
    }
    
    void _ex_ShowOneG(char* title, char* message, char* ok) {
        [IOSNativePopUpsManager showOneG:[DataConverter charToNSString:title] message:[DataConverter charToNSString:message] okTitle:[DataConverter charToNSString:ok]];
    }
}
@end
