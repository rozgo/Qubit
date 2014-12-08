module Definition.Building

open System


let maxLevel = 21

let growthCurve x max power amount =
    int ((Math.Pow (float x / float max, float power)) * float amount)

let goldCost level = growthCurve level maxLevel 3 100000

