using LibraryData.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryData
{
    public interface ICheckout
    {
        IEnumerable<Checkout> GetAll();
        Checkout GetById(int Id);
        void Add(Checkout newCheckout);
        IEnumerable<CheckoutHistory> GetCheckoutHistory(int id);
        void PlaceHold(int assetId, int libraryCardId);
        void CheckOutItem(int id, int libraryCardId);
        void CheckInItem(int assetId);
        Checkout GetLatestCheckout(int id);
        int GetNumberOfCopies(int id);
        int GetAvailableCopies(int id);
        bool IsCheckedOut(int id);

        string GetCurrentHoldPatronName(int id);
         string GetCurrentHoldPlaced(int id);
        string GetCurrentPatron(int id);
        IEnumerable<Hold> GetCurrentHolds(int id);
        string GetCurrentCheckoutPatron(int assetId);
        void MarkLost(int id);
        void MarkFound(int id);
    }
}
