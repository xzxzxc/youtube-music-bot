using IntegrationTests.Common;
using TLSharp.Core;

var client = new TelegramClient(Secrets.AppApiId, Secrets.AppApiHash);
await client.ConnectAsync();

var hash = await client.SendCodeRequestAsync(Secrets.UserPhoneNumber);
System.Console.WriteLine("Please enter code from telegram.");
var code = System.Console.ReadLine();

await client.MakeAuthAsync(Secrets.UserPhoneNumber, hash, code);
