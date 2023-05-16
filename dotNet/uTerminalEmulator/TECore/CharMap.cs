using System;
using System.Collections.Generic;
using UnityEngine;

namespace TerminalEmulator
{
    [Serializable]
    public class CharMap
    {
        [NonSerialized]
        public Dictionary<char , Vector2Int> Map;
        public KVDict<char , Vector2Int> CharsPos;
        public Texture2D Texture;
        public int TWidth;
        public int THeight;
        public void Init()
        {
            Map = CharsPos.ToDictionary();
        }
    }
}
