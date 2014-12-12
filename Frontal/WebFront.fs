namespace Cortex

open System

open Foundation
open UIKit
open CoreGraphics

type WebFrontControl (frame:CGRect) as this =
    inherit UIViewController ()

    let fingers = [| 0; 0; 0; 0; 0; |]
    let fs = Seq.ofArray fingers
    let clear = UIColor.FromRGBA (255, 0, 0, 255)
    let viewport = frame//CGRect ( 0.0, 0.f, frame.Width, 40.f)
    let webView = new UIWebView (frame)

    do
        this.View.Frame <- viewport
        this.View.BackgroundColor <- clear
        webView.Frame <- viewport
        webView.BackgroundColor <- clear
        this.View.AddSubview webView

    override this.ViewDidLoad () =
        base.ViewDidLoad ()
        webView.LoadRequest (new NSUrlRequest (new NSUrl ("http://google.com")))

