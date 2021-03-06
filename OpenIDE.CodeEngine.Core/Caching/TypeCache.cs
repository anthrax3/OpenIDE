using System;
using System.Collections.Generic;
using System.Linq;
using OpenIDE.CodeEngine.Core.Caching.Search;
using OpenIDE.Core.Caching;
using System.IO;
namespace OpenIDE.CodeEngine.Core.Caching
{
	public class TypeCache : ICacheBuilder, ITypeCache, ICrawlResult
	{
		private List<CachedPlugin> _plugins = new List<CachedPlugin>();
		private List<Project> _projects = new List<Project>();
		private List<ProjectFile> _files = new List<ProjectFile>();
		private List<ICodeReference> _codeReferences = new List<ICodeReference>();
		private List<ISignatureReference> _signatureReferences = new List<ISignatureReference>();

		public List<CachedPlugin> Plugins { get { return _plugins;  } }

		public int ProjectCount { get { return _projects.Count; } }
		public int FileCount { get { return _files.Count(x => x.FileSearch); } }
		public int CodeReferences { get { return _codeReferences.Count(x => x.TypeSearch); } }
		
		public IEnumerable<Project> AllProjects()
		{
			return _projects.ToList();
		}

		public IEnumerable<ProjectFile> AllFiles()
		{
			return _files.ToList();
		}

		public IEnumerable<ICodeReference> AllReferences()
		{
			return _codeReferences.ToList();
		}

		public IEnumerable<ISignatureReference> AllSignatures()
		{
			return _signatureReferences.ToList();
		}

		public List<ICodeReference> Find(string name)
		{
			lock (_codeReferences) {
				return find(name).ToList();
			}
		}

		public List<ICodeReference> Find(string name, int limit)
		{
			lock (_codeReferences) {
				return find(name).Take(limit).ToList();
			}
		}

		private IEnumerable<ICodeReference> find(string name)
		{
			var names = name
				.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);
			return
				_codeReferences
				.Where(x => {
						if (!x.TypeSearch)
							return false;
						var matchPos = -1;
						var full = x.File.ToLower() + x.Signature.ToLower();
						foreach (var search in names) {
							var pos = full.LastIndexOf(search.ToLower());
							if (pos == -1)
								return false;
							if (pos < matchPos)
								return false;
							matchPos = pos;
						}
						return true;
					}
				 )
				.OrderBy(x => nameSort(x.Name, x.Signature, x.File, names));
		}

        public List<FileFindResult> FindFiles(string searchString)
        {
            return new FileFinder(_files.ToList(), _projects.ToList())
            	.Find(searchString).ToList();
        }

        public List<FileFindResult> GetFilesInDirectory(string directory)
        {
            return new HierarchyBuilder(_files.ToList(), _projects.ToList())
            	.GetNextStep(directory).ToList();
        }

        public List<FileFindResult> GetFilesInProject(string project)
        {
            var prj = GetProject(project);
            if (prj == null)
                return new List<FileFindResult>();
            return new HierarchyBuilder(_files.ToList(), _projects.ToList())
            	.GetNextStepInProject(prj).ToList();
        }

        public List<FileFindResult> GetFilesInProject(string project, string path)
        {
            var prj = GetProject(project);
            if (prj == null)
                return new List<FileFindResult>();
            return new HierarchyBuilder(_files.ToList(), _projects.ToList())
            	.GetNextStepInProject(prj, path).ToList();
        }
	
		public bool ProjectExists(Project project)
		{
			lock (_projects) {
				return _projects
					.Exists(x => x.File.Equals(project.File));
			}
		}
		
		public void Add(Project project)
		{
			lock (_projects)
			{
				var existing = _projects.FirstOrDefault(x => x.File == project.File);
				if (existing == null) {
					_projects.Add(project);
					return;
				}
				existing.Update(project.JSON, project.FileSearch);
			}
		}
		
		public Project GetProject(string fullpath)
		{
			lock (_projects)
			{
				return _projects.FirstOrDefault(x => x.File.Equals(fullpath));
			}
		}
		
		public bool FileExists(string file)
		{
			return _files.ToList().Count(x => x.File.Equals(file)) != 0;
		}
		
		public void Invalidate(string file)
		{
			var project = GetProject(file);
			if (project != null) {
				lock (_files) {
					_files.RemoveAll(x => x.Project != null && x.Project.Equals(file));
				}
			}
			else {
				lock (_files) {
					lock (_codeReferences) {
						_files.RemoveAll(x => x.File.Equals(file));
						_codeReferences.RemoveAll(x => x.File.Equals(file));
						_signatureReferences.RemoveAll(x => x.File.Equals(file));
					}
				}
			}
		}
		
		public void Add(ProjectFile file)
		{
			lock (_files) {
				var existing = _files.FirstOrDefault(x => x.File == file.File);
				if (existing == null) {
					_files.Add(file);
					return;
				}
				existing.Update(file.Project, file.FileSearch);
			}
		}
		
		public void Add(ICodeReference reference)
		{
			lock (_codeReferences) {
				add(reference);
			}
		}

		public void Add(IEnumerable<ICodeReference> references)
		{
			lock (_codeReferences) {
				foreach (var reference in references)
					add(reference);
			}
		}

		public void Add(ISignatureReference reference)
		{
			lock (_signatureReferences) {
				_signatureReferences.Add(reference);
			}
		}

		private void add(ICodeReference reference)
		{
			if (_codeReferences.Any(x => x.Is(reference)))
				return;
			_codeReferences.Add(reference);
		}
		
		private int nameSort(string name, string signature, string filename, string[] compareString)
		{
			return new SearchSorter().Sort(name, signature, filename, compareString);
		}
    }
}

