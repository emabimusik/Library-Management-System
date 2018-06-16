using LibraryData;
using LibraryData.Models;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System;

namespace LibraryServices
{
    public class LibraryBranchService : ILibraryBranch
    {


        private LibraryContext _context;
        public LibraryBranchService(LibraryContext context)
        {
            _context = context;
        }
        public void Add(LibraryBranch newBranch)
        {
            _context.Add(newBranch);
            _context.SaveChanges();
        }

        public LibraryBranch Get(int branchId)
        {
            var branch = GetAll().FirstOrDefault(b => b.Id == branchId);
            return branch;
        }

        public IEnumerable<LibraryBranch> GetAll()
        {
            return _context.LibraryBranches.Include(b => b.Patrons)
                                           .Include(b => b.LibraryAssets);
        }

        public IEnumerable<LibraryAsset> GetAssets(int branchId)
        {
            var  assets = _context.LibraryBranches.Include(b => b.LibraryAssets).FirstOrDefault(b => b.Id == branchId).LibraryAssets;

            return assets;
        }

        public IEnumerable<Patron> GetPatrons(int branchId)
        {
            var patron =_context.LibraryBranches.Include(b => b.Patrons).FirstOrDefault(b => b.Id == branchId).Patrons;
            return patron;
        }
        
        public bool IsBranchOpen(int branchId)
        {
            var currentTimeHour = DateTime.Now.Hour;
            var currentDayofWeek = (int) DateTime.Now.DayOfWeek + 1;

            var hours = _context.BranchHours.Where(h => h.Branch.Id == branchId);
            var daysHours = hours.FirstOrDefault(h => h.DayOfWeek == currentDayofWeek);
            return currentTimeHour < daysHours.CloseTime && currentTimeHour > daysHours.OpenTime;
        }

        public IEnumerable<string> GetBrachHours(int branchId)
        {
            var hours = _context.BranchHours.Where(h => h.Branch.Id == branchId);
            return DataHelpers.HumanizeBizHours(hours);
        }
    }
}
