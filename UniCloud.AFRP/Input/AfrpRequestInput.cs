using System.Linq;
using System.Windows;
using Microsoft.Practices.ServiceLocation;
using Telerik.Windows.DragDrop.Behaviors;
using UniCloud.AFRP.ViewModels;
using UniCloud.Fleet.Models;

namespace UniCloud.AFRP.Input
{

    #region 计划明细

    /// <summary>
    /// 拖放行为
    /// </summary>
    public class AfrpRequestPlanDragDrop : GridViewDragDropBehavior
    {
        public override bool CanStartDrag(GridViewDragDropState state)
        {
            var viewModel = ServiceLocator.Current.GetInstance<AfrpRequestViewModel>();
            var items = (from object item in state.DraggedItems select item).ToList();
            var planDetail = items[0] as PlanHistory;
            // 选中申请还未审核通过，且拖动的是可申请的计划明细，才允许开始拖放。
            return planDetail != null && viewModel.SelRequest != null &&
                   viewModel.SelRequest.Status < (int) ReqStatus.Checked && planDetail.CanRequest == "1：可申请";
        }

        public override bool CanDrop(GridViewDragDropState state)
        {
            // 拖动的是申请明细时，才可以释放。
            var items = (from object item in state.DraggedItems select item).ToList();
            return items[0] is ApprovalHistory;
        }

        public override void Drop(GridViewDragDropState state)
        {
            var viewModel = ServiceLocator.Current.GetInstance<AfrpRequestViewModel>();
            var items = (from object item in state.DraggedItems select item).ToList();
            var requestDetail = items[0] as ApprovalHistory;
            viewModel.RemoveRequestDetail(requestDetail);
        }

        public override void DragDropCompleted(GridViewDragDropState state)
        {
        }
    }

    /// <summary>
    /// 拖放展示
    /// </summary>
    public class AfrpRequestPlanDragVisual : IDragVisualProvider
    {
        public FrameworkElement CreateDragVisual(DragVisualProviderState state)
        {
            var visual = new Telerik.Windows.DragDrop.DragVisual
            {
                Content = state.DraggedItems.OfType<object>().FirstOrDefault(),
                ContentTemplate = state.Host.Resources["PlanDraggedItemTemplate"] as DataTemplate
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
    public class AfrpRequestPlanDoubleClickHelper : Utility.Input.GridViewDoubleClickHelper
    {
        protected override void GridViewDoubleClick(Telerik.Windows.Controls.GridView.GridViewCellBase cell)
        {
            var viewModel = ServiceLocator.Current.GetInstance<AfrpRequestViewModel>();
            var planDetail = cell.DataContext as PlanHistory;
            viewModel.AddNewRequestDetail(planDetail);
        }

        protected override bool CanDoubleClick(Telerik.Windows.Controls.GridView.GridViewCellBase cell)
        {
            var viewModel = ServiceLocator.Current.GetInstance<AfrpRequestViewModel>();
            var planDetail = cell.DataContext as PlanHistory;
            // 选中申请还未审核通过，且双击的是可申请的计划明细，才允许双击。
            return planDetail != null && viewModel.SelRequest != null &&
                               viewModel.SelRequest.Status < (int)ReqStatus.Checked && planDetail.CanRequest == "1：可申请";
        }
    }

    #endregion

    #region 申请明细

    /// <summary>
    /// 拖放行为
    /// </summary>
    public class AfrpRequestDetailDragDrop : GridViewDragDropBehavior
    {
        public override bool CanStartDrag(GridViewDragDropState state)
        {
            var viewModel = ServiceLocator.Current.GetInstance<AfrpRequestViewModel>();
            // 选中申请还未审核通过，才允许开始拖放。
            return viewModel.SelRequest != null && viewModel.SelRequest.Status < (int)ReqStatus.Checked;
        }

        public override bool CanDrop(GridViewDragDropState state)
        {
            // 拖动的是计划明细时，才可以释放。
            var items = (from object item in state.DraggedItems select item).ToList();
            return items[0] is PlanHistory;
        }

        public override void Drop(GridViewDragDropState state)
        {
            var viewModel = ServiceLocator.Current.GetInstance<AfrpRequestViewModel>();
            var items = (from object item in state.DraggedItems select item).ToList();
            var planDetail = items[0] as PlanHistory;
            viewModel.AddNewRequestDetail(planDetail);
        }

        public override void DragDropCompleted(GridViewDragDropState state)
        {
        }
    }

    /// <summary>
    /// 拖放展示
    /// </summary>
    public class AfrpRequestDetailDragVisual : IDragVisualProvider
    {
        public FrameworkElement CreateDragVisual(DragVisualProviderState state)
        {
            var visual = new Telerik.Windows.DragDrop.DragVisual
            {
                Content = state.DraggedItems.OfType<object>().FirstOrDefault(),
                ContentTemplate = state.Host.Resources["RequestDraggedItemTemplate"] as DataTemplate
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
    public class AfrpRequestDetailDoubleClickHelper : Utility.Input.GridViewDoubleClickHelper
    {
        protected override void GridViewDoubleClick(Telerik.Windows.Controls.GridView.GridViewCellBase cell)
        {
            var viewModel = ServiceLocator.Current.GetInstance<AfrpRequestViewModel>();
            var requestDetail = cell.DataContext as ApprovalHistory;
            viewModel.RemoveRequestDetail(requestDetail);
        }

        protected override bool CanDoubleClick(Telerik.Windows.Controls.GridView.GridViewCellBase cell)
        {
            var viewModel = ServiceLocator.Current.GetInstance<AfrpRequestViewModel>();
            // 选中申请还未审核通过，才允许双击。
            return viewModel.SelRequest != null && viewModel.SelRequest.Status < (int)ReqStatus.Checked;
        }
    }

    #endregion

}
