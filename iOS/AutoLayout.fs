module Praeclarum.AutoLayout

open System
open Foundation
open UIKit
type NativeView = UIView

/// A first-class value representing a layout attribute on a view.
/// Algebraic operations +, -, *, and / are supported to modify the attribute.
/// Use ==, <==, and >== to create constraints.
/// Use @@ to set the priority of the constraint (1000 is the default).
type LayoutAttributeReference =
    {
        View : NativeView
        Attribute : NSLayoutAttribute
        M : nfloat
        C : nfloat
        P : float32
    }
    static member CreateConstraint a b r =
        let m, c, p = b.M / a.M, (b.C - a.C) / a.M, Math.Min (a.P, b.P)
        NSLayoutConstraint.Create (a.View :> NSObject, a.Attribute, r, b.View :> NSObject, b.Attribute, m, c, Priority = p)
    static member ( == ) (a, b) = LayoutAttributeReference.CreateConstraint a b NSLayoutRelation.Equal
    static member ( >== ) (a, b) = LayoutAttributeReference.CreateConstraint a b NSLayoutRelation.GreaterThanOrEqual
    static member ( <== ) (a, b) = LayoutAttributeReference.CreateConstraint a b NSLayoutRelation.LessThanOrEqual
    static member ( @@ ) (r, p) = { r with P = p }
    static member ( * ) (m : nfloat, r) = { r with M = r.M * m; C = r.C * m }
    static member ( * ) (r, m : nfloat) = { r with M = r.M * m; C = r.C * m }
    static member ( / ) (r, m : nfloat) = { r with M = r.M / m; C = r.C / m }
    static member ( + ) (c : nfloat, r) = { r with C = r.C + c }
    static member ( + ) (r, c : nfloat) = { r with C = r.C + c }
    static member ( - ) (c : nfloat, r) = { r with M = -r.M; C = c - r.C }
    static member ( - ) (r, c : nfloat) = { r with C = r.C - c }

type UIView with
    member this.VerticalHuggingPriority 
        with get () : float32 = this.ContentHuggingPriority (UILayoutConstraintAxis.Vertical)
        and set v = this.SetContentHuggingPriority (v, UILayoutConstraintAxis.Vertical)
    member this.HorizontalHuggingPriority 
        with get () : float32 = this.ContentHuggingPriority (UILayoutConstraintAxis.Horizontal)
        and set v = this.SetContentHuggingPriority (v, UILayoutConstraintAxis.Horizontal)

    member this.LayoutBaseline = { View = this; Attribute = NSLayoutAttribute.Baseline; M = nfloat 1.0; C = nfloat 0.0; P = 1000.0f }
    member this.LayoutBottom = { View = this; Attribute = NSLayoutAttribute.Bottom; M = nfloat 1.0; C = nfloat 0.0; P = 1000.0f }
    member this.LayoutCenterX = { View = this; Attribute = NSLayoutAttribute.CenterX; M = nfloat 1.0; C = nfloat 0.0; P = 1000.0f }
    member this.LayoutCenterY = { View = this; Attribute = NSLayoutAttribute.CenterY; M = nfloat 1.0; C = nfloat 0.0; P = 1000.0f }
    member this.LayoutHeight = { View = this; Attribute = NSLayoutAttribute.Height; M = nfloat 1.0; C = nfloat 0.0; P = 1000.0f }
    member this.LayoutLeading = { View = this; Attribute = NSLayoutAttribute.Leading; M = nfloat 1.0; C = nfloat 0.0; P = 1000.0f }
    member this.LayoutLeft = { View = this; Attribute = NSLayoutAttribute.Left; M = nfloat 1.0; C = nfloat 0.0; P = 1000.0f }
    member this.LayoutRight = { View = this; Attribute = NSLayoutAttribute.Right; M = nfloat 1.0; C = nfloat 0.0; P = 1000.0f }
    member this.LayoutTop = { View = this; Attribute = NSLayoutAttribute.Top; M = nfloat 1.0; C = nfloat 0.0; P = 1000.0f }
    member this.LayoutTrailing = { View = this; Attribute = NSLayoutAttribute.Trailing; M = nfloat 1.0; C = nfloat 0.0; P = 1000.0f }
    member this.LayoutWidth = { View = this; Attribute = NSLayoutAttribute.Width; M = nfloat 1.0; C = nfloat 0.0; P = 1000.0f }

        