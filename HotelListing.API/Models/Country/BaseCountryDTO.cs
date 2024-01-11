using System.ComponentModel.DataAnnotations;

namespace HotelListing.API.Models.Country
{
    public abstract class BaseCountryDTO //abstract class means you can't instantiate them/initialise as standalone objects and usually used for inheritance purposes 
    {
        [Required] //data validation (doesn't apply for the readonly Get method, only the Create/Update
        public string Name { get; set; }
        public string ShortName { get; set; }
    }
}
