using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace YoutubeMusicBot.Console.Models
{
    public record TracksList(params TrackModel[] Tracks) : IReadOnlyList<TrackModel>
    {
        public IEnumerator<TrackModel> GetEnumerator() =>
            Tracks.AsEnumerable().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            Tracks.GetEnumerator();

        public int Count => Tracks.Length;

        public TrackModel this[int index] => Tracks[index];
    }
}
