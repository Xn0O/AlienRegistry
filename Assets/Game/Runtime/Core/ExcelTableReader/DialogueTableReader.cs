using System;
using System.Data;
using Game.Runtime.Core.Attributes;
using Game.Runtime.Data;

namespace Game.Runtime.Core.ExcelTableReader
{
    [ExcelSheet("Dialogue")]
    public class DialogueTableReader : IExcelTableReader
    {
        private const int DataStartRow = 3; //0是表头，1是类型，2是说明 

        private const int ColID = 1;
        private const int ColText = 2;
        private const int ColNext = 3;

        public void Read(DataTable table, ExcelTableContext context)
        {
            for (var i = DataStartRow; i < table.Rows.Count; i++)
            {
                var row = table.Rows[i];

                // 跳过空行
                if (ExcelCellParser.IsEmpty(row, ColID) || ExcelCellParser.IsEmpty(row, ColText))
                    continue;

                var id = ExcelCellParser.GetInt(row, ColID);

                if (context.dialogues.ContainsKey(id))
                    throw new Exception($"对话重复 dialogueId={id} ");


                var rawText = ExcelCellParser.GetString(row, ColText);
                var dialogue = new DialogueData()
                {
                    dialogueId = id,
                    lines = DialogueParser.Parse(rawText),
                    nextDialogueId = ExcelCellParser.GetInt(row, ColNext)
                };

                context.dialogues[dialogue.dialogueId] = dialogue;
            }
        }
    }
}