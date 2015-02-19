//
//  ViewController.m
//  SuperTinyIOSApp
//
//  Created by Andrew Piper on 1/16/15.
//  Copyright (c) 2015 Andrew Piper. All rights reserved.
//

#include "StdAfx.h"
#import "ViewController.h"
#include "UIKitWindow.h"

@interface ViewController ()

@end

@implementation ViewController

- (void) loadView {
    uiKitView = [[UIKitView alloc] initWithFrame:[[UIScreen mainScreen] bounds]];
    self.view = uiKitView;
    self.view.opaque = YES;
    self.view.contentScaleFactor = [[UIScreen mainScreen] scale];
}

- (void)viewDidLoad {
    [super viewDidLoad];
    
    [[NSNotificationCenter defaultCenter] addObserver:self
                                             selector:@selector(keyboardWasShown:)
                                                 name:UIKeyboardDidShowNotification
                                               object:nil];
    
    [[NSNotificationCenter defaultCenter] addObserver:self
                                             selector:@selector(keyboardWillBeHidden:)
                                                 name:UIKeyboardDidHideNotification
                                               object:nil];
    
    [[NSNotificationCenter defaultCenter] addObserver:self
                                             selector:@selector(keyboardDidChangeFrame:)
                                                 name:UIKeyboardDidChangeFrameNotification
                                               object:nil];
}

- (void)didReceiveMemoryWarning {
    [super didReceiveMemoryWarning];
    // Dispose of any resources that can be recreated.
}

- (BOOL)prefersStatusBarHidden {
    return YES;
}

-(void) setWindow:(UIKitWindow*) window
{
    win = window;
    [uiKitView setWindow:window];
}

- (void)keyboardWasShown:(NSNotification*)aNotification
{
    NSDictionary* info = [aNotification userInfo];
    CGRect kbRect = [[info objectForKey:UIKeyboardFrameEndUserInfoKey] CGRectValue];
    win->onscreenKeyboardVisible(kbRect);
}

-(void)keyboardWillBeHidden:(NSNotification*)aNotification
{
    win->onscreenKeyboardHiding();
}

-(void)keyboardDidChangeFrame:(NSNotification*)aNotification
{
    NSDictionary* info = [aNotification userInfo];
    CGRect kbRect = [[info objectForKey:UIKeyboardFrameEndUserInfoKey] CGRectValue];
    win->onscreenKeyboardFrameChanged(kbRect);
}

@end
