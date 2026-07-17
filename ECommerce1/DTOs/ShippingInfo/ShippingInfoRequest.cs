namespace ECommerce1.DTOs.ShippingInfo
{
    public class ShippingInfoRequest
    {
        public string RecipientName { get; set; }
        public string PhoneNumber { get; set; }
        public string AddressLine { get; set; }
        public string WardId { get; set; }
        public bool IsDefault { get; set; }
        public double? Latitude { get; set; }  // Tọa độ từ Goong Maps
        public double? Longitude { get; set; } // Tọa độ từ Goong Maps
    }
}
