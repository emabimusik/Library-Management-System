﻿using LibraryData.Models;
using System.Collections.Generic;
using System.Text;

namespace LibraryData
{
    public interface ILibraryBranch
    {
        IEnumerable<LibraryBranch> GetAll();
        IEnumerable<Patron> GetPatrons( int branchId);
        IEnumerable<LibraryAsset> GetAssets(int branchId);
        LibraryBranch Get(int branchId);
        IEnumerable<string> GetBrachHours(int branchId);
        void Add(LibraryBranch newBranch);

        bool IsBranchOpen(int branchId);


    }
}
