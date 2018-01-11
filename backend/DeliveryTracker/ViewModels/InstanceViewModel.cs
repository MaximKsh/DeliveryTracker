using System.ComponentModel.DataAnnotations;
using DeliveryTracker.Localization;

namespace DeliveryTracker.ViewModels
{
    public class InstanceViewModel
    {
        /// <summary>
        /// Имя группы.
        /// </summary>
        [Required(ErrorMessage = LocalizationAlias.Error.InstanceNameIsRequired)]
        public string InstanceName { get; set; }
    }
}