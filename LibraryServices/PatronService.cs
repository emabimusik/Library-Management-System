﻿using LibraryData;
using LibraryData.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibraryServices
{
    public class PatronService : IPatron
    {
        private LibraryContext _context;
         public PatronService(LibraryContext context)
        {
            _context = context;
        }
        public void Add(Patron newBook)
        {
            _context.Patrons.Add(newBook);
            _context.SaveChanges();
        }

        public Patron Get(int id)
        {
           return  _context.Patrons.Include(Patron => Patron.LibraryCard)
                     .Include(patron => patron.HomeLibraryBranch)
                     .FirstOrDefault(patron => patron.Id == id);
                
        }

        public IEnumerable<Patron> GetAll()
        {
            return _context.Patrons.Include(Patron => Patron.LibraryCard)
                     .Include(patron => patron.HomeLibraryBranch);
        }

        public IEnumerable<CheckoutHistory> GetCheckoutHistory(int patronId)
        {
            var cardId = _context.Patrons.Include(patron => patron.LibraryCard)
                        .FirstOrDefault(patron => patron.Id == patronId).LibraryCard.Id;
            return _context.CheckoutHistories.Include(co => co.LibraryCard)
                                             .Include(co => co.LibraryAsset)
                                             .Where(co => co.LibraryCard.Id == cardId)
                                             .OrderByDescending(co => co.CheckedOut);


        }

        public IEnumerable<Checkout> GetCheckouts(int patronId)
        {
            var cardId = _context.Patrons.Include(patron => patron.LibraryCard)
                         .FirstOrDefault(patron => patron.Id == patronId).LibraryCard.Id;
            return _context.Checkouts
                     .Include(patron => patron.LibraryCard)
                     .Include(patron => patron.LibraryAsset)
                     .Where(co => co.LibraryCard.Id == cardId);
        }

        public IEnumerable<Hold> GetHolds(int patronId)
        {
            var cardId = _context.Patrons.Include(patron => patron.LibraryCard)
                        .FirstOrDefault(patron => patron.Id == patronId).LibraryCard.Id;
            return _context.Holds.Include(h => h.LibraryCard)
                                   .Include(h => h.LibraryAsset)
                                   .Where(h => h.LibraryCard.Id == cardId)
                                   .OrderByDescending(h => h.HoldPlaced);
            
                                         
        }
    }
}
