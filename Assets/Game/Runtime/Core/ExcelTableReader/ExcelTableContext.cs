using System;
using System.Collections.Generic;
using Game.Runtime.Data;

namespace Game.Runtime.Core.ExcelTableReader
{
    [Serializable]
    public class ExcelTableContext
    {
        public Dictionary<int, DialogueData> dialogues = new Dictionary<int, DialogueData>(); 
    }
}