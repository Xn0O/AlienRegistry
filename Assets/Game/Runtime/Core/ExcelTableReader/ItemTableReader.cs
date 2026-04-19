using System;
using System.Data;
using Game.Runtime.Core.Attributes;
using Game.Runtime.Data;

namespace Game.Runtime.Core.ExcelTableReader
{
    [ExcelSheet("Item")]
    public class ItemTableReader
    {
        private const int DataStartRow = 3; //0是表头，1是类型，2是说明 

        private const int ColID = 0;
        private const int ColName = 1;
        private const int ColDesc = 2;
        private const int ColIcon = 3;

        public void Read(DataTable table, ExcelTableContext context)
        {
            for (var i = DataStartRow; i < table.Rows.Count; i++)
            {
                var row = table.Rows[i];

                // 跳过空行
                if (ExcelCellParser.IsEmpty(row, ColID))
                    continue;

                var id = ExcelCellParser.GetInt(row, ColID);

                if (context.items.ContainsKey(id))
                    throw new Exception($"道具数据重复 item_id={id} ");


                var item = new ItemData
                {
                    id = id,
                    name = ExcelCellParser.GetString(row, ColName),
                    description = ExcelCellParser.GetString(row, ColDesc),
                    iconName = ExcelCellParser.GetString(row, ColIcon)
                };

                context.items[item.id] = item;
            }
        }
    }
}