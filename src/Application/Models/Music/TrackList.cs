using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace YoutubeMusicBot.Application.Models.Music
{
    public record TrackList(params Track[] Tracks) : IReadOnlyList<Track>
    {
        public IEnumerator<Track> GetEnumerator() =>
            Tracks.AsEnumerable().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            Tracks.GetEnumerator();

        public int Count => Tracks.Length;

        public Track this[int index] => Tracks[index];
    }
}
