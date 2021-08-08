using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace YoutubeMusicBot.Console.Models
{
    public record InlineButtonCollection(IEnumerable<IEnumerable<InlineButton>> InlineButtons)
        : IEnumerable<InlineButton>
    {
        public InlineButtonCollection(params InlineButton[] inlineButtons)
            : this(new[] { inlineButtons })
        {
        }

        public InlineButtonCollection(IEnumerable<InlineButton> inlineButtons)
            : this(inlineButtons.ToArray())
        {
        }

        public IEnumerator<InlineButton> GetEnumerator()
        {
            foreach (var inlineButtons in InlineButtons)
            foreach (var inlineButton in inlineButtons)
                yield return inlineButton;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString() =>
            string.Join(Environment.NewLine, this);
    }
}
