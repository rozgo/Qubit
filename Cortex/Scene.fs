namespace Cortex

open OpenTK

module Scene =

    type Location =
        {
        position : Vector3
        rotation : Quaternion
        }

    type PerspCamera =
        {
        location : Location
        nearPlane : single
        farPlane : single
        fieldOfView : single
        }

    type OrthoCamera =
        {
        location : Location
        nearPlane : single
        farPlane : single
        size : single
        }

    type Camera =
        | PerspCamera
        | OrthoCamera


    let pc = {
        location = { position = Vector3.One; rotation = Quaternion.Identity };
        nearPlane = 0.3f; farPlane = 500.0f; fieldOfView = 60.0f }
//
//    let Move<'T> (obj:'T) vel =
//        {obj with location = obj.location + vel}
//
//    let movedPC = Move<PerspCamera> pc Vector3.One
