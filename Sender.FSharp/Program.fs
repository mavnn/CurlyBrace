module Sender.CSharp
open System
open System.Collections.Generic
open System.IO
open System.Linq
open CsvHelper
open CsvHelper.Configuration

type CsvRow =
  {
    mutable Name : string
    mutable EmailAddress : string
    mutable Message : string
  }

type CsvRowMap() =
  inherit CsvClassMap<CsvRow>()
  do
    base.Map(fun m -> m.Name |> box).Name ("name") |> ignore
    base.Map(fun m -> m.EmailAddress |> box).Name ("email address") |> ignore
    base.Map(fun m -> m.Message |> box).Name ("message") |> ignore

type IEmailSender =
  abstract Send : string * string * string -> bool

type EmailSender() =
  interface IEmailSender with
    member x.Send (emailAddress, subject, message) =
      if String.IsNullOrWhiteSpace emailAddress then
        printfn "Email send failed...\n"
        false
      else
        printfn "Email send! To: %s\nSubject: %s\nMessage: %s"
          emailAddress
          subject
          message
        true

type ICsvProcessor =
  abstract Process : seq<CsvRow> -> int

type CsvProcessor(emailSender : IEmailSender) =
  let rowToEmail (row : CsvRow) =
    let subject = "Hi, " + row.Name
    emailSender.Send(row.EmailAddress, subject, row.Message)
  interface ICsvProcessor with
    member x.Process rows =
      rows
      |> Seq.distinct
      |> Seq.map rowToEmail
      |> Seq.filter id
      |> Seq.length

let main () =
  let sender = EmailSender()
  let processor = CsvProcessor(sender) :> ICsvProcessor
  let config = CsvConfiguration()

  config.RegisterClassMap<CsvRowMap>() |> ignore

  use reader = new StreamReader (File.Open("emails.csv", FileMode.Open))
  use csv = new CsvReader(reader, config)
  let rows = csv.GetRecords<CsvRow>()
  let sentCount = processor.Process rows
  printfn "Total Emails Sent: %d" sentCount

main ()
