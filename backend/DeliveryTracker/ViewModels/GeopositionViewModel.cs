using System.ComponentModel.DataAnnotations;
using DeliveryTracker.Localization;

namespace DeliveryTracker.ViewModels
{
    public class GeopositionViewModel
    {
        /// <summary>
        /// Долгота.
        /// </summary>
        [Required(ErrorMessage = LocalizationAlias.Error.LongitudeIsRequired)]
        public double Longitude { get; set; }
        
        /// <summary>
        /// Широта.
        /// </summary>
        [Required(ErrorMessage = LocalizationAlias.Error.LatitudeIsRequired)]
        public double Latitude { get; set; }
    }
}