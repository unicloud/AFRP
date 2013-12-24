using System.Linq;
using System.Windows;
using Microsoft.Practices.ServiceLocation;
using Telerik.Windows.DragDrop.Behaviors;
using UniCloud.AFRP.ViewModels;
using UniCloud.Fleet.Models;

namespace UniCloud.AFRP.Input
{

    #region 在途申请

    /// <summary>
    /// 拖放行为
    /// </summary>
    public class AfrpApprovalEnRouteDragDrop : GridViewDragDropBehavior
    {
        public override bool CanStartDrag(GridViewDragDropState state)
        {
            var viewModel = ServiceLocator.Current.GetInstance<AfrpApprovalViewModel>();
            var items = (from object item in state.DraggedItems select item).ToList();
            var request = items[0] as Request;
            // 选中批文还未审核通过，且拖动的是已提交的申请，才允许开始拖放。
            return request != null && viewModel.SelApprovalDoc != null &&
                   viewModel.SelApprovalDoc.Status < (int)OpStatus.Checked &&
                   request.Status == (int)ReqStatus.Submited;
        }

        public override bool CanDrop(GridViewDragDropState state)
        {
            // 拖动的是已审批的申请时，才可以释放。
            var items = (from object item in state.DraggedItems select item).ToList();
            var request = items[0] as Request;
            return request != null && request.Status == (int)ReqStatus.Examined;
        }

        public override void Drop(GridViewDragDropState state)
        {
            var viewModel = ServiceLocator.Current.GetInstance<AfrpApprovalViewModel>();
            var items = (from object item in state.DraggedItems select item).ToList();
            var request = items[0] as Request;
            viewModel.RemoveRequest(request);
        }

        public override void DragDropCompleted(GridViewDragDropState state)
        {
        }
    }

    /// <summary>
    /// 拖放展示
    /// </summary>
    public class AfrpApprovalEnRouteDragVisual : IDragVisualProvider
    {
        public FrameworkElement CreateDragVisual(DragVisualProviderState state)
        {
            var visual = new Telerik.Windows.DragDrop.DragVisual
            {
                Content = state.DraggedItems.OfType<object>().FirstOrDefault(),
                ContentTemplate = state.Host.Resources["EnRouteDraggedItemTemplate"] as DataTemplate
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
    public class AfrpApprovalEnRouteDoubleClickHelper : Utility.Input.GridViewDoubleClickHelper
    {
        protected override void GridViewDoubleClick(Telerik.Windows.Controls.GridView.GridViewCellBase cell)
        {
            var viewModel = ServiceLocator.Current.GetInstance<AfrpApprovalViewModel>();
            var request = cell.DataContext as Request;
            viewModel.AddRequestToApprovalDoc(request);
        }

        protected override bool CanDoubleClick(Telerik.Windows.Controls.GridView.GridViewCellBase cell)
        {
            var viewModel = ServiceLocator.Current.GetInstance<AfrpApprovalViewModel>();
            var request = cell.DataContext as Request;
            // 选中批文还未审核通过，且双击的是已提交的申请，才允许双击。
            return request != null && viewModel.SelApprovalDoc != null &&
                   viewModel.SelApprovalDoc.Status < (int)OpStatus.Checked &&
                   request.Status == (int)ReqStatus.Submited;
        }
    }

    #endregion

    #region 批文的申请

    /// <summary>
    /// 拖放行为
    /// </summary>
    public class AfrpApprovalApproveDragDrop : GridViewDragDropBehavior
    {
        public override bool CanStartDrag(GridViewDragDropState state)
        {
            var viewModel = ServiceLocator.Current.GetInstance<AfrpApprovalViewModel>();
            // 选中批文还未审核通过，才允许开始拖放。
            return viewModel.SelApprovalDoc != null && viewModel.SelApprovalDoc.Status < (int)OpStatus.Checked;
        }

        public override bool CanDrop(GridViewDragDropState state)
        {
            // 拖动的是已提交的申请时，才可以释放。
            var items = (from object item in state.DraggedItems select item).ToList();
            var request = items[0] as Request;
            return request != null && request.Status == (int)ReqStatus.Submited;
        }

        public override void Drop(GridViewDragDropState state)
        {
            var viewModel = ServiceLocator.Current.GetInstance<AfrpApprovalViewModel>();
            var items = (from object item in state.DraggedItems select item).ToList();
            var request = items[0] as Request;
            viewModel.AddRequestToApprovalDoc(request);
        }

        public override void DragDropCompleted(GridViewDragDropState state)
        {
        }
    }

    /// <summary>
    /// 拖放展示
    /// </summary>
    public class AfrpApprovalApproveDragVisual : IDragVisualProvider
    {
        public FrameworkElement CreateDragVisual(DragVisualProviderState state)
        {
            var visual = new Telerik.Windows.DragDrop.DragVisual
            {
                Content = state.DraggedItems.OfType<object>().FirstOrDefault(),
                ContentTemplate = state.Host.Resources["ApproveDraggedItemTemplate"] as DataTemplate
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
    public class AfrpApprovalApproveDoubleClickHelper : Utility.Input.GridViewDoubleClickHelper
    {
        protected override void GridViewDoubleClick(Telerik.Windows.Controls.GridView.GridViewCellBase cell)
        {
            var viewModel = ServiceLocator.Current.GetInstance<AfrpApprovalViewModel>();
            var request = cell.DataContext as Request;
            viewModel.RemoveRequest(request);
        }

        protected override bool CanDoubleClick(Telerik.Windows.Controls.GridView.GridViewCellBase cell)
        {
            var viewModel = ServiceLocator.Current.GetInstance<AfrpApprovalViewModel>();
            // 选中批文还未审核通过，才允许双击。
            return viewModel.SelApprovalDoc != null && viewModel.SelApprovalDoc.Status < (int)OpStatus.Checked;
        }
    }

    #endregion

}
