using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Models.ViewModels
{
    public class OrderVM
    {
        public OrderHeader OrderHeader { get; set; } = new OrderHeader();
        public IEnumerable<OrderDetail> OrderDetail { get; set; } = new List<OrderDetail>();
    }
}