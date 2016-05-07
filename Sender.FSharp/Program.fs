module Sender.CSharp
open System
open FSharp.Data

type CsvData = CsvProvider<"emails.csv">

type EmailInformation =
  {
    EmailAddress : string
    Subject : string
    Message : string
  }

type SendResult =
  | Success of Address : string
  | Failure of Reason : string

type ProcessingResult =
  {
    Sent : int
    Failed : int
  }

let sendEmail emailInfo =
  if String.IsNullOrWhiteSpace emailInfo.EmailAddress then
    printfn "Email send failed...\n"
    Failure "Missing email address"
  else
    printfn "Email send! To: %s\nSubject: %s\nMessage: %s"
      emailInfo.EmailAddress
      emailInfo.Subject
      emailInfo.Message
    Success emailInfo.EmailAddress

let processCsv emailSender rows =
  let rowToEmail (row : CsvData.Row) =
    let subject = "Hi, " + row.Name
    emailSender { EmailAddress = row.``Email address``
                  Subject = subject
                  Message = row.Message }
  let fold count result =
    match result with
    | Success _ -> { count with Sent = count.Sent + 1 }
    | Failure _ -> { count with Failed = count.Failed + 1 }
  rows
  |> Seq.distinct
  |> Seq.map rowToEmail
  |> Seq.fold fold { Sent = 0; Failed = 0 }

let processor = processCsv sendEmail
let rows = CsvData.Load "emails.csv"
let processingResults = processor rows.Rows
printfn "Total Emails Sent: %d" processingResults.Sent
printfn "Total Emails Failed: %d" processingResults.Failed
