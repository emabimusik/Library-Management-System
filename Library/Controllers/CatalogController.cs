using Library.Models.Catalog;
using Library.Models.Catalog.CheckoutModels;
using LibraryData;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.Controllers
{
    public class CatalogController:Controller
    {

        private ILibraryAsset _asset;
        private ICheckout _checkouts;
        public CatalogController(ILibraryAsset asset, ICheckout checkouts)
        {
            _asset = asset;
            _checkouts = checkouts;
        }
        public IActionResult Index()
        {
            var assetModels = _asset.GetAll();
            var listingResult = assetModels
                .Select(result => new AssetIndexListingModel
                {
                    Id = result.Id,
                    ImageUrl = result.ImageUrl,
                    AuthorOrDirector = _asset.GetAuthorOrDirector(result.Id),
                    DeweyCallNumber = _asset.GetDeweyIndex(result.Id),
                    Title = result.Title,
                    Type = _asset.GetType(result.Id)

                });
            var model = new AssetIndexModel()
            {
                Assets = listingResult
            };

            return View(model);
        }


        public  IActionResult Portofolio()
        {
            return View();
        }
       
        public IActionResult Details(int id)
        {
            var asset = _asset.GetById(id);
            var currentHolds = _checkouts.GetCurrentHolds(id).Select(a => new AssetHoldModel()
            {
                HoldPlaced = _checkouts.GetCurrentHoldPlaced(a.Id),
                PatronName = _checkouts.GetCurrentHoldPatronName(a.Id)

            });
            
            var assetmodel = new AssetDetailModel
            {

                AssetId = id ,
                Title = asset.Title,
                Type = _asset.GetType(id),
                Year = asset.Year,
                Cost = asset.Cost,
                Status = asset.Status.Name,
                ImageUrl = asset.ImageUrl,
                AuthorOrDirector = _asset.GetAuthorOrDirector(id),
                CurrentLocation = _asset.GetCurrentLocation(id).Name,
                DeweyCallNumber = _asset.GetDeweyIndex(id),
                ISBN = _asset.GetIsbn(id),
                CheckoutHistory = _checkouts.GetCheckoutHistory(id),
                LatestCheckout = _checkouts.GetLatestCheckout(id),
                PatronName = _checkouts.GetCurrentCheckoutPatron(id),
                CurrentHolds = currentHolds
               
            };
            return View(assetmodel);
        }




        public  IActionResult Checkout(int id)
        {
            var asset = _asset.GetById(id);
            var model = new CheckoutModel
            {
                AssetId =id,
                ImageUrl = asset.ImageUrl,
                Title = asset.Title,
                LibraryCardId ="",
                IsCheckedOut = _checkouts.IsCheckedOut(id)
            };
            return View(model);
        }
        [HttpPost]
        public IActionResult PlaceCheckout(int assetId, int libraryCardId)
        {
            _checkouts.CheckOutItem(assetId, libraryCardId);
            return RedirectToAction("Details", new { id = assetId });
        }
        public IActionResult MarkLost(int assetId)
        {
            _checkouts.MarkLost(assetId);
            return RedirectToAction("Details", new { id = assetId });
            
        }
        public IActionResult Hold(int id)
        {
            var asset = _asset.GetById(id);
            var model = new CheckoutModel
            {
                AssetId = id,
                ImageUrl = asset.ImageUrl,
                Title = asset.Title,
                LibraryCardId = "",
                IsCheckedOut = _checkouts.IsCheckedOut(id),
                HoldCount = _checkouts.GetCurrentHolds(id).Count()
            };
            return View(model);
        }
        public IActionResult MarkFound(int assetId)
        {
            _checkouts.MarkLost(assetId);
            return RedirectToAction("Details", new { id = assetId });
        }

       
        [HttpPost]
        public IActionResult PlaceHold(int assetId, int libraryCardId)
        {
            _checkouts.PlaceHold(assetId, libraryCardId);
            return RedirectToAction("Details", new { id = assetId });
        }

        public IActionResult Checkin( int id)
        {
            _checkouts.CheckInItem(id);
            return RedirectToAction("Details", new { id = id });
        }
    }
}
