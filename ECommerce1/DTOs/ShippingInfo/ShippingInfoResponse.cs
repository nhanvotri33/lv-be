using System;

namespace ECommerce1.DTOs.ShippingInfo
{
    public class ShippingInfoResponse
    {
        public int Id { get; set; }
        public string RecipientName { get; set; }
        public string PhoneNumber { get; set; }
        public string AddressLine { get; set; }
        public string WardId { get; set; }
        public string WardName { get; set; }
        public string ProvinceId { get; set; }
        public string ProvinceName { get; set; }
        public bool IsDefault { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
