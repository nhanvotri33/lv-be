// ==========================================================================
// MODULE: ReturnRequestDto.cs
// MỤC ĐÍCH: File mã nguồn C# xử lý module ReturnRequestDto
// ==========================================================================
using System;
using System.Collections.Generic;

namespace ECommerce1.DTOs
{
    public class CreateReturnItemDto
    {
        public int OrderItemId { get; set; }
        public int Quantity { get; set; }
        public string Reason { get; set; }
        public List<string>? ProofImages { get; set; }
    }

    public class CreateReturnRequestDto
    {
        public int OrderId { get; set; }
        public string? GeneralNote { get; set; }
        public List<CreateReturnItemDto> Items { get; set; } = new List<CreateReturnItemDto>();
    }

    public class ApproveReturnRequestDto
    {
        public string? AdminNote { get; set; }
    }

    public class RejectReturnRequestDto
    {
        public string? AdminNote { get; set; }
    }
}
