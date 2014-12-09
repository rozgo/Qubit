namespace Cortex

open System

open MonoTouch.Foundation
open MonoTouch.UIKit

type WebFrontControl (frame:Drawing.RectangleF) as this =
    inherit UIViewController ()

    let fingers = [| 0; 0; 0; 0; 0; |]
    let fs = Seq.ofArray fingers
    let clear = UIColor.FromRGBA (1.f,0.f,0.f,1.f)
    let viewport = Drawing.RectangleF (0.f, 0.f, frame.Width, 40.f)
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

