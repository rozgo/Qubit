open System
open System.IO
open System.Threading
open System.Security.Permissions
open System.Net
//open System.Net.WebSockets
open System.Text


let httpListener assetPath (handler:(string -> HttpListenerRequest -> HttpListenerResponse -> Async<unit>)) =
    let hl = new HttpListener()
    hl.Prefixes.Add "http://*:8080/"
    hl.Start()
    let task = Async.FromBeginEnd (hl.BeginGetContext, hl.EndGetContext)
    async {
        while true do
            let! context = task
            Async.Start (handler assetPath context.Request context.Response)
    } |> Async.Start

let serveAsset assetPath (request:HttpListenerRequest) (response:HttpListenerResponse) = async {
    let filePath = assetPath + request.Url.AbsolutePath
    let fileInfo = new FileInfo (filePath)
    use fs = new FileStream (filePath, FileMode.Open)
    use br = new BinaryReader (fs)
    let bytes = br.ReadBytes (int fileInfo.Length)
    response.ContentType <- Mime.MediaTypeNames.Application.Octet
//    response.ContentType <- "image/jpg"
    response.ContentLength64 <- fileInfo.Length
    let stream = response.OutputStream
    do! Async.FromBeginEnd (bytes, 0, int fileInfo.Length, (fun (bytes, offset, count, callback, state) ->
        stream.BeginWrite (bytes, offset, count, callback, state)), stream.EndWrite)
    printfn "Sent bytes %A" (int fileInfo.Length) }



let httpHandler assetPath (request:HttpListenerRequest) (response:HttpListenerResponse) =
    async {
        printfn "httpHandler GET %A" request.Url.AbsolutePath
        do! serveAsset assetPath request response
        response.OutputStream.Close ()
    }




let onFileEvent (e : FileSystemEventArgs) =
    printfn "Type: %A Path: %A" e.ChangeType e.FullPath





[<EntryPoint>]
let main argv = 

    let assetPath = System.IO.Path.GetFullPath ("../../../Assets")

    printfn "Watching asset changes at: %A" assetPath

    let watcher = new FileSystemWatcher ()

    watcher.Path <- assetPath
    watcher.IncludeSubdirectories <- true

    watcher.NotifyFilter <-
//        NotifyFilters.LastAccess    |||
        NotifyFilters.LastWrite     |||
        NotifyFilters.FileName      |||
        NotifyFilters.DirectoryName
//        NotifyFilters.Size
    watcher.Filter <- "*.*"

    watcher.Created.Add onFileEvent
    watcher.Deleted.Add onFileEvent
    watcher.Changed.Add onFileEvent
    watcher.Renamed.Add onFileEvent

    watcher.EnableRaisingEvents <- true

    httpListener assetPath httpHandler

    printfn "Press return to exit..."
    Console.ReadLine () |> ignore
    0
    