﻿//
// ProjectExtensions.cs
//
// Author:
//       Matt Ward <matt.ward@microsoft.com>
//
// Copyright (c) 2019 Microsoft
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MonoDevelop.PackageManagement.PowerShell.EnvDTE;
using MonoDevelop.PackageManagement.PowerShell.Protocol;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Versioning;

namespace MonoDevelop.PackageManagement.PowerShell.ConsoleHost.Core
{
	public static class ProjectExtensions
	{
		public static async Task<IEnumerable<PackageReference>> GetInstalledPackagesAsync (
			this Project project,
			CancellationToken token)
		{
			var message = new ProjectParams {
				FileName = project.FileName
			};
			var list = await JsonRpcProvider.Rpc.InvokeWithParameterObjectAsync<ProjectPackagesList> (Methods.ProjectInstalledPackagesName, message, token);
			return ToPackageReferences (list.Packages);
		}

		static IEnumerable<PackageReference> ToPackageReferences (IEnumerable<PackageReferenceInfo> packages)
		{
			return packages.Select (package => CreatePackageReference (package));
		}

		static PackageReference CreatePackageReference (PackageReferenceInfo package)
		{
			return new PackageReference (
				new PackageIdentity (package.Id, new NuGetVersion (package.Version)),
				NuGetFramework.Parse (package.TargetFramework)
			);
		}
	}
}