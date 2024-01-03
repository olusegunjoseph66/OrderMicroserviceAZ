using System;
using System.Collections.Generic;

namespace Order.Application.DTOs.Response
{
    public partial class ShoppingCartDto
    {
        
        public int Id { get; set; }
        public DateTime DateCreated { get; set; }
        public int CreatedByUserId { get; set; }
        public int UserId { get; set; }
        public byte ShoppingCartStatusId { get; set; }
        public DateTime? DateModified { get; set; }
        public int? ModifiedByUserId { get; set; }
    }
}
