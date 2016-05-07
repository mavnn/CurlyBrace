module Sender.CSharp
open System
open FSharp.Data

type CsvData = CsvProvider<"emails.csv">

let sendEmail emailAddress subject message =
  if String.IsNullOrWhiteSpace emailAddress then
    printfn "Email send failed...\n"
    false
  else
    printfn "Email send! To: %s\nSubject: %s\nMessage: %s"
      emailAddress
      subject
      message
    true

let processCsv emailSender rows =
  let rowToEmail (row : CsvData.Row) =
    let subject = "Hi, " + row.Name
    emailSender row.``Email address`` subject row.Message
  rows
  |> Seq.distinct
  |> Seq.map rowToEmail
  |> Seq.filter id
  |> Seq.length

let main () =
  let processor = processCsv sendEmail

  let rows = CsvData.Load "emails.csv"
  let sentCount = processor rows.Rows
  printfn "Total Emails Sent: %d" sentCount

main ()
