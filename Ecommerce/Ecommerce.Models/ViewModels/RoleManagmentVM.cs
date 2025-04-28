using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Models.ViewModels
{
    [ValidateNever]
    public class RoleManagmentVM
    {
        public ApplicationUser ApplicationUser { get; set; } = new ApplicationUser();
        public IEnumerable<SelectListItem> RoleList { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> CompanyList { get; set; } = new List<SelectListItem>();
    }
}