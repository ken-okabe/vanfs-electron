module Backend

open System
open System.Net
open System.Text
open System.IO

let log = // 'a -> unit
    fun a -> printfn "%A" a

log "--------------------------"
log "F# .net start runnning."
log "--------------------------"

let url = "http://localhost:9999/"

let listener = new HttpListener()
listener.Prefixes.Add url
listener.Start()

let rec handleRequestsAsync () = async { // Async workflow computation expressionを使用
    let! context = Async.FromBeginEnd(listener.BeginGetContext, listener.EndGetContext)
    use response = context.Response

    let request = context.Request
    let responseString =
        if request.HttpMethod = "POST" then
            use reader = new StreamReader(request.InputStream)
            let message = reader.ReadToEnd() 
            $"Received message: {message}"
        else
            "Hello, World!"

    let buffer = Encoding.UTF8.GetBytes(responseString)
    response.ContentLength64 <- int64 buffer.Length
    response.ContentType <- "text/plain"
    let output = response.OutputStream
    do! Async.FromBeginEnd(buffer, 0, buffer.Length, output.BeginWrite, output.EndWrite)

    return! handleRequestsAsync ()
}

handleRequestsAsync () |> Async.Start



Console.WriteLine $"HTTP server listening on {url}"

Console.ReadLine() |> ignore