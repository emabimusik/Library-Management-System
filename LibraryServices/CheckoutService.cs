using LibraryData;
using LibraryData.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibraryServices
{
    public class CheckoutService : ICheckout
    {
        private LibraryContext _context;
        public CheckoutService ( LibraryContext context){
            _context = context;
         }
        public void Add(Checkout newCheckout)
        {
            _context.Add(newCheckout);
            _context.SaveChanges();
        }

        public void CheckInItem(int id)
        {

            var now = DateTime.Now;

            var item = _context.LibraryAssets
                .First(a => a.Id == id);

            _context.Update(item);

            // remove any existing checkouts on the item
            var checkout = _context.Checkouts
                .Include(c => c.LibraryAsset)
                .Include(c => c.LibraryCard)
                .FirstOrDefault(a => a.LibraryAsset.Id == id);
            if (checkout != null) _context.Remove(checkout);

            // close any existing checkout history
            var history = _context.CheckoutHistories
                .Include(h => h.LibraryAsset)
                .Include(h => h.LibraryCard)
                .FirstOrDefault(h =>
                    h.LibraryAsset.Id == id
                    && h.CheckedIn == null);
            if (history != null)
            {
                _context.Update(history);
                history.CheckedIn = now;
            }

            // look for current holds
            var currentHolds = _context.Holds
                .Include(a => a.LibraryAsset)
                .Include(a => a.LibraryCard)
                .Where(a => a.LibraryAsset.Id == id);

            // if there are current holds, check out the item to the earliest
            if (currentHolds.Any())
            {
                CheckoutEarliersHold(id, currentHolds);
                return;
            }

            // otherwise, set item status to available
            item.Status = _context.Statuses.FirstOrDefault(a => a.Name == "Available");

            _context.SaveChanges();
            /*
            var now = DateTime.Now;
            var item = _context.LibraryAssets.FirstOrDefault(a => a.Id == assetId);
            //_context.Update(item);
            // remove any existing checkout
            RemoveExistingCheckouts(assetId);
            // close existing checkout history
            CloseExistingCheckoutHistory(assetId, now);
            //lock for the existing holds on the item
            var currentHolds = _context.Holds
               .Include(h => h.LibraryAsset)
               .Include(h => h.LibraryCard)
               .Where(h => h.LibraryAsset.Id == assetId);

            // if there are hlods, checkout the item to the librarycard with the earliest hold
            if (currentHolds.Any())
            {
                CheckoutEarliersHold(assetId, currentHolds);
                return;
            }
            // otherwise upadte the item status to available
            UpadateAssetStatus(assetId, "Available");
            _context.SaveChanges();
            */
        }

        private void CheckoutEarliersHold(int id, IQueryable<Hold> currentHolds)
        {
            var earlierestHold = currentHolds.OrderBy(a => a.HoldPlaced)
                .FirstOrDefault();
            var card = earlierestHold.LibraryCard;
            _context.Remove(earlierestHold);
            _context.SaveChanges();
            CheckOutItem(id, card.Id);
        }

        public void CheckOutItem(int id, int libraryCardId)
        {
            if (IsCheckedOut(id))
            {

                return;
                // add logic here  to send the feed back to the user

            }
            var item = _context.LibraryAssets.FirstOrDefault(a => a.Id == id);
            UpadateAssetStatus(id, "Checked Out");
            var libraryCard = _context.LibraryCards.Include(card => card.Checkouts)
                            .FirstOrDefault(card => card.Id == libraryCardId);
            var now = DateTime.Now;
            var checkout = new Checkout
            {
                LibraryAsset = item,
                LibraryCard = libraryCard,
                Since = now,
                Until = GetDefaultCheckOutTime(now)
          };
            _context.Add(checkout);
            var checkoutHistory = new CheckoutHistory
            {
                CheckedOut = now,
                LibraryAsset = item,
                LibraryCard = libraryCard,
               // CheckedIn = GetDefaultCheckOutTime(now)
            };
            _context.Add(checkoutHistory);
            _context.SaveChanges();
        }

        private DateTime GetDefaultCheckOutTime(DateTime now)
        {
            return now.AddDays(30);
        }

        public Checkout GetById(int id)
        {
            return GetAll().FirstOrDefault(checkout => checkout.Id == id);
        }

        public IEnumerable<Checkout> GetAll()
        {
            return _context.Checkouts;
        }

        public int GetAvailableCopies(int id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<CheckoutHistory> GetCheckoutHistory(int id)
        {
            return _context.CheckoutHistories
                .Include(h => h.LibraryAsset)
                .Include(h => h.LibraryCard)
                .Where(h => h.LibraryAsset.Id == id);
        }

        public string GetCurrentHoldPatronName(int holdId)
        {
            var hold = _context.Holds.Include(h => h.LibraryAsset)
                                      .Include(h => h.LibraryCard)
                                      .FirstOrDefault(h => h.Id == holdId);
            var cardId = hold?.LibraryCard.Id;
            var patron = _context.Patrons.Include(p => p.LibraryCard)
                                 .FirstOrDefault(p => p.LibraryCard.Id == cardId);
            return patron?.FirstName + "" + patron?.LastName;
        }

        public string GetCurrentHoldPlaced(int holdId)
        {
            var hold = _context.Holds.Include(a => a.LibraryAsset)
                                     .Include(a => a.LibraryCard)
                                     .Where(v => v.Id == holdId);
            return  hold.Select(a=>a.HoldPlaced).FirstOrDefault().ToString();
        }

        public IEnumerable<Hold> GetCurrentHolds(int id)
        {
            return _context.Holds
                    .Include(h => h.LibraryAsset)
                    .Where(h => h.LibraryAsset.Id == id);
        }

        public string GetCurrentPatron(int id)
        {
            var checkout = _context.Checkouts
                 .Include(a => a.LibraryAsset)
                 .Include(a => a.LibraryCard)
                 .FirstOrDefault(a => a.LibraryAsset.Id == id);

            if (checkout == null) return "Not checked out.";

            var cardId = checkout.LibraryCard.Id;

            var patron = _context.Patrons
                .Include(p => p.LibraryCard)
                .First(c => c.LibraryCard.Id == cardId);

            return patron.FirstName + " " + patron.LastName;
        }

        public Checkout GetLatestCheckout(int id)
        {
            return _context.Checkouts.Where(c => c.LibraryAsset.Id == id)
                 .OrderByDescending(c => c.Since).FirstOrDefault();
        }

        public int GetNumberOfCopies(int id)
        {
            return _context.LibraryAssets
                 .First(a => a.Id == id)
                  .NumberOfCopies;
        }
        /**
        public bool IsCheckedOut(int id)
        {
            return _context.Checkouts
                .Where(co => co.Id == id)
                .Any();
            
        }
      */
        public bool IsCheckedOut(int id)
        {
            var isCheckedOut = _context.Checkouts.Where(a => a.LibraryAsset.Id == id).Any();
            return isCheckedOut;
        }
        public void MarkFound(int id)
        {
            var now = DateTime.Now;
          
            // refactory function
            UpadateAssetStatus(id, "Available");
            _context.SaveChanges();

            // remove any exixting checkout on the item
            RemoveExistingCheckouts(id);
            CloseExistingCheckoutHistory(id, now);
           
            // close any existing checkout history
           
            _context.SaveChanges();
        }
       
        private void UpadateAssetStatus(int id, string newStatus)
        {
            var item = _context.LibraryAssets.FirstOrDefault(a => a.Id == id);
            _context.Update(item);
            item.Status = _context.Statuses.FirstOrDefault(status => status.Name == newStatus);
        }

        private void CloseExistingCheckoutHistory(int id, DateTime now)
        {
            var history = _context.CheckoutHistories.FirstOrDefault(h => h.Id == id
               && h.CheckedIn == null);
            if (history != null)
            {
                _context.Update(history);
                history.CheckedIn = now;
            }
        }

        private void RemoveExistingCheckouts(int id)
        {
            var checkout = _context.Checkouts
                 .FirstOrDefault(co => co.LibraryAsset.Id == id);
            if (checkout != null)
            {
                _context.Checkouts.Remove(checkout);
            }
        }

        public void MarkLost(int id)
        {
            UpadateAssetStatus(id, "Lost");
            _context.SaveChanges();
        }

        public void PlaceHold(int assetId, int libraryCardId)
        {
            var now = DateTime.Now;
            var asset = _context.LibraryAssets.Include(a=>a.Status)
                .FirstOrDefault(a => a.Id == assetId);
            var card = _context.LibraryCards.FirstOrDefault(c => c.Id == libraryCardId);
            if (asset.Status.Name == "Available")
            {
                UpadateAssetStatus(assetId, "On Hold");
            }
            var hold = new Hold
            {
                HoldPlaced = now,
                LibraryAsset = asset,
                LibraryCard = card
            };
            _context.Add(hold);
            _context.SaveChanges();
        }

        public string GetCurrentCheckoutPatron(int assetId)
        {
            var checkout = GetCheckoutByAssetId(assetId);
            if(checkout == null)
            {
                return "";

            }
            var cardId = checkout.LibraryCard.Id;
            var patron = _context.Patrons.Include(p => p.LibraryCard)
                        .FirstOrDefault(p => p.LibraryCard.Id == cardId);
            return patron.FirstName + "" + patron.LastName;
        }

        private Checkout GetCheckoutByAssetId(int assetId)
        {
            var checkout = _context.Checkouts
                   .Include(co => co.LibraryAsset)
                   .Include(co => co.LibraryCard)
                    .FirstOrDefault(co => co.LibraryAsset.Id == assetId);
            return checkout;
        }
    }
}
