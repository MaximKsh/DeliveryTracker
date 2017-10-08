using System.ComponentModel.DataAnnotations;
using DeliveryTracker.Localization;

namespace DeliveryTracker.ViewModels
{
    public class GeopositionViewModel
    {
        /// <summary>
        /// Долгота.
        /// </summary>
        [Required(ErrorMessage = LocalizationString.Error.LongitudeIsRequired)]
        public double Longitude { get; set; }
        
        /// <summary>
        /// Широта.
        /// </summary>
        [Required(ErrorMessage = LocalizationString.Error.LatitudeIsRequired)]
        public double Latitude { get; set; }
    }
}