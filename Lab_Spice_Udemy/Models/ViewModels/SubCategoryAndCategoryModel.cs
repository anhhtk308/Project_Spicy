using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lab_Spice_Udemy.Models.ViewModels
{
    public class SubCategoryAndCategoryModel
    {
        public IEnumerable<Category> CategoryList { get; set; }
        public SubCategory SubCategory { get; set; }

        public List<string> SubCategoryList { get; set; }
        public string StatusMessage { get; set; }
    }
}
