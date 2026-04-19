using System;
using System.Collections.Generic;
using System.Data;
using System.Linq; // 用于 Array 转 List
using Game.Runtime.Core.Attributes;
using Game.Runtime.Data;

namespace Game.Runtime.Core.ExcelTableReader
{
    [ExcelSheet("Character")]
    public class CharacterTableReader : IExcelTableReader
    {
        private const int DataStartRow = 3; // 0是表头，1是类型，2是说明 

        // 基础字段列索引
        private const int ColID = 0;
        private const int ColName = 1;
        private const int ColSpecies = 2;
        private const int ColDesc = 4;
        private const int ColHomePlanet = 5;
        private const int CoPlanetOption = 6;

        // 数组字段：使用 "|" 分割
        private const int ColItemIds = 7;

        // 跨列字段：Question 1-3
        private const int ColQuestion1 = 8;
        private const int ColQuestion2 = 11;
        private const int ColQuestion3 = 14;

        // 跨列字段：关键字 1-3
        private const int ColsQuestion1 = 9;
        private const int ColsQuestion2 = 12;
        private const int ColsQuestion3 = 15;

        // 跨列字段：Answer 1-3
        private const int ColAnswer1 = 10;
        private const int ColAnswer2 = 13;
        private const int ColAnswer3 = 16;

        // 资源字段
        private const int ColPortrait = 20;
        private const int ColFullBody = 21;
        private const int ColXray = 22;

        public void Read(DataTable table, ExcelTableContext context)
        {
            for (var i = DataStartRow; i < table.Rows.Count; i++)
            {
                var row = table.Rows[i];

                // 跳过空行（通常以 ID 列是否为空为准）
                if (ExcelCellParser.IsEmpty(row, ColID))
                    continue;

                var id = ExcelCellParser.GetInt(row, ColID);

                if (context.characters.ContainsKey(id))
                    throw new Exception($"角色数据重复 character_id={id}");

                var character = new CharacterData()
                {
                    id = id,
                    name = ExcelCellParser.GetString(row, ColName),
                    species = ExcelCellParser.GetString(row, ColSpecies),
                    description = ExcelCellParser.GetString(row, ColDesc),
                    homePlanet = ExcelCellParser.GetString(row, ColHomePlanet),
                    planetOption = ExcelCellParser.GetIntArray(row, CoPlanetOption).ToList(),

                    itemIds = ExcelCellParser.GetIntArray(row, ColItemIds).ToList(),

                    questions = new List<string>
                    {
                        ExcelCellParser.GetString(row, ColQuestion1),
                        ExcelCellParser.GetString(row, ColQuestion2),
                        ExcelCellParser.GetString(row, ColQuestion3)
                    },

                    shortQuestions = new List<string>
                    {
                        ExcelCellParser.GetString(row, ColsQuestion1),
                        ExcelCellParser.GetString(row, ColsQuestion2),
                        ExcelCellParser.GetString(row, ColsQuestion3)
                    },

                    answers = new List<string>
                    {
                        ExcelCellParser.GetString(row, ColAnswer1),
                        ExcelCellParser.GetString(row, ColAnswer2),
                        ExcelCellParser.GetString(row, ColAnswer3)
                    },

                    portrait = ExcelCellParser.GetString(row, ColPortrait),
                    fullBody = ExcelCellParser.GetString(row, ColFullBody),
                    xray = ExcelCellParser.GetString(row, ColXray)
                };

                context.characters[character.id] = character;
            }
        }
    }
}