﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Linq;

using ICSharpCode.PackageManagement;
using ICSharpCode.PackageManagement.Design;
using ICSharpCode.SharpDevelop.Project;
using NuGet;
using NUnit.Framework;
using PackageManagement.Tests.Helpers;

namespace PackageManagement.Tests
{
	[TestFixture]
	public class PackageManagementSelectedProjectsTests
	{
		PackageManagementSelectedProjects selectedProjects;
		FakePackageManagementSolution fakeSolution;
		
		void CreateSelectedProjects()
		{
			selectedProjects = new PackageManagementSelectedProjects(fakeSolution);
		}
	
		void CreateFakeSolution()
		{
			fakeSolution = new FakePackageManagementSolution();
		}
		
		List<IProject> AddSolutionWithOneProjectToProjectService()
		{
			TestableProject project = ProjectHelper.CreateTestProject("Test1");
			fakeSolution.FakeMSBuildProjects.Add(project);
			
			return fakeSolution.FakeMSBuildProjects;
		}
		
		List<IProject> AddSolutionWithTwoProjectsToProjectService()
		{
			TestableProject project1 = ProjectHelper.CreateTestProject("Test1");
			TestableProject project2 = ProjectHelper.CreateTestProject("Test2");
			
			Solution solution = project1.ParentSolution;
			project2.Parent = solution;
			
			fakeSolution.FakeMSBuildProjects.Add(project1);
			fakeSolution.FakeMSBuildProjects.Add(project2);
			
			return fakeSolution.FakeMSBuildProjects;
		}
		
		void NoProjectsSelected()
		{
			fakeSolution.NoProjectsSelected();
		}
		
		[Test]
		public void GetProjects_SolutionHasTwoProjectsAndOneProjectSelectedInProjectsBrowser_ReturnsProjectSelectedInProjects()
		{
			CreateFakeSolution();
			List<IProject> projectsAddedToSolution = AddSolutionWithTwoProjectsToProjectService();
			IProject project = projectsAddedToSolution[1];
			project.Name = "MyProject";
			fakeSolution.FakeActiveMSBuildProject = project;
			
			var fakeProject = fakeSolution.AddFakeProjectToReturnFromGetProject("MyProject");
			CreateSelectedProjects();
			
			var fakePackage = new FakePackage();
			var projects = new List<IPackageManagementSelectedProject>();
			projects.AddRange(selectedProjects.GetProjects(fakePackage));
			
			var expectedProject = new FakeSelectedProject("MyProject");
			var expectedProjects = new List<IPackageManagementSelectedProject>();
			expectedProjects.Add(expectedProject);
			
			SelectedProjectCollectionAssert.AreEqual(expectedProjects, projects);
		}
		
		[Test]
		public void GetProjects_SolutionHasTwoProjectsAndOneProjectSelectedInitiallyAndGetProjectsCalledAgainAfterNoProjectsAreSelected_ReturnsProjectSelectedInProjects()
		{
			CreateFakeSolution();
			List<IProject> projectsAddedToSolution = AddSolutionWithTwoProjectsToProjectService();
			IProject project = projectsAddedToSolution[1];
			project.Name = "MyProject";
			fakeSolution.FakeActiveMSBuildProject = project;
			
			var fakeProject = fakeSolution.AddFakeProjectToReturnFromGetProject("MyProject");
			CreateSelectedProjects();
			
			var fakePackage = new FakePackage();
			var projects = new List<IPackageManagementSelectedProject>();
			projects.AddRange(selectedProjects.GetProjects(fakePackage));
			
			projects.Clear();
			
			NoProjectsSelected();
			projects.AddRange(selectedProjects.GetProjects(fakePackage));
			
			var expectedProject = new FakeSelectedProject("MyProject");
			var expectedProjects = new List<IPackageManagementSelectedProject>();
			expectedProjects.Add(expectedProject);
			
			SelectedProjectCollectionAssert.AreEqual(expectedProjects, projects);
		}
		
		[Test]
		public void HasMultipleProjects_SolutionHasTwoProjectsAndOneProjectSelectedInProjectsBrowser_ReturnsFalse()
		{
			CreateFakeSolution();
			List<IProject> projectsAddedToSolution = AddSolutionWithTwoProjectsToProjectService();
			IProject expectedProject = projectsAddedToSolution[1];
			fakeSolution.FakeActiveMSBuildProject = expectedProject;
			CreateSelectedProjects();
			
			bool hasMultipleProjects = selectedProjects.HasMultipleProjects();
			
			Assert.IsFalse(hasMultipleProjects);
		}
		
		[Test]
		public void GetProjects_SolutionHasTwoProjectsAndNoProjectSelectedInProjectsBrowser_ReturnsAllProjectsInSolutionForPackage()
		{
			CreateFakeSolution();
			List<IProject> projectsAddedToSolution = AddSolutionWithTwoProjectsToProjectService();
			projectsAddedToSolution[0].Name = "Project A";
			projectsAddedToSolution[1].Name = "Project B";
			fakeSolution.FakeActiveProject = null;
			
			fakeSolution.AddFakeProjectToReturnFromGetProject("Project A");
			fakeSolution.AddFakeProjectToReturnFromGetProject("Project B");
			CreateSelectedProjects();
			
			var fakePackage = new FakePackage();
			var projects = new List<IPackageManagementSelectedProject>();
			projects.AddRange(selectedProjects.GetProjects(fakePackage));
						
			var expectedProjects = new List<IPackageManagementSelectedProject>();
			expectedProjects.Add(new FakeSelectedProject("Project A"));
			expectedProjects.Add(new FakeSelectedProject("Project B"));
			
			SelectedProjectCollectionAssert.AreEqual(expectedProjects, projects);
		}
		
		[Test]
		public void HasMultipleProjects_SolutionHasTwoProjectsAndNoProjectSelectedInProjectsBrowser_ReturnsTrue()
		{
			CreateFakeSolution();
			List<IProject> projectsAddedToSolution = AddSolutionWithTwoProjectsToProjectService();
			fakeSolution.FakeActiveProject = null;
			CreateSelectedProjects();
			
			bool hasMultipleProjects = selectedProjects.HasMultipleProjects();
			
			Assert.IsTrue(hasMultipleProjects);
		}
		
		[Test]
		public void HasMultipleProjects_SolutionHasOneProjectAndNoProjectSelectedInProjectsBrowser_ReturnsFalse()
		{
			CreateFakeSolution();
			List<IProject> projectsAddedToSolution = AddSolutionWithOneProjectToProjectService();
			fakeSolution.FakeActiveProject = null;
			CreateSelectedProjects();
			
			bool hasMultipleProjects = selectedProjects.HasMultipleProjects();
			
			Assert.IsFalse(hasMultipleProjects);
		}
		
		[Test]
		public void SelectionName_SolutionHasOneProject_ReturnsProjectNameWithoutFileExtension()
		{
			CreateFakeSolution();
			List<IProject> projectsAddedToSolution = AddSolutionWithOneProjectToProjectService();
			projectsAddedToSolution[0].Name = "MyProject";
			fakeSolution.FakeActiveMSBuildProject = projectsAddedToSolution[0];
			CreateSelectedProjects();
			
			string name = selectedProjects.SelectionName;
			
			Assert.AreEqual("MyProject", name);
		}
		
		[Test]
		public void SelectionName_SolutionHasTwoProjectsAndNoProjectSelected_ReturnsSolutionFileNameWithoutFullPath()
		{
			CreateFakeSolution();
			List<IProject> projectsAddedToSolution = AddSolutionWithTwoProjectsToProjectService();
			NoProjectsSelected();
			fakeSolution.FileName = @"d:\projects\MyProject\MySolution.sln";
			CreateSelectedProjects();
			
			string name = selectedProjects.SelectionName;
			
			Assert.AreEqual("MySolution.sln", name);
		}
		
		[Test]
		public void IsPackageInstalled_PackageInstalledInSolutionWithTwoProjectsAndNoProjectSelected_ReturnsTrue()
		{
			CreateFakeSolution();
			List<IProject> projectsAddedToSolution = AddSolutionWithTwoProjectsToProjectService();
			NoProjectsSelected();
			
			var package = new FakePackage("Test");
			fakeSolution.FakeInstalledPackages.Add(package);
			CreateSelectedProjects();
			
			bool installed = selectedProjects.IsPackageInstalled(package);
			
			Assert.IsTrue(installed);
		}
		
		[Test]
		public void IsPackageInstalled_PackageIsInstalledInSolutionWithTwoProjectsAndNoProjectSelected_ReturnsFalse()
		{
			CreateFakeSolution();
			AddSolutionWithTwoProjectsToProjectService();
			NoProjectsSelected();
			CreateSelectedProjects();
			
			var package = new FakePackage("Test");
			bool installed = selectedProjects.IsPackageInstalled(package);
			
			Assert.IsFalse(installed);
		}
		
		[Test]
		public void IsPackageInstalled_PackageIsInstalledInProjectAndProjectSelected_ReturnsTrue()
		{
			CreateFakeSolution();
			List<IProject> projectsAddedToSolution = AddSolutionWithTwoProjectsToProjectService();
			fakeSolution.FakeActiveMSBuildProject = projectsAddedToSolution[0];
			
			var package = new FakePackage("Test");
			fakeSolution.FakeProjectToReturnFromGetProject.FakePackages.Add(package);
			CreateSelectedProjects();
			
			bool installed = selectedProjects.IsPackageInstalled(package);
			
			Assert.IsTrue(installed);
		}	
		
		[Test]
		public void IsPackageInstalled_PackageIsNotInstalledInProjectAndProjectSelected_ReturnsFalse()
		{
			CreateFakeSolution();
			List<IProject> projectsAddedToSolution = AddSolutionWithTwoProjectsToProjectService();
			fakeSolution.FakeActiveMSBuildProject = projectsAddedToSolution[0];
			CreateSelectedProjects();
			
			var package = new FakePackage("Test");
			bool installed = selectedProjects.IsPackageInstalled(package);
			
			Assert.IsFalse(installed);
		}
		
		[Test]
		public void IsPackageInstalled_PackagePackageIsNotInstalledInProjectAndProjectSelected_ProjectCreatedUsingPackageRepository()
		{
			CreateFakeSolution();
			List<IProject> projectsAddedToSolution = AddSolutionWithTwoProjectsToProjectService();
			fakeSolution.FakeActiveMSBuildProject = projectsAddedToSolution[0];
			CreateSelectedProjects();
			
			var package = new FakePackage("Test");
			bool installed = selectedProjects.IsPackageInstalled(package);
			
			IPackageRepository repository = fakeSolution.RepositoryPassedToGetProject;
			IPackageRepository expectedRepository = package.FakePackageRepository;
			
			Assert.AreEqual(expectedRepository, repository);
		}
		
		[Test]
		public void IsPackageInstalledInSolution_PackageInstalledInSolutionWithTwoProjectsAndOneProjectSelected_ReturnsTrue()
		{
			CreateFakeSolution();
			List<IProject> projectsAddedToSolution = AddSolutionWithTwoProjectsToProjectService();
			fakeSolution.FakeActiveMSBuildProject = projectsAddedToSolution[0];
			
			var package = new FakePackage("Test");
			fakeSolution.FakeInstalledPackages.Add(package);
			CreateSelectedProjects();
			
			bool installed = selectedProjects.IsPackageInstalledInSolution(package);
			
			Assert.IsTrue(installed);
		}
		
		[Test]
		public void IsPackageInstalledInSolution_PackageNotInstalledInSolutionWithTwoProjectsAndOneProjectSelected_ReturnsFalse()
		{
			CreateFakeSolution();
			List<IProject> projectsAddedToSolution = AddSolutionWithTwoProjectsToProjectService();
			fakeSolution.FakeActiveMSBuildProject = projectsAddedToSolution[0];
			CreateSelectedProjects();
			
			var package = new FakePackage("Test");
			
			bool installed = selectedProjects.IsPackageInstalledInSolution(package);
			
			Assert.IsFalse(installed);
		}
		
		[Test]
		public void GetPackagesInstalledInSolution_PackageInstalledInSolutionAndProjectNotSelected_ReturnsPackageInstalledInSolution()
		{
			CreateFakeSolution();
			List<IProject> projectsAddedToSolution = AddSolutionWithTwoProjectsToProjectService();
			NoProjectsSelected();
			
			var package = new FakePackage("Test");
			fakeSolution.FakeInstalledPackages.Add(package);
			CreateSelectedProjects();
			
			IQueryable<IPackage> packages = selectedProjects.GetPackagesInstalledInSolution();
			
			var expectedPackages = new FakePackage[] {
				package
			};
			
			PackageCollectionAssert.AreEqual(expectedPackages, packages);
		}
		
		[Test]
		public void GetSingleProjectSelected_ProjectSelected_ReturnsProject()
		{
			CreateFakeSolution();
			List<IProject> projectsAddedToSolution = AddSolutionWithTwoProjectsToProjectService();
			fakeSolution.FakeActiveMSBuildProject = projectsAddedToSolution[0];
			CreateSelectedProjects();
			
			var repository = new FakePackageRepository();
			IPackageManagementProject project = selectedProjects.GetSingleProjectSelected(repository);
			
			FakePackageManagementProject expectedProject = fakeSolution.FakeProjectToReturnFromGetProject;
			
			Assert.AreEqual(expectedProject, project);
		}
		
		[Test]
		public void GetSingleProjectSelected_ProjectSelectedAndRepositoryPassed_ReturnsProjectCreatedWithRepository()
		{
			CreateFakeSolution();
			List<IProject> projectsAddedToSolution = AddSolutionWithTwoProjectsToProjectService();
			fakeSolution.FakeActiveMSBuildProject = projectsAddedToSolution[0];
			CreateSelectedProjects();
			
			var repository = new FakePackageRepository();
			IPackageManagementProject project = selectedProjects.GetSingleProjectSelected(repository);
			
			Assert.AreEqual(repository, fakeSolution.RepositoryPassedToGetProject);
		}
		
		[Test]
		public void GetSingleProjectSelected_NoProjectSelectedAndRepositoryPassed_ReturnsProjectCreatedWithRepository()
		{
			CreateFakeSolution();
			List<IProject> projectsAddedToSolution = AddSolutionWithTwoProjectsToProjectService();
			fakeSolution.FakeActiveMSBuildProject = projectsAddedToSolution[0];
			CreateSelectedProjects();
			
			var repository = new FakePackageRepository();
			IPackageManagementProject project = selectedProjects.GetSingleProjectSelected(repository);
			
			Assert.AreEqual(repository, fakeSolution.RepositoryPassedToGetProject);
		}
		
		[Test]
		public void HasSingleProjectSelected_SolutionHasTwoProjectsAndOneProjectSelectedInProjectsBrowser_ReturnsTrue()
		{
			CreateFakeSolution();
			List<IProject> projectsAddedToSolution = AddSolutionWithTwoProjectsToProjectService();
			IProject expectedProject = projectsAddedToSolution[1];
			fakeSolution.FakeActiveMSBuildProject = expectedProject;
			CreateSelectedProjects();
			
			bool singleProjectSelected = selectedProjects.HasSingleProjectSelected();
			
			Assert.IsTrue(singleProjectSelected);
		}
		
		[Test]
		public void HasSingleProjectSelected_SolutionHasTwoProjectsAndNoProjectsSelectedInProjectsBrowser_ReturnsFalse()
		{
			CreateFakeSolution();
			List<IProject> projectsAddedToSolution = AddSolutionWithTwoProjectsToProjectService();
			NoProjectsSelected();
			CreateSelectedProjects();
			
			bool singleProjectSelected = selectedProjects.HasSingleProjectSelected();
			
			Assert.IsFalse(singleProjectSelected);
		}
		
		[Test]
		public void HasSingleProjectSelected_NoProjectsInitiallySelectedAndProjectSelectedAfterInitialCall_IsUnchangedAndReturnsFalse()
		{
			CreateFakeSolution();
			List<IProject> projectsAddedToSolution = AddSolutionWithTwoProjectsToProjectService();
			NoProjectsSelected();
			CreateSelectedProjects();
			
			bool singleProjectSelected = selectedProjects.HasSingleProjectSelected();
			fakeSolution.FakeActiveMSBuildProject = fakeSolution.FakeMSBuildProjects[0];
			singleProjectSelected = selectedProjects.HasSingleProjectSelected();
			
			Assert.IsFalse(singleProjectSelected);
		}
		
		[Test]
		public void GetInstalledPackages_PackageInstalledInSolutionAndProjectNotSelected_ReturnsPackageInstalledInSolution()
		{
			CreateFakeSolution();
			List<IProject> projectsAddedToSolution = AddSolutionWithTwoProjectsToProjectService();
			NoProjectsSelected();
			
			var package = new FakePackage("Test");
			fakeSolution.FakeInstalledPackages.Add(package);
			CreateSelectedProjects();
			
			var repository = new FakePackageRepository();
			IQueryable<IPackage> packages = selectedProjects.GetInstalledPackages(repository);
			
			var expectedPackages = new FakePackage[] {
				package
			};
			
			PackageCollectionAssert.AreEqual(expectedPackages, packages);
		}
		
		[Test]
		public void GetInstalledPackages_PackageInstalledInProjectAndProjectIsSelected_ReturnsPackageInstalledInProject()
		{
			CreateFakeSolution();
			List<IProject> projectsAddedToSolution = AddSolutionWithTwoProjectsToProjectService();
			fakeSolution.FakeActiveMSBuildProject = projectsAddedToSolution[0];
			fakeSolution.FakeActiveMSBuildProject.Name = "MyProject";
			
			var package = new FakePackage("Test");
			var project = new FakePackageManagementProject("MyProject");
			project.FakePackages.Add(package);
			fakeSolution.FakeProjectsToReturnFromGetProject.Add("MyProject", project);
			CreateSelectedProjects();
			
			var repository = new FakePackageRepository();
			IQueryable<IPackage> packages = selectedProjects.GetInstalledPackages(repository);
			
			var expectedPackages = new FakePackage[] {
				package
			};
			
			PackageCollectionAssert.AreEqual(expectedPackages, packages);
		}
		
		[Test]
		public void GetInstalledPackages_PackageInstalledInProjectAndProjectIsSelected_CreatesProjectUsingRepository()
		{
			CreateFakeSolution();
			List<IProject> projectsAddedToSolution = AddSolutionWithTwoProjectsToProjectService();
			fakeSolution.FakeActiveMSBuildProject = projectsAddedToSolution[0];
			CreateSelectedProjects();
			
			var expectedRepository = new FakePackageRepository();
			IQueryable<IPackage> packages = selectedProjects.GetInstalledPackages(expectedRepository);
			
			IPackageRepository repository = fakeSolution.RepositoryPassedToGetProject;
			
			Assert.AreEqual(expectedRepository, repository);
		}
		
		[Test]
		public void GetSingleProjectSelected_NoProjectSelected_ReturnsNull()
		{
			CreateFakeSolution();
			AddSolutionWithTwoProjectsToProjectService();
			NoProjectsSelected();
			CreateSelectedProjects();
			
			var repository = new FakePackageRepository();
			IPackageManagementProject project = selectedProjects.GetSingleProjectSelected(repository);
			
			Assert.IsNull(project);
		}
		
		[Test]
		public void GetProjects_SolutionHasTwoProjectsAndOneProjectSelectedInitiallyAndActiveProjectChangedInSolutionAfterInstanceCreated_ReturnsProjectSelectedInProjects()
		{
			CreateFakeSolution();
			List<IProject> projectsAddedToSolution = AddSolutionWithTwoProjectsToProjectService();
			IProject project = projectsAddedToSolution[1];
			project.Name = "MyProject";
			fakeSolution.FakeActiveMSBuildProject = project;
			var fakeProject = fakeSolution.AddFakeProjectToReturnFromGetProject("MyProject");
			CreateSelectedProjects();
				
			NoProjectsSelected();
			
			var fakePackage = new FakePackage();
			var projects = new List<IPackageManagementSelectedProject>();
			projects.AddRange(selectedProjects.GetProjects(fakePackage));
			
			var expectedProject = new FakeSelectedProject("MyProject");
			var expectedProjects = new List<IPackageManagementSelectedProject>();
			expectedProjects.Add(expectedProject);
			
			SelectedProjectCollectionAssert.AreEqual(expectedProjects, projects);
		}
	}
}
