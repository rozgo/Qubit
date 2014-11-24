open System
open System.IO
open System.Threading
open System.Security.Permissions
open System.Net
open System.Text


let listener (handler:(HttpListenerRequest -> HttpListenerResponse -> Async<unit>)) =
    let hl = new HttpListener()
    hl.Prefixes.Add "http://*:8080/"
    hl.Start()
    let task = Async.FromBeginEnd(hl.BeginGetContext, hl.EndGetContext)
    async {
        while true do
            let! context = task
            Async.Start(handler context.Request context.Response)
    } |> Async.Start

let output (req:HttpListenerRequest) =
    if req.UrlReferrer = null then
        "No referrer!"
    else
        "Referrer: " + req.UrlReferrer.ToString()



let onFileEvent (e : FileSystemEventArgs) =
    printfn "Type: %A Path: %A" e.ChangeType e.FullPath


[<EntryPoint>]
let main argv = 

    let path = System.IO.Path.GetFullPath ("../../../Assets")

    printfn "Watching asset changes at: %A" path

    let watcher = new FileSystemWatcher ()

    watcher.Path <- path
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

    listener (fun req resp ->
        async {
            let txt = Encoding.ASCII.GetBytes(output req)
            resp.OutputStream.Write(txt, 0, txt.Length)
            resp.OutputStream.Close()
        })

    printfn "Press return to exit..."
    Console.ReadLine () |> ignore
    0
