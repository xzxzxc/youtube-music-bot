using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TeleSharp.TL;
using TeleSharp.TL.Messages;
using TLSharp.Core;
using TLSharp.Core.Network.Exceptions;

namespace YoutubeMusicBot.AcceptanceTest.Extensions
{
    public static class TelegramClientExtension
    {
        public static async Task<IEnumerable<TLMessage>> GetHistoryMessages(
            this TelegramClient telegramClient,
            TLAbsInputPeer peer,
            int? offsetId = null,
            int? offsetDate = null,
            int? addOffset = null,
            int? limit = null,
            int? maxId = null,
            int? minId = null,
            CancellationToken token = default)
        {
            try
            {
                var res = await telegramClient.GetHistoryAsync(
                    peer,
                    offsetId ?? 0,
                    offsetDate ?? 0,
                    addOffset ?? 0,
                    limit ?? 100,
                    maxId ?? 0,
                    minId ?? 0,
                    token);

                return res switch
                {
                    TLMessagesSlice messagesSlice => messagesSlice.Messages.OfType<TLMessage>(),
                    TLMessages messages => messages.Messages.OfType<TLMessage>(),
                    _ => throw new InvalidOperationException("Unknown messages type"),
                };
            }
            catch (FloodException floodException)
            {
                await Task.Delay(floodException.TimeToWait, token);

                return await GetHistoryMessages(
                    telegramClient,
                    peer,
                    offsetId,
                    offsetDate,
                    addOffset,
                    limit,
                    maxId,
                    minId,
                    token);
            }
        }
    }
}
