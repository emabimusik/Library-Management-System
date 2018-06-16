using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LibraryData;
using LibraryData.Models;
using Library.Models.Branch;

namespace Library.Controllers
{
    public class BranchController : Controller
    {
        private ILibraryBranch _branch;
        public BranchController(ILibraryBranch branch)
        {
            _branch = branch;
        }

        public IActionResult Index()
        {
            var branches = _branch.GetAll().Select(branch => new BranchDetailModel
            {
                Id = branch.Id,
                Name = branch.Name,
                isOpen = _branch.IsBranchOpen(branch.Id),
                NumberOfAssets = _branch.GetAssets(branch.Id).Count(),
                NumberOfPatrons = _branch.GetPatrons(branch.Id).Count()
            });
            var model = new BranchIndexModel()
            {
                Branches = branches
            };
            return View(model);
        }
        public IActionResult Detail(int id)
        {
            var branch = _branch.Get(id);
            var model = new BranchDetailModel
            {

                Id = branch.Id,
                Name = branch.Name,
                isOpen = _branch.IsBranchOpen(branch.Id),
                OpenDate = branch.OpenDate.ToString("yyyy-MM-dd"),
                NumberOfAssets = _branch.GetAssets(branch.Id).Count(),
                NumberOfPatrons = _branch.GetPatrons(branch.Id).Count(),
                TotalAssetValue = _branch.GetAssets(id).Sum(a => a.Cost),
                ImageUrl = branch.ImageUrl,
                Address = branch.Address,
                Description = branch.Description,
                Telephone = branch.Telephone,
                HoursOpen = _branch.GetBrachHours(id)
            };

            return View(model);

        }
    }
}