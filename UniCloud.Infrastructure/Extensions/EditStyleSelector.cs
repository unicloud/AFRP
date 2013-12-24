using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace UniCloud.Infrastructure.Extensions
{
    public class EditStyleSelector : StyleSelector
    {
        public override Style SelectStyle(object item, DependencyObject container)
        {
            var cell = container as GridViewCell;
            if (cell == null) return ReadStyle;
            var column = cell.Column as GridViewBoundColumnBase;
            var q = column.IsReadOnly;
            var q1 = column.IsReadOnlyBinding;
            
            return cell.Column.IsReadOnly ? ReadStyle : EditStyle;
        }

        public Style EditStyle { get; set; }
        public Style ReadStyle { get; set; }
    }
}
