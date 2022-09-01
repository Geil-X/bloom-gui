module Gui.DataTypes.Webcam



open SeeShark.Device
open SeeShark.FFmpeg

/// Only works on Windows
let getAllConnectedCameras () : string seq =
    use manager = new CameraManager()

    FFmpegManager.SetupFFmpeg()

    manager.Devices
    |> Seq.cast<CameraInfo>
    |> Seq.map (fun device -> device.ToString())
