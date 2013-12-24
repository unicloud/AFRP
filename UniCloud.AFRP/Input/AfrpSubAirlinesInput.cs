using System.Linq;
using System.Windows;
using Microsoft.Practices.ServiceLocation;
using Telerik.Windows.DragDrop.Behaviors;
using UniCloud.AFRP.ViewModels;
using UniCloud.Fleet.Models;
using UniCloud.Fleet.Services;

namespace UniCloud.AFRP.Input
{

    #region 航空公司

    /// <summary>
    /// 拖放行为
    /// </summary>
    public class AfrpSubAirlinesMasterDragDrop : GridViewDragDropBehavior
    {
        public override bool CanStartDrag(GridViewDragDropState state)
        {
            // 当前航空公司不能拖动
            var service = ServiceLocator.Current.GetInstance<IFleetService>();
            var items = (from object item in state.DraggedItems select item).ToList();
            var airlines = items[0] as Airlines;
            return airlines != service.CurrentAirlines;
        }

        public override bool CanDrop(GridViewDragDropState state)
        {
            // 拖动的是子公司时，才可以释放。
            var items = (from object item in state.DraggedItems select item).ToList();
            var airlines = items[0] as Airlines;
            return airlines != null && airlines.MasterID != null;
        }

        public override void Drop(GridViewDragDropState state)
        {
            var viewModel = ServiceLocator.Current.GetInstance<AfrpSubsidiarySettingViewModel>();
            var items = (from object item in state.DraggedItems select item).ToList();
            var airlines = items[0] as Airlines;
            viewModel.ConvertToMasterAirlines(airlines);
        }

        public override void DragDropCompleted(GridViewDragDropState state)
        {
        }
    }

    /// <summary>
    /// 拖放展示
    /// </summary>
    public class AfrpSubAirlinesMasterDragVisual : IDragVisualProvider
    {
        public FrameworkElement CreateDragVisual(DragVisualProviderState state)
        {
            var visual = new Telerik.Windows.DragDrop.DragVisual
            {
                Content = state.DraggedItems.OfType<object>().FirstOrDefault(),
                ContentTemplate = state.Host.Resources["MasterDraggedItemTemplate"] as DataTemplate
            };
            return visual;
        }

        public Point GetDragVisualOffset(DragVisualProviderState state)
        {
            return state.RelativeStartPoint;
        }

        public bool UseDefaultCursors { get; set; }
    }

    /// <summary>
    /// 鼠标双击逻辑
    /// </summary>
    public class AfrpSubAirlinesMasterClickHelper : Utility.Input.GridViewDoubleClickHelper
    {
        protected override void GridViewDoubleClick(Telerik.Windows.Controls.GridView.GridViewCellBase cell)
        {
            var viewModel = ServiceLocator.Current.GetInstance<AfrpSubsidiarySettingViewModel>();
            var airlines = cell.DataContext as Airlines;
            viewModel.ConvertToSubAirlines(airlines);
        }

        protected override bool CanDoubleClick(Telerik.Windows.Controls.GridView.GridViewCellBase cell)
        {
            return true;
        }
    }

    #endregion

    #region 子公司

    /// <summary>
    /// 拖放行为
    /// </summary>
    public class AfrpSubAirlinesSubDragDrop : GridViewDragDropBehavior
    {
        public override bool CanStartDrag(GridViewDragDropState state)
        {
            return true;
        }

        public override bool CanDrop(GridViewDragDropState state)
        {
            // 拖动的不是子公司时，才可以释放。
            var items = (from object item in state.DraggedItems select item).ToList();
            var airlines = items[0] as Airlines;
            return airlines != null && airlines.MasterID == null;
        }

        public override void Drop(GridViewDragDropState state)
        {
            var viewModel = ServiceLocator.Current.GetInstance<AfrpSubsidiarySettingViewModel>();
            var items = (from object item in state.DraggedItems select item).ToList();
            var airlines = items[0] as Airlines;
            viewModel.ConvertToSubAirlines(airlines);
        }

        public override void DragDropCompleted(GridViewDragDropState state)
        {
        }
    }

    /// <summary>
    /// 拖放展示
    /// </summary>
    public class AfrpSubAirlinesSubDragVisual : IDragVisualProvider
    {
        public FrameworkElement CreateDragVisual(DragVisualProviderState state)
        {
            var visual = new Telerik.Windows.DragDrop.DragVisual
            {
                Content = state.DraggedItems.OfType<object>().FirstOrDefault(),
                ContentTemplate = state.Host.Resources["SubDraggedItemTemplate"] as DataTemplate
            };
            return visual;
        }

        public Point GetDragVisualOffset(DragVisualProviderState state)
        {
            return state.RelativeStartPoint;
        }

        public bool UseDefaultCursors { get; set; }
    }

    /// <summary>
    /// 鼠标双击逻辑
    /// </summary>
    public class AfrpSubAirlinesSubDoubleClickHelper : Utility.Input.GridViewDoubleClickHelper
    {
        protected override void GridViewDoubleClick(Telerik.Windows.Controls.GridView.GridViewCellBase cell)
        {
            var viewModel = ServiceLocator.Current.GetInstance<AfrpSubsidiarySettingViewModel>();
            var airlines = cell.DataContext as Airlines;
            viewModel.ConvertToMasterAirlines(airlines);
        }

        protected override bool CanDoubleClick(Telerik.Windows.Controls.GridView.GridViewCellBase cell)
        {
            return true;
        }
    }

    #endregion

}
