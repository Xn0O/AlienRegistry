using Game.Runtime.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

// 用于 Array 转 List

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
        private const int ColDesc = 3;
        private const int ColHomePlanet = 4;
        private const int CoPlanetOption = 5;

        // 数组字段：使用 "|" 分割
        private const int ColItemIds = 6;

        // 跨列字段：Question 1-3
        private const int ColQuestion1 = 7;
        private const int ColQuestion2 = 10;
        private const int ColQuestion3 = 13;

        // 跨列字段：关键字 1-3
        private const int ColsQuestion1 = 8;
        private const int ColsQuestion2 = 11;
        private const int ColsQuestion3 = 14;

        // 跨列字段：Answer 1-3
        private const int ColAnswer1 = 9;
        private const int ColAnswer2 = 12;
        private const int ColAnswer3 = 15;

        // 资源字段
        private const int ColGlitterPrefab = 16;
        private const int ColProfile = 17;
        private const int ColPortrait = 18;
        private const int ColFullBody = 19;
        private const int ColXray = 20;

        public void Read(DataTable table, ExcelTableContext context)
        {
            for (int i = DataStartRow; i < table.Rows.Count; i++)
            {
                DataRow row = table.Rows[i];

                // 跳过空行（通常以 ID 列是否为空为准）
                if (ExcelCellParser.IsEmpty(row, ColID))
                {
                    continue;
                }

                int id = ExcelCellParser.GetInt(row, ColID);

                if (context.characters.ContainsKey(id))
                {
                    throw new Exception($"角色数据重复 character_id={id}");
                }

                CharacterData character = new()
                {
                    id = id,
                    name = ExcelCellParser.GetString(row, ColName),
                    species = ExcelCellParser.GetString(row, ColSpecies),
                    description = ExcelCellParser.GetString(row, ColDesc),
                    homePlanet = ExcelCellParser.GetInt(row, ColHomePlanet),
                    planetOption = ExcelCellParser.GetIntArray(row, CoPlanetOption).ToList(),
                    itemIds = ExcelCellParser.GetIntArray(row, ColItemIds).ToList(),
                    questions =
                        new List<string>
                        {
                            ExcelCellParser.GetString(row, ColQuestion1),
                            ExcelCellParser.GetString(row, ColQuestion2),
                            ExcelCellParser.GetString(row, ColQuestion3)
                        },
                    shortQuestions =
                        new List<string>
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
                    glitterPrefab = ExcelCellParser.GetString(row, ColGlitterPrefab),
                    profile = ExcelCellParser.GetString(row, ColProfile),
                    portrait = ExcelCellParser.GetString(row, ColPortrait),
                    fullBody = ExcelCellParser.GetString(row, ColFullBody),
                    xray = ExcelCellParser.GetString(row, ColXray)
                };

                context.characters[character.id] = character;
            }
        }
    }
}