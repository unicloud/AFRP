using System.Linq;
using System.Windows;
using Microsoft.Practices.ServiceLocation;
using Telerik.Windows.DragDrop.Behaviors;
using UniCloud.AFRP.ViewModels;
using UniCloud.Fleet.Models;

namespace UniCloud.AFRP.Input
{

    #region 计划飞机

    /// <summary>
    /// 拖放行为
    /// </summary>
    public class AfrpPlanLayPlanDragDrop : GridViewDragDropBehavior
    {
        public override bool CanStartDrag(GridViewDragDropState state)
        {
            var viewModel = ServiceLocator.Current.GetInstance<AfrpPlanLayViewModel>();
            // 当前计划不为空且还未审核通过的，才允许开始拖放。
            return viewModel.CurrentPlan != null && viewModel.CurrentPlan.Status < (int)PlanStatus.Checked;
        }

        public override bool CanDrop(GridViewDragDropState state)
        {
            return false;
        }

        public override void DragDropCompleted(GridViewDragDropState state)
        {
        }
    }

    /// <summary>
    /// 拖放展示
    /// </summary>
    public class AfrpPlanLayPlanDragVisual : IDragVisualProvider
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
    public class AfrpPlanLayPlanDoubleClickHelper : Utility.Input.GridViewDoubleClickHelper
    {
        protected override void GridViewDoubleClick(Telerik.Windows.Controls.GridView.GridViewCellBase cell)
        {
            var viewModel = ServiceLocator.Current.GetInstance<AfrpPlanLayViewModel>();
            var planAircraft = cell.DataContext as PlanAircraft;
            viewModel.OpenEditDialog(planAircraft, PlanDetailCreateSource.PlanAircraft);
        }

        protected override bool CanDoubleClick(Telerik.Windows.Controls.GridView.GridViewCellBase cell)
        {
            var viewModel = ServiceLocator.Current.GetInstance<AfrpPlanLayViewModel>();
            // 当前计划不为空且还未审核通过的，才允许双击。
            return viewModel.CurrentPlan != null && viewModel.CurrentPlan.Status < (int)PlanStatus.Checked;
        }
    }

    #endregion

    #region 运营中的飞机

    public class AfrpPlanLayOperationDragDrop : GridViewDragDropBehavior
    {
        public override bool CanStartDrag(GridViewDragDropState state)
        {
            var viewModel = ServiceLocator.Current.GetInstance<AfrpPlanLayViewModel>();
            // 当前计划不为空且还未审核通过的，才允许开始拖放。
            return viewModel.CurrentPlan != null && viewModel.CurrentPlan.Status < (int)PlanStatus.Checked;
        }

        public override bool CanDrop(GridViewDragDropState state)
        {
            return false;
        }

        public override void DragDropCompleted(GridViewDragDropState state)
        {
        }
    }

    /// <summary>
    /// 拖放展示
    /// </summary>
    public class AfrpPlanLayOperationDragVisual : IDragVisualProvider
    {
        public FrameworkElement CreateDragVisual(DragVisualProviderState state)
        {
            var visual = new Telerik.Windows.DragDrop.DragVisual
            {
                Content = state.DraggedItems.OfType<object>().FirstOrDefault(),
                ContentTemplate = state.Host.Resources["OperationDraggedItemTemplate"] as DataTemplate
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
    public class AfrpPlanLayOperationDoubleClickHelper : Utility.Input.GridViewDoubleClickHelper
    {
        protected override void GridViewDoubleClick(Telerik.Windows.Controls.GridView.GridViewCellBase cell)
        {
            var viewModel = ServiceLocator.Current.GetInstance<AfrpPlanLayViewModel>();
            var aircraft = cell.DataContext as Aircraft;
            if (aircraft != null)
            {
                var planAircraft = aircraft.PlanAircrafts.FirstOrDefault(pa => pa.IsOwn);
                viewModel.OpenEditDialog(planAircraft, PlanDetailCreateSource.Aircraft);
            }
        }

        protected override bool CanDoubleClick(Telerik.Windows.Controls.GridView.GridViewCellBase cell)
        {
            var viewModel = ServiceLocator.Current.GetInstance<AfrpPlanLayViewModel>();
            // 当前计划不为空且还未审核通过的，才允许双击。
            return viewModel.CurrentPlan != null && viewModel.CurrentPlan.Status < (int)PlanStatus.Checked;
        }
    }

    #endregion

    #region 当前计划明细

    public class AfrpPlanLayCurrentPlanDetail : GridViewDragDropBehavior
    {
        public override bool CanStartDrag(GridViewDragDropState state)
        {
            return false;
        }

        public override bool CanDrop(GridViewDragDropState state)
        {
            return true;
        }

        public override void Drop(GridViewDragDropState state)
        {
            var viewModel = ServiceLocator.Current.GetInstance<AfrpPlanLayViewModel>();
            var items = (from object item in state.DraggedItems select item).ToList();
            if (items.Any())
            {
                if (items[0] is PlanAircraft)
                {
                    var planAircraft = items.Select(pa => pa as PlanAircraft).FirstOrDefault();
                    viewModel.OpenEditDialog(planAircraft, PlanDetailCreateSource.PlanAircraft);
                }
                else if (items[0] is Aircraft)
                {
                    var planAircraft = items.SelectMany(a =>
                        {
                            var aircraft = a as Aircraft;
                            return aircraft != null ? aircraft.PlanAircrafts : null;
                        }).FirstOrDefault(pa => pa.IsOwn);
                    viewModel.OpenEditDialog(planAircraft, PlanDetailCreateSource.Aircraft);
                }
            }
        }
    }

    #endregion

}
