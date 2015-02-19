//
//  UIKitView.m
//  OSHelper
//
//  Created by Andrew Piper on 2/19/15.
//  Copyright (c) 2015 Andrew Piper. All rights reserved.
//

#include "StdAfx.h"
#import <Foundation/Foundation.h>
#include "UIKitView.h"
#include "UIKitWindow.h"

@implementation UIKitView

- (NSString *)description
{
    return [NSString stringWithFormat:@"UIKitView frame dimensions x: %.0f y: %.0f w: %.0f h: %.0f",
            [self frame].origin.x,
            [self frame].origin.y,
            [self frame].size.width,
            [self frame].size.height];
}

+ (Class)layerClass
{
    return [CAEAGLLayer class];
}

- (void)layoutSubviews
{
    win->fireSized();
}

-(void) setWindow:(UIKitWindow*) window
{
    win = window;
    pendingResize = false;
}

-(void) firePendingResize
{
    if(pendingResize)
    {
        pendingResize = false;
        self.frame = newFrame;
        //We do an extra firesized here, this makes sure this goes through this frame
        win->fireSized();
    }
}

//The code to not get stretched frames (or at least reduce them as much as possible
//is a bit complicated. We must set the frame resizes we want using this function
//then before each update firePendingResize is called. If there is a resize to do
//it is done and then we update the program's idle normally. This mostly eliminates
//the stretched out frames unless they occur on a slow frame and we miss the update
//but I'm not sure we can do anything about that.
-(void) setFrameForNextUpdate:(CGRect) frame
{
    pendingResize = true;
    newFrame = frame;
}

@end
