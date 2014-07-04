﻿// 
// PackageManagementSolution.cs
// 
// Author:
//   Matt Ward <ward.matt@gmail.com>
// 
// Copyright (C) 2012 Matthew Ward
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
using System.Collections.Generic;
using System.Linq;
using MonoDevelop.Projects;
using NuGet;
using MonoDevelop.PackageManagement;

namespace ICSharpCode.PackageManagement
{
	public class PackageManagementSolution2 : IPackageManagementSolution2
	{
		IRegisteredPackageRepositories registeredPackageRepositories;
		IExtendedPackageManagementProjectService projectService;
		IPackageManagementProjectFactory2 projectFactory;
		ISolutionPackageRepositoryFactory solutionPackageRepositoryFactory;

		public PackageManagementSolution2 (
			IRegisteredPackageRepositories registeredPackageRepositories,
			IPackageManagementEvents packageManagementEvents)
			: this (
				registeredPackageRepositories,
				PackageManagementExtendedServices.ProjectService,
				new PackageManagementProjectFactory2 (packageManagementEvents),
				new SolutionPackageRepositoryFactory ())
		{
		}

		public PackageManagementSolution2 (
			IRegisteredPackageRepositories registeredPackageRepositories,
			IExtendedPackageManagementProjectService projectService,
			IPackageManagementProjectFactory2 projectFactory,
			ISolutionPackageRepositoryFactory solutionPackageRepositoryFactory)
		{
			this.registeredPackageRepositories = registeredPackageRepositories;
			this.projectFactory = projectFactory;
			this.projectService = projectService;
			this.solutionPackageRepositoryFactory = solutionPackageRepositoryFactory;
		}

		public string FileName {
			get { return OpenSolution.FileName; }
		}

		Solution OpenSolution {
			get { return projectService.OpenSolution; }
		}

		public IPackageManagementProject2 GetActiveProject ()
		{
			if (HasActiveProject ()) {
				return GetActiveProject (registeredPackageRepositories.CreateAggregateRepository ());
			}
			return null;
		}

		bool HasActiveProject ()
		{
			return GetActiveDotNetProject () != null;
		}

		public Project GetActiveDotNetProject ()
		{
			return projectService.CurrentProject;
		}

		IPackageRepository ActivePackageRepository {
			get { return registeredPackageRepositories.ActiveRepository; }
		}

		public IPackageManagementProject2 GetActiveProject (IPackageRepository sourceRepository)
		{
			var activeProject = GetActiveDotNetProject () as DotNetProject;
			if (activeProject != null) {
				return CreateProject (sourceRepository, activeProject);
			}
			return null;
		}

		IPackageManagementProject2 CreateProject (IPackageRepository sourceRepository, DotNetProject project)
		{
			return projectFactory.CreateProject (sourceRepository, project);
		}

		IPackageRepository CreatePackageRepository (PackageSource source)
		{
			return registeredPackageRepositories.CreateRepository (source);
		}

		public IPackageManagementProject2 GetProject (PackageSource source, string projectName)
		{
			DotNetProject project = GetDotNetProject (projectName);
			return CreateProject (source, project);
		}

		DotNetProject GetDotNetProject (string name)
		{
			var openProjects = new OpenDotNetProjects2 (projectService);
			return openProjects.FindProject (name);
		}

		IPackageManagementProject2 CreateProject (PackageSource source, DotNetProject project)
		{
			IPackageRepository sourceRepository = CreatePackageRepository (source);
			return CreateProject (sourceRepository, project);
		}

		public IPackageManagementProject2 GetProject (IPackageRepository sourceRepository, string projectName)
		{
			DotNetProject project = GetDotNetProject (projectName);
			return CreateProject (sourceRepository, project);
		}

		public IPackageManagementProject2 GetProject (IPackageRepository sourceRepository, Project project)
		{
			var dotNetProject = project as DotNetProject;
			return CreateProject (sourceRepository, dotNetProject);
		}

		public IEnumerable<Project> GetDotNetProjects ()
		{
			return projectService.GetOpenProjects ();
		}

		public bool IsOpen {
			get { return OpenSolution != null; }
		}

		public bool HasMultipleProjects ()
		{
			return projectService.GetOpenProjects ().Count () > 1;
		}

		public bool IsPackageInstalled (IPackage package)
		{
			ISolutionPackageRepository2 repository = CreateSolutionPackageRepository ();
			return repository.IsInstalled (package);
		}

		ISolutionPackageRepository2 CreateSolutionPackageRepository ()
		{
			return new SolutionPackageRepository2 (OpenSolution);
		}

		public IQueryable<IPackage> GetPackages ()
		{
			ISolutionPackageRepository2 repository = CreateSolutionPackageRepository ();
			List<IPackageManagementProject2> projects = GetProjects (ActivePackageRepository).ToList ();
			return repository
				.GetPackages ()
				.Where (package => IsPackageInstalledInSolutionOrAnyProject (projects, package));
		}

		bool IsPackageInstalledInSolutionOrAnyProject (IList<IPackageManagementProject2> projects, IPackage package)
		{
			if (projects.Any (project => project.IsPackageInstalled (package))) {
				return true;
			}
			return false;
		}

		public string GetInstallPath (IPackage package)
		{
			ISolutionPackageRepository2 repository = CreateSolutionPackageRepository ();
			return repository.GetInstallPath (package);
		}

		public IEnumerable<IPackage> GetPackagesInReverseDependencyOrder ()
		{
			ISolutionPackageRepository2 repository = CreateSolutionPackageRepository ();
			return repository.GetPackagesByReverseDependencyOrder ();
		}

		public IEnumerable<IPackageManagementProject2> GetProjects (IPackageRepository sourceRepository)
		{
			foreach (DotNetProject dotNetProject in GetDotNetProjects()) {
				yield return projectFactory.CreateProject (sourceRepository, dotNetProject);
			}
		}
	}
}