﻿// 
// ManagePackagesViewModel.cs
// 
// Author:
//   Matt Ward <ward.matt@gmail.com>
// 
// Copyright (C) 2013 Matthew Ward
// 
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.ObjectModel;
using System.Linq;

using NuGet;
using MonoDevelop.PackageManagement;

namespace ICSharpCode.PackageManagement
{
	public class ManagePackagesViewModel2 : ViewModelBase<ManagePackagesViewModel2>, IDisposable
	{
		IThreadSafePackageManagementEvents packageManagementEvents;
		PackagesViewModels2 packagesViewModels;
		ManagePackagesViewTitle viewTitle;
		string message;
		bool hasError;

		public ManagePackagesViewModel2 (
			PackagesViewModels2 packagesViewModels,
			ManagePackagesViewTitle viewTitle,
			IThreadSafePackageManagementEvents packageManagementEvents)
		{
			this.packagesViewModels = packagesViewModels;
			this.viewTitle = viewTitle;
			this.packageManagementEvents = packageManagementEvents;

			packageManagementEvents.PackageOperationError += PackageOperationError;
			packageManagementEvents.PackageOperationsStarting += PackageOperationsStarting;

			packagesViewModels.ReadPackages ();
		}

		public AvailablePackagesViewModel AvailablePackagesViewModel {
			get { return packagesViewModels.AvailablePackagesViewModel; }
		}

		public InstalledPackagesViewModel2 InstalledPackagesViewModel {
			get { return packagesViewModels.InstalledPackagesViewModel; }
		}

		public RecentPackagesViewModel2 RecentPackagesViewModel {
			get { return packagesViewModels.RecentPackagesViewModel; }
		}

		public UpdatedPackagesViewModel2 UpdatedPackagesViewModel {
			get { return packagesViewModels.UpdatedPackagesViewModel; }
		}

		public string Title {
			get { return viewTitle.Title; }
		}

		public void Dispose ()
		{
			packagesViewModels.Dispose ();

			packageManagementEvents.PackageOperationError -= PackageOperationError;
			packageManagementEvents.PackageOperationsStarting -= PackageOperationsStarting;
			packageManagementEvents.Dispose ();
		}

		void PackageOperationError (object sender, PackageOperationExceptionEventArgs e)
		{
			ShowErrorMessage (e.Exception.Message);
		}

		void ShowErrorMessage (string message)
		{
			this.Message = message;
			this.HasError = true;
		}

		public string Message {
			get { return message; }
			set {
				message = value;
				OnPropertyChanged (model => model.Message);
			}
		}

		public bool HasError {
			get { return hasError; }
			set {
				hasError = value;
				OnPropertyChanged (model => model.HasError);
			}
		}

		void PackageOperationsStarting (object sender, EventArgs e)
		{
			ClearMessage ();
		}

		void ClearMessage ()
		{
			this.Message = null;
			this.HasError = false;
		}
	}
}
