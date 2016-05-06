using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;

namespace Sender.CSharp
{
	class CsvRow : IEquatable<CsvRow>
	{
		public string Name
		{
			get;
			set;
		}

		public string EmailAddress
		{
			get;
			set;
		}

		public string Message
		{
			get;
			set;
		}

		// equality logic from https://msdn.microsoft.com/en-gb/library/dd183755.aspx
		public override bool Equals(object obj)
		{
			return this.Equals (obj as CsvRow);
		}

		public bool Equals(CsvRow row) 
		{
			if (Object.ReferenceEquals(row, null))
			{
				return false;
			}

			if (Object.ReferenceEquals(this, row))
			{
				return true;
			}

			if (this.GetType() != row.GetType())
			{
				return false;
			}

			return Name == row.Name && EmailAddress == row.EmailAddress && Message == row.Message;
		}

		public override int GetHashCode()
		{
			// note - this isn't the best idea in the world...
			return (Name ?? "").GetHashCode () ^ (EmailAddress ?? "").GetHashCode () ^ (Message ?? "").GetHashCode ();
		}

		public static bool operator ==(CsvRow lhs, CsvRow rhs)
		{
			if (Object.ReferenceEquals(lhs, null))
			{
				if (Object.ReferenceEquals(rhs, null))
				{
					return true;
				}

				return false;
			}

			return lhs.Equals (rhs);
		}

		public static bool operator !=(CsvRow lhs, CsvRow rhs) 
		{
			return !(lhs == rhs);
		}
	}

	class CsvRowMap : CsvClassMap<CsvRow>
	{
		public CsvRowMap()
		{
			Map (m => m.Name).Name("name");
			Map (m => m.EmailAddress).Name("email address");
			Map (m => m.Message).Name("message");
		}
	}

	interface IEmailSender
	{
		bool Send (string EmailAddress, string Subject, string Message);
	}

	class EmailSender : IEmailSender
	{
		bool IEmailSender.Send (string EmailAddress, string Subject, string Message)
		{
			if(String.IsNullOrWhiteSpace(EmailAddress)) {
				Console.WriteLine ("Email send failed...\n");
				return false;
			}
			Console.WriteLine("Email sent! To: {0}\nSubject: {1}\nMessage: {2}\n", EmailAddress, Subject, Message);
			return true;
		}
	}

	interface ICsvProcessor
	{
		int Process (IEnumerable<CsvRow> rows);
	}

	class CsvProcessor : ICsvProcessor
	{
		private IEmailSender _sender;

		public CsvProcessor(IEmailSender emailSender)
		{
			_sender = emailSender;
		}

		int ICsvProcessor.Process(IEnumerable<CsvRow> rows)
		{
			var count = 0;
			foreach (var row in rows.Distinct()) {
				var emailAddress = row.EmailAddress;
				var subject = "Hi, " + row.Name;
				var message = row.Message;
				if(_sender.Send (emailAddress, subject, message)) { count++; }
			}			
			return count;
		}
	}

	class MainClass
	{
		public static void Main (string[] args)
		{
			IEmailSender sender = new EmailSender ();
			ICsvProcessor processor = new CsvProcessor (sender);

			var config = new CsvConfiguration ();
			config.RegisterClassMap<CsvRowMap> ();

			using(var reader = new StreamReader(File.Open("emails.csv", FileMode.Open))) {
				var csv = new CsvReader(reader, config);
				var rows = csv.GetRecords<CsvRow> ();
				var sentCount = processor.Process (rows);
				Console.WriteLine ("Total Emails Sent: {0}", sentCount);
			}
		}
	}
}
