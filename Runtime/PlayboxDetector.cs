using UnityEngine;
using System;

namespace Playbox.Purchases
{
    public class PlayboxDetector : MonoBehaviour
    {
        private static bool? _cached;

        public static bool IsPlayboxInstalled()
        {
            if (_cached.HasValue)
                return _cached.Value;

            var type = Type.GetType("Playbox.MainInitialization, Playbox");

            _cached = type != null;
            return _cached.Value;
        }
    }
}
