module Service.Logging

open System.IO
open Serilog.Events
open Serilog.Parsing
open Newtonsoft.Json
open Serilog
open Microsoft.FSharp.Reflection
open System

// (c) by Isaac Abraham https://gist.github.com/isaacabraham/ba679f285bfd15d2f53e
type IdiomaticDuConverter() = 
    inherit JsonConverter()
    
    [<Literal>]
    let discriminator = "__Case"
    let primitives = Set [ JsonToken.Boolean; JsonToken.Date; JsonToken.Float; JsonToken.Integer; JsonToken.Null; JsonToken.String ]

    let writeValue (value:obj) (serializer:JsonSerializer, writer : JsonWriter) =
        if value.GetType().IsPrimitive then writer.WriteValue value
        else serializer.Serialize(writer, value)

    let writeProperties (fields : obj array) (serializer:JsonSerializer, writer : JsonWriter) = 
        fields |> Array.iteri (fun index value -> 
                      writer.WritePropertyName(sprintf "Item%d" index)
                      (serializer, writer) |> writeValue value)
    
    let writeDiscriminator (name : string) (writer : JsonWriter) = 
        writer.WritePropertyName discriminator
        writer.WriteValue name
        
    override __.WriteJson(writer, value, serializer) = 
        let unionCases = FSharpType.GetUnionCases(value.GetType())
        let unionType = value.GetType()
        let case, fields = FSharpValue.GetUnionFields(value, unionType)
        let allCasesHaveValues = unionCases |> Seq.forall (fun c -> c.GetFields() |> Seq.length > 0)

        match unionCases.Length, fields, allCasesHaveValues with
        | 2, [||], false -> writer.WriteNull()
        | 1, [| singleValue |], _
        | 2, [| singleValue |], false -> (serializer, writer) |> writeValue singleValue
        | 1, fields, _
        | 2, fields, false -> 
            writer.WriteStartObject()
            (serializer, writer) |> writeProperties fields
            writer.WriteEndObject()
        | _ -> 
            writer.WriteStartObject()
            writer |> writeDiscriminator case.Name
            (serializer, writer) |> writeProperties fields
            writer.WriteEndObject()
    
    override __.ReadJson(reader, destinationType, _, _) = 
        let parts = 
            if reader.TokenType <> JsonToken.StartObject then [| (JsonToken.Undefined, obj()), (reader.TokenType, reader.Value) |]
            else 
                seq { 
                    yield! reader |> Seq.unfold (fun reader -> 
                                         if reader.Read() then Some((reader.TokenType, reader.Value), reader)
                                         else None)
                }
                |> Seq.takeWhile(fun (token, _) -> token <> JsonToken.EndObject)
                |> Seq.pairwise
                |> Seq.mapi (fun id value -> id, value)
                |> Seq.filter (fun (id, _) -> id % 2 = 0)
                |> Seq.map snd
                |> Seq.toArray
        
        let values = 
            parts
            |> Seq.filter (fun ((_, keyValue), _) -> keyValue <> (discriminator :> obj))
            |> Seq.map snd
            |> Seq.filter (fun (valueToken, _) -> primitives.Contains valueToken)
            |> Seq.map snd
            |> Seq.toArray
        
        let case = 
            let unionCases = FSharpType.GetUnionCases(destinationType)
            let unionCase =
                parts
                |> Seq.tryFind (fun ((_,keyValue), _) -> keyValue = (discriminator :> obj))
                |> Option.map (snd >> snd)
            match unionCase with
            | Some case -> unionCases |> Array.find (fun f -> f.Name :> obj = case)
            | None ->
                // implied union case
                match values with
                | [| null |] -> unionCases |> Array.find(fun c -> c.GetFields().Length = 0)
                | _ -> unionCases |> Array.find(fun c -> c.GetFields().Length > 0)
        
        let values = 
            case.GetFields()
            |> Seq.zip values
            |> Seq.map (fun (value, propertyInfo) -> Convert.ChangeType(value, propertyInfo.PropertyType))
            |> Seq.toArray
        
        FSharpValue.MakeUnion(case, values)
    
    override __.CanConvert(objectType) = FSharpType.IsUnion objectType

type LogLine =
    {
        time : string
        level : string
        msg : string
        tokens : string list option
        ``exception`` : string option
        properties : Map<string,obj> option
    }

type CustomJsonFormatter() =
    let _jsonSerializer =
        let jsonSerializerSettings = JsonSerializerSettings()
        jsonSerializerSettings.NullValueHandling <- NullValueHandling.Ignore
        jsonSerializerSettings.Converters.Add(IdiomaticDuConverter())
        JsonSerializer.Create(jsonSerializerSettings)

    interface Serilog.Formatting.ITextFormatter with 
        member this.Format (logEvent : LogEvent, output: TextWriter) =
            this.FormatEvent(logEvent, output);
            output.WriteLine();

    member this.FormatEvent(logEvent : LogEvent, output : TextWriter) =
        let renderToken (token: MessageTemplateToken) =
            let writer = new StringWriter()
            token.Render(logEvent.Properties, writer)
            writer.ToString()

        let tokensWithFormat =
            logEvent.MessageTemplate.Tokens
                |> Seq.choose (fun t ->
                    match t with
                    | :? PropertyToken as token -> Some(token)
                    | _ -> None)
                |> Seq.filter (fun pt -> not (isNull pt.Format))
                |> Seq.map renderToken
                |> Seq.toList

        let rec propertyValue (p : LogEventPropertyValue) =
            match p with
            | :? ScalarValue as v -> v.Value
            | :? DictionaryValue as v ->
                Seq.zip
                    (v.Elements.Keys |> Seq.map (fun x -> x.Value.ToString()))
                    (v.Elements.Values |> Seq.map propertyValue)
                |> Map.ofSeq :> obj
            | :? SequenceValue as v ->
                v.Elements
                |> Seq.map propertyValue
                |> List.ofSeq :> obj
            | :? StructureValue as v ->
                v.Properties
                |> Seq.map (fun p -> p.Name, propertyValue p.Value)
                |> Map.ofSeq :> obj
            | _ -> failwith "unknown property value type"

        let properties =
            logEvent.Properties
                |> Seq.map (fun p -> (if p.Key.StartsWith("@") then "@" + p.Key else p.Key), propertyValue p.Value)
                |> Map.ofSeq

        let line = {
            time = logEvent.Timestamp.UtcDateTime.ToString("O")
            level = logEvent.Level.ToString().ToUpper()
            msg = logEvent.MessageTemplate.Text
            tokens = if tokensWithFormat.IsEmpty then None else Some(tokensWithFormat)
            ``exception`` = logEvent.Exception
                |> Option.ofObj
                |> Option.map (fun x -> x.ToString())
            properties = if properties.IsEmpty then None else Some(properties)
        }

        _jsonSerializer.Serialize(output, line)

let initLogger level = 
    Log.Logger <- LoggerConfiguration()
      .Enrich.FromLogContext()
      .WriteTo.Console(formatter = CustomJsonFormatter())
      .MinimumLevel.Is(level)
      .CreateLogger();
