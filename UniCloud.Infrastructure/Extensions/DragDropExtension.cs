using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;

namespace UniCloud.Infrastructure
{
    public static class DragDropExtension
    {
        public static IEnumerable<T> GetData<T>(this Microsoft.Windows.DragEventArgs args)
        {
            IEnumerable<T> results = null;

            // 从Data属性获取数据。
            ItemDragEventArgs dragEventArgs = args.Data.GetData(args.Data.GetFormats()[0]) as ItemDragEventArgs;

            if (dragEventArgs == null)
                return results;

            // 获取数据项的集合
            SelectionCollection selectionCollection = dragEventArgs.Data as SelectionCollection;
            if (selectionCollection != null)
            {
                // 转换为期望的实体集合
                results = selectionCollection.Select(selection => selection.Item).OfType<T>();
            }

            return results;
        }
    }
}
