using System.Collections.Generic;

namespace DeliveryTracker.ViewModels
{
    public class ErrorListViewModel
    {
        /// <summary>
        /// Список ошибок.
        /// </summary>
        public ICollection<ErrorItemViewModel> Errors { get; set; } = new List<ErrorItemViewModel>();
    }
}